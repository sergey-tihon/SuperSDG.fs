open System
open System.Numerics
open System.Reflection
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG
open SuperSDG.Engine


let Width, Height = (800, 700)
let Vertices = [|
    //X    Y      Z       Normals             U     V
    -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 0.0f; 1.0f;
    0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 1.0f; 1.0f;
    0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 1.0f; 0.0f;
    0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 1.0f; 0.0f;
    -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 0.0f; 0.0f;
    -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 0.0f; 1.0f;

    -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 0.0f; 1.0f;
    0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 1.0f; 1.0f;
    0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 1.0f; 0.0f;
    0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 1.0f; 0.0f;
    -0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 0.0f; 0.0f;
    -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f; 0.0f; 1.0f;

    -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f; 0.0f; 1.0f;
    -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f; 1.0f; 1.0f;
    -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f; 1.0f; 0.0f;
    -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f; 1.0f; 0.0f;
    -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f; 0.0f; 0.0f;
    -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f; 0.0f; 1.0f;

    0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f; 0.0f; 1.0f;
    0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f; 1.0f; 1.0f;
    0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f; 1.0f; 0.0f;
    0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f; 1.0f; 0.0f;
    0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f; 0.0f; 0.0f;
    0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f; 0.0f; 1.0f;

    -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f; 0.0f; 1.0f;
    0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f; 1.0f; 1.0f;
    0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f; 1.0f; 0.0f;
    0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f; 1.0f; 0.0f;
    -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f; 0.0f; 0.0f;
    -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f; 0.0f; 1.0f;

    -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f; 0.0f; 1.0f;
    0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f; 1.0f; 1.0f;
    0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f; 1.0f; 0.0f;
    0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f; 1.0f; 0.0f;
    -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f; 0.0f; 0.0f;
    -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f; 0.0f; 1.0f
|]
let Indices = [|
    0u; 1u; 3u;
    1u; 2u; 3u
|]

let mutable options = WindowOptions.Default
options.Size <- Vector2D<int>(Width, Height)
options.Title <- "SuperSDG v3"

