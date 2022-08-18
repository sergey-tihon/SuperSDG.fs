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
         0.5f;  0.5f; 0.0f;  // top right
         0.5f; -0.5f; 0.0f;  // bottom right
        -0.5f; -0.5f; 0.0f;  // bottom left
        -0.5f;  0.5f; 0.0f   // top left 
    |]
    let indices = [|  // note that we start from 0!
        0u; 1u; 3u;   // first triangle
        1u; 2u; 3u    // second triangle
    |]
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, vertices)
    let ebo = BufferObject.Create(gl, BufferTargetARB.ElementArrayBuffer, indices)
    
    let vao = VertexArrayObject.Create(gl, vbo, ebo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 3u, 0)
                            
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)

        shaderProgram.Use()
        vao.Bind()
        gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
)
window.Run();
