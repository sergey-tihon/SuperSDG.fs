open System
open System.IO
open System.Numerics
open System.Reflection
open Silk.NET.Maths
open Silk.NET.Windowing
open Silk.NET.Input
open Silk.NET.OpenGL
open SuperSDG
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
    let disposables = Collections.Generic.List<IDisposable>()

    let input = window.CreateInput()
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            if key = Key.Escape then window.Close()
        )
    
    let sharedAssets = AssetManager(typeof<AssetManager>.Assembly)
    let assets = AssetManager(Assembly.GetExecutingAssembly())
        
    let gl = GL.GetApi(window)
    disposables.AddRange([input; gl])
    
    let shaderProgram = Shader.Create(gl,
        assets.LoadEmbeddedText "Shader.vert",
        assets.LoadEmbeddedText "Shader.frag")
    let loadTexture name =
        use stream = sharedAssets.LoadEmbeddedStream name
        Texture.Load(gl, stream)
    let texture1 = loadTexture "floor.jpeg"
    let texture2 = loadTexture "wall.jpeg"
    disposables.AddRange([shaderProgram; texture1; texture2])
    
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
    //gl.EnableVertexAttribArray(2u);
    disposables.AddRange([vao; vbo; ebo])
                            
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
        
        shaderProgram.Use()
        shaderProgram.SetUniform("texture1", 0)
        shaderProgram.SetUniform("texture2", 1)
        let blend = float32 <| Math.Sin(window.Time) / 2.0 + 0.5
        shaderProgram.SetUniform("blend", blend)
        
        let model = {
            Transform.Identity with
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.degreesToRadians -55f)
        }
        shaderProgram.SetUniform("model", model.ViewMatrix)
        let view = {
            Transform.Identity with
                Position = Vector3(blend - 0.5f, -0.f, -3.f)
        }
        shaderProgram.SetUniform("view", view.ViewMatrix)
        let projection =
            let aspectRatio = float32(window.Size.X) / float32(window.Size.Y)
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/4f, aspectRatio, 0.1f, 100f)
        shaderProgram.SetUniform("projection", projection)
        
        texture1.Bind(TextureUnit.Texture0)
        texture2.Bind(TextureUnit.Texture1)
        vao.Bind()
        //gl.DrawArrays(GLEnum.Triangles, 0, 3u)
        gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
    
    window.add_Closing(fun _ ->
        disposables
        |> Seq.rev
        |> Seq.iter (fun x -> x.Dispose())
    )
)
window.Run();
