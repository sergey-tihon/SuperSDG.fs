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
        FreeCamera.Default with
            Position = Vector3(1.1f, 2.3f, 4.0f)
            Pitch = -30f
            Yaw = 260f
            MouseSensitivity = 0.1f
            AspectRatio = float32(window.Size.X) / float32(window.Size.Y)
    }
    let lightPosition = Vector3(1.2f, 1.0f, 2.0f)
    
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
    
    let cubeShader = assets.LoadShaderProgram("cube.vert", "cube.frag")
    let lightShader = assets.LoadShaderProgram("light.vert", "light.frag")
    disposables.AddRange([cubeShader; lightShader])
    
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, MapRenderer.Data.cubeVertices)
   
    let vao = VertexArrayObject.Create(gl, vbo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
    
    let lightVao = VertexArrayObject.Create(gl, vbo)
    lightVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)

    disposables.AddRange([vao; vbo; lightVao])
                            
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
        
        let icam = camera :> ICamera
        cubeShader.Use()
        cubeShader.SetUniform("objectColor", Vector3(1.0f, 0.5f, 0.31f))
        cubeShader.SetUniform("lightColor", Vector3(1.0f, 1.0f, 1.0f))
        cubeShader.SetUniform("view", icam.GetViewMatrix())
        cubeShader.SetUniform("projection", icam.GetProjectionMatrix())
        cubeShader.SetUniform("model", Matrix4x4.Identity)
    
        vao.Bind()
        gl.DrawArrays(GLEnum.Triangles, 0, 36u)
        
        lightShader.Use()
        lightShader.SetUniform("view", icam.GetViewMatrix())
        lightShader.SetUniform("projection", icam.GetProjectionMatrix())
        let model = {
            Transform.Identity with
                Position = lightPosition
                Scale = 0.2f
        }
        lightShader.SetUniform("model", model.ViewMatrix)
        
        vao.Bind()
        gl.DrawArrays(GLEnum.Triangles, 0, 36u)
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
