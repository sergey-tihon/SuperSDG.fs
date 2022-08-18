namespace SuperSDG.Engine

open System
open Silk.NET.OpenGL

type Shader(gl:GL, handle:uint32) =
    member _.Use() = gl.UseProgram(handle)
    interface IDisposable with
        member _.Dispose() = gl.DeleteProgram(handle)
        
    member _.GetUniformLocation(name:string) =
        gl.GetUniformLocation(handle, name)
        
    static member Create (gl:GL, vertexShaderPath, fragmentShaderPath) =        
        let loadShader (ty:ShaderType) path =
            let src = System.IO.File.ReadAllText path
            let handle = gl.CreateShader(ty)
            gl.ShaderSource(handle, src)
            gl.CompileShader(handle)
            let infoLog = gl.GetShaderInfoLog(handle)
            if not <| String.IsNullOrWhiteSpace(infoLog)
            then failwith $"Error compiling shader of type {ty}, failed with error {infoLog}"
            handle
            
        let handle = gl.CreateProgram()
        let vertex = loadShader ShaderType.VertexShader vertexShaderPath
        let fragment = loadShader ShaderType.FragmentShader fragmentShaderPath
        gl.AttachShader(handle, vertex)
        gl.AttachShader(handle, fragment)
        gl.LinkProgram(handle)
        let status = gl.GetProgram(handle, GLEnum.LinkStatus)
        if status = 0 then failwith $"Program failed to link with error: {gl.GetProgramInfoLog(handle)}"
        gl.DetachShader(handle, vertex)
        gl.DetachShader(handle, fragment)
        gl.DeleteShader(vertex)
        gl.DeleteShader(fragment)
        
        new Shader(gl, handle)
