open System.Drawing
open Microsoft.FSharp.NativeInterop
open Silk.NET.Maths
open Silk.NET.Windowing
open Silk.NET.Input
open Silk.NET.OpenGL
open System
open System.IO

let mutable options = WindowOptions.Default
options.Title <- "Playground"
options.API <- GraphicsAPI(
    ContextAPI.OpenGL,
    ContextProfile.Core, // Only modern OpenGL functions
    ContextFlags.ForwardCompatible, // Required for macOS
    APIVersion(3, 3) // Default is 3.3 (4.1 is max version on macOS)
)
//options.WindowState <- WindowState.Fullscreen


let window = Window.Create options
window.add_Load(fun _ ->
    let input = window.CreateInput()
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            if key = Key.Escape then window.Close()
        )
        
    let gl = GL.GetApi(window)
    
    let src = File.ReadAllText "Resources/Shader.vert"
    let vertexShader= gl.CreateShader(GLEnum.VertexShader)
    gl.ShaderSource(vertexShader, src)
    gl.CompileShader(vertexShader)
    let infoLog = gl.GetShaderInfoLog(vertexShader)
    if not <| String.IsNullOrEmpty(infoLog)
    then failwith $"ERROR::SHADER::VERTEX::COMPILATION_FAILED {infoLog}"
    
    let src = File.ReadAllText "Resources/Shader.frag"
    let fragmentShader = gl.CreateShader(GLEnum.FragmentShader)
    gl.ShaderSource(fragmentShader, src)
    gl.CompileShader(fragmentShader)
    let infoLog = gl.GetShaderInfoLog(fragmentShader)
    if not <| String.IsNullOrEmpty(infoLog)
    then failwith $"ERROR::SHADER::FRAGMENT::COMPILATION_FAILED {infoLog}"
    
    let shaderProgram = gl.CreateProgram()
    gl.AttachShader(shaderProgram, vertexShader)
    gl.AttachShader(shaderProgram, fragmentShader)
    gl.LinkProgram(shaderProgram)
    let status = gl.GetProgram(shaderProgram, GLEnum.LinkStatus)
    if status = 0 then failwith $"Program failed to link with error: {gl.GetProgramInfoLog(shaderProgram)}"
    //gl.DetachShader(shaderProgram, vertexShader)
    //gl.DetachShader(shaderProgram, fragmentShader)
    gl.DeleteShader(vertexShader)
    gl.DeleteShader(fragmentShader)
    
    
    let vertices = [|
         0.5f;  0.5f; 0.0f;  // top right
         0.5f; -0.5f; 0.0f;  // bottom right
        -0.5f; -0.5f; 0.0f;  // bottom left
        -0.5f;  0.5f; 0.0f   // top left 
    |]
    let indices = [|  // note that we start from 0!
        0u; 1u; 3u;   // first triangle
        1u; 2u; 3u    // second triangle
    |]
    let vao = gl.GenVertexArray()
    let vbo = gl.GenBuffer()
    let ebo = gl.GenBuffer()
    gl.BindVertexArray vao
    
    gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
    do  use ptr = fixed vertices
        let size = unativeint <| (vertices.Length * sizeof<float32>)
        gl.BufferData(GLEnum.ArrayBuffer, size, NativePtr.toVoidPtr ptr, GLEnum.StaticDraw)
    
    gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo)
    do  use ptr = fixed indices
        let size = unativeint <| (indices.Length * sizeof<uint32>)
        gl.BufferData(GLEnum.ElementArrayBuffer, size, NativePtr.toVoidPtr ptr, GLEnum.StaticDraw)
        
    let stride = uint32 <| 3 * sizeof<float32>
    let offsetPtr = IntPtr.Zero.ToPointer() 
    gl.VertexAttribPointer(0u, 3, GLEnum.Float, false, stride, offsetPtr)
    gl.EnableVertexAttribArray(0u)
        
    gl.BindBuffer(GLEnum.ArrayBuffer, 0u)
    gl.BindVertexArray(0u)
    
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)

        gl.UseProgram shaderProgram
        gl.BindVertexArray vao
        //gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo)
        gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
)
window.Run();
