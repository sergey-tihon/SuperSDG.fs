open System
open System.Numerics
open System.Reflection
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG
open SuperSDG.Engine
open SuperSDG.MapGenerator

let mutable options = WindowOptions.Default
options.Size <- Vector2D<int>(1280, 720)
options.PreferredDepthBufferBits <- 24
options.Title <- "SuperSDG 3"

let window = Window.Create options
window.add_Load(fun _ ->
    let mutable map = MapGenerator.createMap 40 (DateTime.Now.Ticks |> int)
    let mutable camera =
        { FollowCamera.Default with
            CameraTarget = MapRenderer.to3D map.Player
            Distance = 15.0f
            Pitch = -45f
            Yaw = -60f
            AspectRatio = float32(window.Size.X) / float32(window.Size.Y)
        }
    
    let input = window.CreateInput()
    let primaryKeyboard = input.Keyboards[0]
    let isCameraControlMode() =
        primaryKeyboard.IsKeyPressed(Key.ControlLeft)
        || primaryKeyboard.IsKeyPressed(Key.ControlRight)
        
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            match key with 
            | Key.Escape -> window.Close()
            | Key.F ->
                window.WindowState <-
                    if window.WindowState = WindowState.Fullscreen
                    then WindowState.Normal else WindowState.Fullscreen
            | Key.Up | Key.Down | Key.Left | Key.Right ->
                let direction =
                    match key with
                    | Key.Up -> Forward
                    | Key.Down -> Backward
                    | Key.Left -> Left
                    | Key.Right -> Right
                    | _ -> failwith "Impossible"
                let directionVector =
                    Mover.getDirection camera.Front direction
                let player' = map.Player + directionVector
                if map.Map[player'.X, player'.Y] = '.' then
                    map <- { map with Player = player' }
                    camera <- { camera with CameraTarget = MapRenderer.to3D map.Player }
            | _ -> ()
        )

    let mutable lastMousePosition = None
    for mice in input.Mice do
        //mice.Cursor.CursorMode <- CursorMode.Raw
        mice.add_MouseMove(fun mouse position ->
            if isCameraControlMode() then
                match lastMousePosition with
                | None -> lastMousePosition <- Some position
                | Some(lastPosition) ->
                    let delta = position - lastPosition
                    camera <- camera.ProcessMouseMovement(delta.X, delta.Y)
                    lastMousePosition <- Some(position)
            else
                lastMousePosition <- None
        )
        mice.add_Scroll(fun mouse scrollWheel ->
            if isCameraControlMode() then
                camera <- camera.ProcessMouseScroll(scrollWheel.Y)
        )

    let gl = GL.GetApi(window)
    let assets = AssetManager(gl, Assembly.GetExecutingAssembly())
    gl.Enable(EnableCap.DepthTest)
    gl.DepthFunc(DepthFunction.Less)

    let disposables = Collections.Generic.List<IDisposable>()
    disposables.AddRange([input; gl])

    let renderer = new MapRenderer.WorldRenderer(gl, assets)
    disposables.AddRange([renderer])

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
