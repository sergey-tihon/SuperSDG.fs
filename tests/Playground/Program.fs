open System
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

let window = Window.Create options
window.add_Load(fun _ ->
    let disposables = Collections.Generic.List<IDisposable>()

    let mutable camera = {
        Camera.Default with
            Position = Vector3(0.0f, 0.0f,  3.0f)
            MouseSensitivity = 0.1f
            AspectRatio = float32(window.Size.X) / float32(window.Size.Y)
    }
    
    let input = window.CreateInput()
    let primaryKeyboard = input.Keyboards.Item(0)
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            match key with 
            | Key.Escape -> window.Close()
            | Key.F ->
                window.WindowState <-
                    if window.WindowState = WindowState.Fullscreen
                    then WindowState.Normal else WindowState.Fullscreen
            | _ -> ()
        )
        
    let mutable lastMousePosition = None    
    for mice in input.Mice do
        mice.add_MouseMove(fun mouse position ->
            match lastMousePosition with
            | None -> lastMousePosition <- Some position
            | Some(lastPosition) ->
                let delta = position - lastPosition
                camera <- camera.ProcessMouseMovement(delta.X, delta.Y)
                lastMousePosition <- Some(position)
        )
        mice.add_Scroll(fun mouse scrollWheel ->
            camera <- camera.ProcessMouseScroll(scrollWheel.Y)
        )
    
        
    let gl = GL.GetApi(window)
    let sharedAssets = AssetManager(gl, typeof<AssetManager>.Assembly)
    let assets = AssetManager(gl, Assembly.GetExecutingAssembly())

    gl.Enable(GLEnum.DepthTest)
    disposables.AddRange([input; gl])
    
    let shaderProgram = assets.LoadShaderProgram("Shader.vert", "Shader.frag")
    let texture1 = sharedAssets.LoadTexture "floor.jpeg"
    let texture2 = sharedAssets.LoadTexture "wall.jpeg"
    disposables.AddRange([shaderProgram; texture1; texture2])
    
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
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, MapRenderer.Data.cubeVertices)
    
    let vao = VertexArrayObject.Create(gl, vbo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
    vao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3)
    //gl.EnableVertexAttribArray(2u);
    disposables.AddRange([vao; vbo])
    
    shaderProgram.Use()
    shaderProgram.SetUniform("texture1", 0)
    shaderProgram.SetUniform("texture2", 1)
                            
    window.add_Update(fun deltaTime ->
        if primaryKeyboard.IsKeyPressed(Key.W) then
            camera <- camera.ProcessKeyboard(CameraMovement.Forward, float32 deltaTime)
        if primaryKeyboard.IsKeyPressed(Key.S) then
            camera <- camera.ProcessKeyboard(CameraMovement.Backward, float32 deltaTime)
        if primaryKeyboard.IsKeyPressed(Key.A) then
            camera <- camera.ProcessKeyboard(CameraMovement.Left, float32 deltaTime)
        if primaryKeyboard.IsKeyPressed(Key.D) then
            camera <- camera.ProcessKeyboard(CameraMovement.Right, float32 deltaTime)
    )                        
    window.add_Render(fun deltaTime ->
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
        
        texture1.Bind(TextureUnit.Texture0)
        texture2.Bind(TextureUnit.Texture1)
        shaderProgram.Use()
        
        shaderProgram.SetUniform("view", camera.GetViewMatrix())
        shaderProgram.SetUniform("projection", camera.GetProjectionMatrix())
        
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
    
    window.add_Resize(fun size ->
        let aspectRation = float32(size.X) / float32(size.Y)
        camera <- { camera with AspectRatio = aspectRation }
    )
    window.add_Closing(fun _ ->
        disposables
        |> Seq.rev
        |> Seq.iter (fun x -> x.Dispose())
    )
)
window.Run();
