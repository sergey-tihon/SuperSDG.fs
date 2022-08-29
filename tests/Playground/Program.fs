open System
open System.Numerics
open System.Reflection
open Silk.NET.Maths
open Silk.NET.OpenGL.Extensions.ImGui
open Silk.NET.Windowing
open Silk.NET.Input
open Silk.NET.OpenGL
open SuperSDG
open SuperSDG.Engine
open ImGuiNET

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
            Position = Vector3(-3f, -2.6f, 4.0f)
            Pitch = 21f
            Yaw = 320f
            MouseSensitivity = 0.1f
            MovementSpeed = 5f
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
            if mouse.IsButtonPressed(MouseButton.Left) then
                match lastMousePosition with
                | None -> lastMousePosition <- Some position
                | Some(lastPosition) ->
                    let delta = position - lastPosition
                    camera <- camera.ProcessMouseMovement(delta.X, delta.Y)
                    lastMousePosition <- Some(position)
            else lastMousePosition <- None
        )
        mice.add_Scroll(fun mouse scrollWheel ->
            if mouse.IsButtonPressed(MouseButton.Left) then
                camera <- camera.ProcessMouseScroll(scrollWheel.Y)
        )
    
        
    let gl = GL.GetApi(window)
    let imGuiController = new ImGuiController(gl, window, input)
    let sharedAssets = AssetManager(gl, typeof<AssetManager>.Assembly)
    let assets = AssetManager(gl, Assembly.GetExecutingAssembly())

    gl.Enable(GLEnum.DepthTest)
    disposables.AddRange([input; gl; imGuiController])
    
    let cubeShader = assets.LoadShaderProgram("cube.vert", "cube.frag")
    let lightShader = assets.LoadShaderProgram("light.vert", "light.frag")
    let containerTexture = sharedAssets.LoadTexture "container.png"
    let containerSpecularTexture = sharedAssets.LoadTexture "container_specular.png"
    disposables.AddRange([cubeShader; lightShader; containerTexture; containerSpecularTexture])
    
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, MapRenderer.Data.cubeVerticesWithNorm)
   
    let vao = VertexArrayObject.Create(gl, vbo)
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)
    vao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 8u, 3)
    vao.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, 8u, 6)
    
    let lightVao = VertexArrayObject.Create(gl, vbo)
    lightVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)

    disposables.AddRange([vao; vbo; lightVao])
    
    let cubePositions = [|
        Vector3( 0.0f,  0.0f,  0.0f)
        Vector3( 2.0f,  5.0f, -15.0f)
        Vector3(-1.5f, -2.2f, -2.5f)
        Vector3(-3.8f, -2.0f, -12.3f)
        Vector3( 2.4f, -0.4f, -3.5f)
        Vector3(-1.7f,  3.0f, -7.5f)
        Vector3( 1.3f, -2.0f, -2.5f)
        Vector3( 1.5f,  2.0f, -2.5f)
        Vector3( 1.5f,  0.2f, -1.5f)
        Vector3(-1.3f,  1.0f, -1.5f)
    |]
    let pointLightPositions = [|
        Vector3( 0.7f,  0.2f,  2.0f)
        Vector3( 2.3f, -3.3f, -4.0f)
        Vector3(-4.0f,  2.0f, -12.0f)
        Vector3( 0.0f,  0.0f, -3.0f)
    |]  
    
    let rec renderGui =
        let mutable demoWindowVisible = false
        
        fun () ->
            if demoWindowVisible then
                ImGui.ShowDemoWindow()
            
            ImGui.Begin("App Info")  |> ignore
            if ImGui.Button("Show Demo window") then
                demoWindowVisible <- not demoWindowVisible
            if ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen) then
                ImGui.Text($"Position = %A{camera.Position}");
                ImGui.Text($"Pitch = {camera.Pitch}");
                ImGui.Text($"Yaw = {camera.Yaw}")
                ImGui.Text($"Zoom = {camera.Zoom}")
                
            if ImGui.CollapsingHeader("Window", ImGuiTreeNodeFlags.DefaultOpen) then
                ImGui.Text($"FPS = %A{window.FramesPerSecond}")
                ImGui.Text($"UPS = %A{window.UpdatesPerSecond}")
                ImGui.Text($"Size = %A{window.Size}")
                ImGui.Text($"VideoMode.Resolution = %A{window.VideoMode.Resolution}")
                ImGui.Text($"VideoMode.RefreshRate = %A{window.VideoMode.RefreshRate}")
                ImGui.Text($"API = %A{window.API.API}")
                ImGui.Text($"API.Profile = %A{window.API.Profile}")
                ImGui.Text($"API.Version = {window.API.Version.MajorVersion}.{window.API.Version.MinorVersion}")
                ImGui.Text($"Monitor = %A{window.Monitor.Name}")
                ImGui.Text($"VSync = %A{window.VSync}")

            ImGui.End()
            imGuiController.Render()
                            
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
        imGuiController.Update(float32 deltaTime)

        gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        //gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line)
                
        let icam = camera :> ICamera
        cubeShader.Use()
        cubeShader.SetUniform("material.diffuse", 0)
        containerTexture.Bind(TextureUnit.Texture0)
        cubeShader.SetUniform("material.specular", 1)
        containerSpecularTexture.Bind(TextureUnit.Texture1)
        cubeShader.SetUniform("material.shininess", 32.0f)
       
        // directional light
        cubeShader.SetUniform("dirLight.direction", Vector3(-0.2f, -1.0f, -0.3f))
        cubeShader.SetUniform("dirLight.ambient", Vector3(0.05f, 0.05f, 0.05f))
        cubeShader.SetUniform("dirLight.diffuse", Vector3(0.4f, 0.4f, 0.4f))
        cubeShader.SetUniform("dirLight.specular", Vector3(0.5f, 0.5f, 0.5f))
        
        // point lights
        pointLightPositions |> Array.iteri (fun i pos ->
            let lightName = $"pointLights[{i}]"
            cubeShader.SetUniform($"{lightName}.position", pos)
            cubeShader.SetUniform($"{lightName}.ambient", Vector3(0.05f, 0.05f, 0.05f))
            cubeShader.SetUniform($"{lightName}.diffuse", Vector3(0.8f, 0.8f, 0.8f))
            cubeShader.SetUniform($"{lightName}.specular", Vector3(1.0f, 1.0f, 1.0f))
            cubeShader.SetUniform($"{lightName}.constant", 1.0f)
            cubeShader.SetUniform($"{lightName}.linear", 0.09f)
            cubeShader.SetUniform($"{lightName}.quadratic", 0.032f)
        )
        
        // spotLight
        cubeShader.SetUniform("spotLight.position", camera.Position)
        cubeShader.SetUniform("spotLight.direction", camera.Front)
        cubeShader.SetUniform("spotLight.ambient", Vector3(0.0f, 0.0f, 0.0f))
        cubeShader.SetUniform("spotLight.diffuse", Vector3(1.0f, 1.0f, 1.0f))
        cubeShader.SetUniform("spotLight.specular", Vector3(1.0f, 1.0f, 1.0f))
        cubeShader.SetUniform("spotLight.constant", 1.0f)
        cubeShader.SetUniform("spotLight.linear", 0.09f)
        cubeShader.SetUniform("spotLight.quadratic", 0.032f)
        cubeShader.SetUniform("spotLight.cutOff", cos(MathH.radians(12.5f)))
        cubeShader.SetUniform("spotLight.outerCutOff", cos(MathH.radians(15.0f)))
        
        cubeShader.SetUniform("viewPos", camera.Position)
        cubeShader.SetUniform("view", icam.GetViewMatrix())
        cubeShader.SetUniform("projection", icam.GetProjectionMatrix())
        
        vao.Bind()
        cubePositions |> Array.iteri (fun i position ->
            let model = {
                Transform.Identity with
                    Position = position
                    Rotation = Quaternion.CreateFromAxisAngle(Vector3(1.0f, 0.3f, 0.5f), float32 (i * 20))
            }
            cubeShader.SetUniform("model", model.ViewMatrix)
            gl.DrawArrays(GLEnum.Triangles, 0, 36u)
        )
        
        lightShader.Use()
        lightShader.SetUniform("view", icam.GetViewMatrix())
        lightShader.SetUniform("projection", icam.GetProjectionMatrix())
        
        vao.Bind()
        pointLightPositions |> Seq.iter (fun position ->
            let model = {
                Transform.Identity with
                    Position = position
                    Scale = 0.2f
            }
            lightShader.SetUniform("model", model.ViewMatrix)
            gl.DrawArrays(GLEnum.Triangles, 0, 36u)
        )
        
        renderGui()
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
