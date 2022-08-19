open System
open System.IO
open Silk.NET.Maths
open Silk.NET.Windowing
open Silk.NET.Input
open Silk.NET.OpenGL
open SuperSDG.Engine

let mutable options = WindowOptions.Default
options.Title <- "Playground"
options.Size <- Vector2D(1000, 1000)
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
    let texture1 = Texture.Load(gl, "Resources/floor.jpeg")
    let texture2 = Texture.Load(gl, "Resources/wall.jpeg")
    
    let vertices = [|
         // positions           // colors           // texture coords
         0.5f;  0.5f; 0.0f;     1.0f; 0.0f; 0.0f;   1.0f; 1.0f;
         0.5f; -0.5f; 0.0f;     0.0f; 1.0f; 0.0f;   1.0f; 0.0f;
        -0.5f; -0.5f; 0.0f;     1.0f; 0.0f; 0.0f;   0.0f; 0.0f;
        -0.5f;  0.5f; 0.0f;     0.0f; 0.0f; 1.0f;   0.0f; 1.0f;
    |]
    let indices = [|  // note that we start from 0!
        0u; 1u; 3u;   // first triangle
        1u; 2u; 3u    // second triangle
    |]
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, vertices)
    let ebo = BufferObject.Create(gl, BufferTargetARB.ElementArrayBuffer, indices)
    
    let vao = VertexArrayObject.Create(gl, vbo, ebo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)
    vao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 8u, 3)
    vao.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, 8u, 6)
    gl.EnableVertexAttribArray(2u);
                            
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
        
        shaderProgram.Use()
        shaderProgram.SetInt("texture1",0)
        shaderProgram.SetInt("texture2",1)
        
        let greenValue = float32 <| (Math.Sin(window.Time) / 2.0) + 0.5
        let vertexColorLocation = shaderProgram.GetUniformLocation("ourColor")
        gl.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f)
        
        let dxLocation = shaderProgram.GetUniformLocation("dx")
        gl.Uniform1(dxLocation, greenValue - 0.5f)

        texture1.Bind(TextureUnit.Texture0)
        texture2.Bind(TextureUnit.Texture1)
        vao.Bind()
        //gl.DrawArrays(GLEnum.Triangles, 0, 3u)
        gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
)
window.Run();
