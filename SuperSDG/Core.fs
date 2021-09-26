namespace SuperSDG.Core

open System
open System.IO
open Microsoft.FSharp.NativeInterop
open Silk.NET.OpenGL

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
        
    interface IDisposable with
        member this.Dispose() = gl.DeleteProgram(handle)