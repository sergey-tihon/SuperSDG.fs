open System
open System.Numerics
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG.Core
#nowarn "9"

let Width, Height = (800, 600)
let Vertices = [|
    //X    Y      Z       Normals
    -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
     0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
     0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
     0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
    -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
    -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;

    -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
     0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
     0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
     0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
    -0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
    -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;

    -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
    -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
    -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
    -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
    -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
    -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;

     0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
     0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
     0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
     0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
     0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
     0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;

    -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
     0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
     0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
     0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
    -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
    -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;

    -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
     0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
     0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
     0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
    -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
-0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f
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
        
    let gl = GL.GetApi(window)
    let disposables = Collections.Generic.List<IDisposable>()
    
    //Instantiating our new abstractions
    let vbo = new BufferObject<float32>(gl, Vertices, BufferTargetARB.ArrayBuffer)
    let ebo = new BufferObject<uint>(gl, Indices, BufferTargetARB.ElementArrayBuffer)
    let VaoCube = new VertexArrayObject<float32, uint>(gl, vbo, ebo)
    disposables.AddRange([vbo; ebo; VaoCube])
            
    VaoCube.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 6u, 0)
    VaoCube.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 6u, 3)
    
    //The lighting shader will give our main cube it's colour multiplied by the lights intensity
    let lightingShader = new Shader(gl, "Resources/shader.vert", "Resources/lighting.frag")
    //The Lamp shader uses a fragment shader that just colours it solid white so that we know it is the light source
    let lampShader = new Shader(gl, "Resources/shader.vert", "Resources/shader.frag")
    let lampPosition = new Vector3(1.2f, 1.0f, 2.0f)

    let texture = new Texture(gl, "Resources/floor.jpeg")
    disposables.AddRange([lightingShader; lampShader; texture])
    
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
        
        //Use the 'lighting shader' that is capable of modifying the cubes colours based on ambient lighting and diffuse lighting
        lightingShader.Use()
        //Set up the uniforms needed for the lighting shaders to be able to draw and light the coral cube
        lightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(MathHelper.degreesToRadians(25f)))
        lightingShader.SetUniform("uView", camera.GetViewMatrix())
        lightingShader.SetUniform("uProjection", camera.GetProjectionMatrix())
        lightingShader.SetUniform("objectColor", Vector3(1.0f, 0.5f, 0.31f))
        lightingShader.SetUniform("lightColor", Vector3.One)
        lightingShader.SetUniform("lightPos", lampPosition)
        
        //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36u)
        
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
        for x in disposables do
            x.Dispose()
    )
)

window.Run();