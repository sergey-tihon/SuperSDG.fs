open System
open System.Numerics
open System.Reflection
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG
open SuperSDG.Engine

let mutable options = WindowOptions.Default
options.Size <- Vector2D<int>(1280, 720)
options.PreferredDepthBufferBits <- 24
options.Title <- "SuperSDG 3"

let window = Window.Create options
window.add_Load(fun _ ->
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

    //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
    let mutable camera =
        { FollowCamera.Default with
            CameraTarget = Vector3(20.5f, 0.5f, -20.5f)
            Distance = 15.0f
            Pitch = -45f
            Yaw = -60f
            AspectRatio = float32(window.Size.X) / float32(window.Size.Y)}

    let mutable lastMousePosition = None
    for mice in input.Mice do
        //mice.Cursor.CursorMode <- CursorMode.Raw
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
    let assets = AssetManager(gl, Assembly.GetExecutingAssembly())
    gl.Enable(EnableCap.DepthTest)
    gl.DepthFunc(DepthFunction.Less)

    let disposables = Collections.Generic.List<IDisposable>()
    disposables.AddRange([input; gl])

    let map = MapGenerator.createMap 40 (DateTime.Now.Ticks |> int)
    let renderer = new MapRenderer.WorldRenderer(gl, assets)
    disposables.AddRange([renderer])

    // window.add_Update(fun deltaTime ->
    //     if primaryKeyboard.IsKeyPressed(Key.W) then
    //         camera <- camera.ProcessKeyboard(CameraMovement.Forward, float32 deltaTime)
    //     if primaryKeyboard.IsKeyPressed(Key.S) then
    //         camera <- camera.ProcessKeyboard(CameraMovement.Backward, float32 deltaTime)
    //     if primaryKeyboard.IsKeyPressed(Key.A) then
    //         camera <- camera.ProcessKeyboard(CameraMovement.Left, float32 deltaTime)
    //     if primaryKeyboard.IsKeyPressed(Key.D) then
    //         camera <- camera.ProcessKeyboard(CameraMovement.Right, float32 deltaTime)
    // )

    window.add_Render(fun deltaTime ->
        gl.Enable(EnableCap.DepthTest)
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        renderer.Render(map, camera) 
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
