open System
open System.IO
open Silk.NET.Windowing
open Silk.NET.Input
open Silk.NET.OpenGL
open SuperSDG.Engine

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
    let shaderProgram = Shader.Create(gl, "Resources/Shader.vert", "Resources/Shader.frag")
    
    let vertices = [|
         0.5f;  0.5f; 0.0f;     1.0f; 0.0f; 0.0f;
         0.5f; -0.5f; 0.0f;     0.0f; 1.0f; 0.0f;
        -0.5f; -0.5f; 0.0f;     1.0f; 0.0f; 0.0f;
        -0.5f;  0.5f; 0.0f;     0.0f; 0.0f; 1.0f;
    |]
    let indices = [|  // note that we start from 0!
        0u; 1u; 3u;   // first triangle
        1u; 2u; 3u    // second triangle
    |]
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, vertices)
    let ebo = BufferObject.Create(gl, BufferTargetARB.ElementArrayBuffer, indices)
    
    let vao = VertexArrayObject.Create(gl, vbo, ebo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 6u, 0)
    vao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 6u, 3)
                            
    let mutable timeValue = 0.0                        
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
        
        shaderProgram.Use()
        
        timeValue <- timeValue + deltaTime
        let greenValue = float32 <| (Math.Sin(timeValue) / 2.0) + 0.5
        let vertexColorLocation = shaderProgram.GetUniformLocation("ourColor")
        gl.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f)
        
        let dxLocation = shaderProgram.GetUniformLocation("dx")
        gl.Uniform1(dxLocation, greenValue - 0.5f)

        vao.Bind()
        //gl.DrawArrays(GLEnum.Triangles, 0, 3u)
        gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
)
window.Run();
