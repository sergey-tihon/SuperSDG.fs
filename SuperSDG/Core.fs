namespace SuperSDG.Core

open System
open System.IO
open System.Numerics;
open Microsoft.FSharp.NativeInterop
open Silk.NET.OpenGL
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

#nowarn "9"

type BufferObject<'TDataType when 'TDataType : unmanaged>
    (gl:GL, data:'TDataType[], bufferType:BufferTargetARB) as self =
    let handle = gl.GenBuffer()
    do  self.Bind()
        use ptr = fixed data
        let size = unativeint <| (data.Length * sizeof<'TDataType>)
        gl.BufferData(bufferType, size , NativePtr.toVoidPtr ptr, BufferUsageARB.StaticDraw)
    member _.Bind() = gl.BindBuffer(bufferType, handle) 
    interface IDisposable with
        member this.Dispose() = gl.DeleteBuffer(handle)
        
type VertexArrayObject<'TVertexType, 'TIndexType when 'TVertexType : unmanaged and 'TIndexType : unmanaged>
    (gl:GL, vbo:BufferObject<'TVertexType>, ebo:BufferObject<'TIndexType>) as self =
    let handle = gl.GenVertexArray()
    do  self.Bind()
        vbo.Bind()
        ebo.Bind()

    member this.VertexAttributePointer(index:uint, count:int, ty:VertexAttribPointerType, vertexSize, offSet) =
        let stride = uint32 <| vertexSize * (uint) sizeof<'TVertexType>
        let offsetPtr = IntPtr(offSet * sizeof<'TVertexType>).ToPointer()
        gl.VertexAttribPointer(index, count, ty, false, stride, offsetPtr);
        gl.EnableVertexAttribArray(index)
    member _.Bind() = gl.BindVertexArray(handle)
    interface IDisposable with
        member this.Dispose() = gl.DeleteVertexArray(handle)
        
type Shader(gl:GL, vertexPath, fragmentPath) =
    let handle = gl.CreateProgram()
    do
        let loadShader (ty:ShaderType) path =
            let src = File.ReadAllText path
            let handle = gl.CreateShader(ty)
            gl.ShaderSource(handle, src)
            gl.CompileShader(handle)
            let infoLog = gl.GetShaderInfoLog(handle)
            if not <| String.IsNullOrWhiteSpace(infoLog)
            then failwith $"Error compiling shader of type {ty}, failed with error {infoLog}"
            handle
        let vertex = loadShader ShaderType.VertexShader vertexPath
        let fragment = loadShader ShaderType.FragmentShader fragmentPath
        gl.AttachShader(handle, vertex)
        gl.AttachShader(handle, fragment)
        gl.LinkProgram(handle)
        let status = gl.GetProgram(handle, GLEnum.LinkStatus)
        if status = 0 then failwith $"Program failed to link with error: {gl.GetProgramInfoLog(handle)}"
        gl.DetachShader(handle, vertex)
        gl.DetachShader(handle, fragment)
        gl.DeleteShader(vertex)
        gl.DeleteShader(fragment)
    member _.Use() = gl.UseProgram(handle)
    member _.SetUniform(name:string, value:float32) =
        let location = gl.GetUniformLocation(handle, name)
        if location = -1 then failwith $"{name} uniform not found on shader."
        gl.Uniform1(location, value)
    member _.SetUniform(name:string, value:Matrix4x4) =
        let location = gl.GetUniformLocation(handle, name);
        if location = -1 then failwith $"{name} uniform not found on shader."
        gl.UniformMatrix4(location, 1u, false, &value.M11) // :(

    interface IDisposable with
        member this.Dispose() = gl.DeleteProgram(handle)

type Texture(gl:GL, path: string) as self =
    let handle = gl.GenTexture()
    do  use img : Image<Rgba32> = Image.Load(path);
        img.Mutate(fun x -> x.Flip(FlipMode.Vertical) |> ignore)
        let data = &&img.GetPixelRowSpan(0).GetPinnableReference()
        self.Load(gl, NativePtr.toVoidPtr data, uint <| img.Width, uint <| img.Height)

    member this.Load(gl:GL, data: voidptr, width:uint, height:uint) =
        this.Bind()
        gl.TexImage2D(TextureTarget.Texture2D, 0, int InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        //Setting some texture parameters so the texture behaves as expected.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int GLEnum.Linear);
        //Generating mipmaps.
        gl.GenerateMipmap(TextureTarget.Texture2D);

    member this.Bind(?textureSlot: TextureUnit) =
        let textureSlot' = defaultArg textureSlot TextureUnit.Texture0
        //When we bind a texture we can choose which textureslot we can bind it to.
        gl.ActiveTexture(textureSlot');
        gl.BindTexture(TextureTarget.Texture2D, handle);

    interface IDisposable with
        member this.Dispose() = gl.DeleteTexture(handle);
    
type Transform =
    {
        Position : Vector3
        Scale : float32
        Rotation: Quaternion
    }
    static member Identity =
        {
            Position = Vector3(0f, 0f, 0f)
            Scale = 1f
            Rotation = Quaternion.Identity
        }
    member this.ViewMatrix with get () =
        Matrix4x4.Identity
            * Matrix4x4.CreateFromQuaternion(this.Rotation)
            * Matrix4x4.CreateScale(this.Scale)
            * Matrix4x4.CreateTranslation(this.Position)

module MathHelper =
    let degreesToRadians degrees =
        MathF.PI / 180f * degrees