let window = Window.Create options
window.add_Load(fun _ ->
    let startTime = DateTime.UtcNow

    let input = window.CreateInput()
    let primaryKeyboard = input.Keyboards.Item(0)
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            if key = Key.Escape then window.Close()
        )

    let mutable LastMousePosition = Vector2.Zero
    //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
    let mutable camera =
        { Camera.Default with
            Position = Vector3.Multiply(Vector3.UnitZ, 6f)
            Front = Vector3.Multiply(Vector3.UnitZ, -1f)
            Up = Vector3.UnitY
            AspectRatio = (float32 Width) / (float32 Height) }

    for mice in input.Mice do
        //mice.Cursor.CursorMode <- CursorMode.Raw
        mice.add_MouseMove(fun mouse position ->
            let lookSensitivity = 0.1f
            if LastMousePosition = Vector2.Zero
            then LastMousePosition <- position
            else
                let xOffset = (position.X - LastMousePosition.X) * lookSensitivity
                let yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity
                LastMousePosition <- position

                camera <- camera.ModifyDirection(xOffset, yOffset);
        )
        mice.add_Scroll(fun mouse scrollWheel ->
            camera <- camera.ModifyZoom(scrollWheel.Y);
        )

    let assets = AssetManager(Assembly.GetExecutingAssembly())
    let gl = GL.GetApi(window)
    gl.Enable(EnableCap.DepthTest)
    gl.DepthFunc(DepthFunction.Less)

    let disposables = Collections.Generic.List<IDisposable>()
    disposables.AddRange([input; gl])

    //Instantiating our new abstractions
    let ebo = BufferObject.Create(gl, BufferTargetARB.ElementArrayBuffer, Indices)
    let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Vertices)
    let VaoCube = VertexArrayObject.Create(gl, vbo, ebo)
    disposables.AddRange([vbo; ebo; VaoCube])

    VaoCube.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)
    VaoCube.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 8u, 3)
    VaoCube.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, 8u, 6)

    //The lighting shader will give our main cube it's colour multiplied by the lights intensity
    let lightingShader = Shader.Create(gl, assets.LoadEmbeddedText "Shader.vert", assets.LoadEmbeddedText  "lighting.frag")
    //The Lamp shader uses a fragment shader that just colours it solid white so that we know it is the light source
    let lampShader = Shader.Create(gl, assets.LoadEmbeddedText "Shader.vert", assets.LoadEmbeddedText  "Shader.frag")
    let lampPosition = Vector3(1.2f, 1.0f, 2.0f)

    let loadTexture name =
        use stream = assets.LoadEmbeddedStream name
        Texture.Load(gl, stream)
    let diffuseMap = loadTexture "floor.jpeg"
    let specularMap = loadTexture "wall.jpeg"
    disposables.AddRange([lightingShader; lampShader; diffuseMap; specularMap])

    window.add_Update(fun deltaTime ->
        let moveSpeed = 2.5f * (float32 deltaTime)

        if primaryKeyboard.IsKeyPressed(Key.W)
        then camera <- { camera with Position = camera.Position + moveSpeed * camera.Front }
        if primaryKeyboard.IsKeyPressed(Key.S)
        then camera <- { camera with Position = camera.Position - moveSpeed * camera.Front }
        if primaryKeyboard.IsKeyPressed(Key.A)
        then camera <- { camera with Position = camera.Position - Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * moveSpeed }
        if primaryKeyboard.IsKeyPressed(Key.D)
        then camera <- { camera with Position = camera.Position + Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * moveSpeed }
    )

    window.add_Render(fun deltaTime ->
        gl.Enable(EnableCap.DepthTest)
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

        VaoCube.Bind()

        do // Render Lit Cube
            //Use the 'lighting shader' that is capable of modifying the cubes colours based on ambient lighting and diffuse lighting
            lightingShader.Use()
            //Bind the diffuse map and and set to use texture0.
            diffuseMap.Bind(TextureUnit.Texture0);
            //Bind the diffuse map and and set to use texture1.
            specularMap.Bind(TextureUnit.Texture1);

            //Setup the coordinate systems for our view
            lightingShader.SetUniform("uModel", Matrix4x4.Identity)
            lightingShader.SetUniform("uView", camera.GetViewMatrix())
            lightingShader.SetUniform("uProjection", camera.GetProjectionMatrix())
            //Let the shaders know where the Camera is looking from
            lightingShader.SetUniform("viewPos", camera.Position)
            //Configure the materials variables.
            //Diffuse is set to 0 because our diffuseMap is bound to Texture0
            lightingShader.SetUniform("material.diffuse", 0);
            //Specular is set to 1 because our diffuseMap is bound to Texture1
            lightingShader.SetUniform("material.specular", 1);
            lightingShader.SetUniform("material.shininess", 32.0f);

            let diffuseColor = Vector3(0.5f)
            let ambientColor = diffuseColor * Vector3(2.2f)
    
            lightingShader.SetUniform("light.ambient", ambientColor)
            lightingShader.SetUniform("light.diffuse", diffuseColor) // darkened
            lightingShader.SetUniform("light.specular", Vector3(1.0f, 1.0f, 1.0f))
            lightingShader.SetUniform("light.position", lampPosition)

            //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
            gl.DrawArrays(PrimitiveType.Triangles, 0, 36u)

        do // Render Lamp Cube
            //Use the 'main' shader that does not do any lighting calculations to just draw the cube to screen in the requested colours.
            lampShader.Use();
            //The Lamp cube is going to be a scaled down version of the normal cubes verticies moved to a different screen location
            let lampMatrix =
                Matrix4x4.Identity
                * Matrix4x4.CreateScale(0.2f)
                * Matrix4x4.CreateTranslation(lampPosition)

            //Setup the uniforms needed to draw the Lamp in the correct place on screen
            lampShader.SetUniform("uModel", lampMatrix)
            lampShader.SetUniform("uView", camera.GetViewMatrix())
            lampShader.SetUniform("uProjection", camera.GetProjectionMatrix())

            gl.DrawArrays(PrimitiveType.Triangles, 0, 36u);
    )

    window.add_Closing(fun _ ->
        disposables
        |> Seq.rev
        |> Seq.iter (fun x -> x.Dispose())
    )
)

window.Run();
