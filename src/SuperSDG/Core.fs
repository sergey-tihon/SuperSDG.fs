namespace SuperSDG.Core

open System
open System.IO
open System.Numerics;
open System.Runtime.CompilerServices
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

    member this.VertexAttributePointer(index:uint, count:int, ty:VertexAttribPointerType, vertexSize:uint, offSet) =
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
    member _.SetUniform(name:string, value:int) =
        let location = gl.GetUniformLocation(handle, name)
        if location = -1 then failwith $"{name} uniform not found on shader."
        gl.Uniform1(location, value)
    member _.SetUniform(name:string, value:Matrix4x4) =
        let location = gl.GetUniformLocation(handle, name)
        if location = -1 then failwith $"{name} uniform not found on shader."
        //gl.UniformMatrix4(location, 1u, false, &Unsafe.As<_,float32>(&Unsafe.AsRef(&value)))
        gl.UniformMatrix4(location, 1u, false, &value.M11) // TODO: :( hack for F# suntax
    member _.SetUniform(name:string, value:float32) =
        let location = gl.GetUniformLocation(handle, name)
        if location = -1 then failwith $"{name} uniform not found on shader."
        gl.Uniform1(location, value)
    member _.SetUniform(name:string, value:Vector3) =
        let location = gl.GetUniformLocation(handle, name)
        if location = -1 then failwith $"{name} uniform not found on shader."
        gl.Uniform3(location, value.X, value.Y, value.Z)
    interface IDisposable with
        member this.Dispose() = gl.DeleteProgram(handle)

type Texture(gl:GL, path: string) as self =
    let handle = gl.GenTexture()
    let setParameters() =
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int GLEnum.ClampToEdge)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int GLEnum.ClampToEdge)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int GLEnum.LinearMipmapLinear)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int GLEnum.Linear)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8)
        gl.GenerateMipmap(TextureTarget.Texture2D)
        
    do
        self.Bind()
        do
            use img = Image.Load<Rgba32>(path)
            let width, height = uint32 <| img.Width, uint32 <| img.Height
            gl.TexImage2D(TextureTarget.Texture2D, 0, int InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero.ToPointer())
            img.ProcessPixelRows(fun accessor ->
                for y in [0..accessor.Height-1] do
                    let data = &&accessor.GetRowSpan(y).GetPinnableReference()
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, width, 1u, PixelFormat.Rgba, PixelType.UnsignedByte, NativePtr.toVoidPtr data);
            )
        setParameters()

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
        degrees * MathF.PI / 180f
        
type Camera =
    {
        Position : Vector3
        Front : Vector3
        Up : Vector3
        AspectRatio : float32
        Yaw : float32
        Pitch : float32
        Zoom : float32
    }
    static member Default =
        {
            Position = Vector3.Zero
            Front = Vector3.Zero
            Up = Vector3.Zero
            AspectRatio = 0f
            Yaw = -90f
            Pitch = 0f
            Zoom = 45f
        }
    member this.ModifyZoom(zoomAmount) =
        //We don't want to be able to zoom in too close or too far away so clamp to these values
        { this with Zoom = Math.Clamp(this.Zoom - zoomAmount, 1.0f, 45f) }
    member this.ModifyDirection(xOffset:float32, yOffset:float32) =
        let yaw = this.Yaw + xOffset
        let pitch = this.Pitch - yOffset
        //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
        let pitch = Math.Clamp(pitch, -89f, 89f)
        let cameraDirection = Vector3(
            MathF.Cos(MathHelper.degreesToRadians(yaw)) * MathF.Cos(MathHelper.degreesToRadians(pitch)),
            MathF.Sin(MathHelper.degreesToRadians(pitch)),
            MathF.Sin(MathHelper.degreesToRadians(yaw)) * MathF.Cos(MathHelper.degreesToRadians(pitch));
        )
        { this with Yaw = yaw; Pitch = pitch; Front = Vector3.Normalize(cameraDirection) }
    member this.GetViewMatrix() =
        Matrix4x4.CreateLookAt(this.Position, this.Position + this.Front, this.Up)
    member this.GetProjectionMatrix() =
        Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.degreesToRadians(this.Zoom), this.AspectRatio, 0.1f, 100.0f)
