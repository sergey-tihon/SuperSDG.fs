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
options.Size <- Vector2D(1280, 720)
options.PreferredDepthBufferBits <- 24
options.PreferredStencilBufferBits <- 8
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

    let mutable camera = {
        Camera2.Default with
            Position = Vector3(0.0f, 0.0f,  3.0f)
    }
    
    let input = window.CreateInput()
    let pressedKeys = Collections.Generic.Dictionary<Key, float32 -> unit>()
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            match key with 
            | Key.Escape -> window.Close()
            | Key.F ->
                window.WindowState <-
                    if window.WindowState = WindowState.Fullscreen
                    then WindowState.Normal else WindowState.Fullscreen
            | Key.W | Key.A | Key.S | Key.D ->
                let moveDirection =
                     match key with
                     | Key.W -> CameraMovement.Forward
                     | Key.S -> CameraMovement.Backward
                     | Key.A -> CameraMovement.Left
                     | Key.D -> CameraMovement.Right
                pressedKeys.Add(key, fun deltaTime ->
                    camera <- camera.ProcessKeyboard(moveDirection, deltaTime)
                )
            | _ -> ()
        )
        keyboard.add_KeyUp(fun keyboard key _ ->
            pressedKeys.Remove(key) |> ignore
        )
    
    let sharedAssets = AssetManager(typeof<AssetManager>.Assembly)
    let assets = AssetManager(Assembly.GetExecutingAssembly())
        
    let gl = GL.GetApi(window)
    gl.Enable(GLEnum.DepthTest)
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
        -0.5f; -0.5f; -0.5f;  0.0f; 0.0f;
         0.5f; -0.5f; -0.5f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 0.0f;

        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;

        -0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f;  0.5f;  0.5f;  1.0f; 0.0f;

         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;

        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  1.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;

        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f
    |]
    let cubePositions = [|
        Vector3( 0.0f,  0.0f,  0.0f);
        Vector3( 2.0f,  5.0f, -15.0f);
        Vector3(-1.5f, -2.2f, -2.5f);
        Vector3(-3.8f, -2.0f, -12.3f);
        Vector3( 2.4f, -0.4f, -3.5f);
        Vector3(-1.7f,  3.0f, -7.5f);
        Vector3( 1.3f, -2.0f, -2.5f);
        Vector3( 1.5f,  2.0f, -2.5f);
        Vector3( 1.5f,  0.2f, -1.5f);
        Vector3(-1.3f,  1.0f, -1.5f)
    |]
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, vertices)
    
    let vao = VertexArrayObject.Create(gl, vbo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
    vao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3)
    //gl.EnableVertexAttribArray(2u);
    disposables.AddRange([vao; vbo])
    
    shaderProgram.Use()
    shaderProgram.SetUniform("texture1", 0)
    shaderProgram.SetUniform("texture2", 1)
                            
    window.add_Render(fun deltaTime ->
        pressedKeys.Values
        |> Seq.iter (fun f -> f(float32 deltaTime))
        
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
        
        texture1.Bind(TextureUnit.Texture0)
        texture2.Bind(TextureUnit.Texture1)
        shaderProgram.Use()
        
        shaderProgram.SetUniform("view", camera.GetViewMatrix())
        let projection =
            let aspectRatio = float32(window.Size.X) / float32(window.Size.Y)
            camera.GetProjectionMatrix(aspectRatio)
        shaderProgram.SetUniform("projection", projection)
        
        vao.Bind()
        
        cubePositions
        |> Seq.iteri(fun i cubePos ->
            let blend = Math.Sin(window.Time + float(i)) / 2.0 + 0.5
            shaderProgram.SetUniform("blend", float32 blend)
            
            let angle = window.Time * 10. * float(i+1)
            let model = {
                Transform.Identity with
                    Position = cubePos
                    Rotation = Quaternion.CreateFromAxisAngle(
                        Vector3(1.0f, 0.3f, 0.5f),
                        MathH.radians(float32 angle))
            }
            shaderProgram.SetUniform("model", model.ViewMatrix)
            gl.DrawArrays(GLEnum.Triangles, 0, 36u)
        )
        //gl.DrawElements(GLEnum.Triangles, 6u, GLEnum.UnsignedInt, IntPtr.Zero.ToPointer())
    )
    
    window.add_Closing(fun _ ->
        disposables
        |> Seq.rev
        |> Seq.iter (fun x -> x.Dispose())
    )
)
window.Run();
