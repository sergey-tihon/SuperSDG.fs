open System
open System.Numerics
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG.Core
#nowarn "9"

let Width, Height = (800, 600)
//Setup the camera's location, and relative up and right directions
let mutable CameraPosition = Vector3(0.0f, 0.0f, 3.0f);
let mutable CameraFront = Vector3(0.0f, 0.0f, -1.0f);
let CameraUp = Vector3.UnitY;
let CameraDirection = Vector3.Zero;

let mutable CameraYaw = -90f
let mutable CameraPitch = 0f
let mutable CameraZoom = 45f
let mutable LastMousePosition = Vector2.Zero

let Vertices = [|
    //X    Y      Z     U   V
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
    for mice in input.Mice do
        mice.Cursor.CursorMode <- CursorMode.Raw
        mice.add_MouseMove(fun mouse position ->
            let lookSensitivity = 0.1f
            if LastMousePosition = Vector2.Zero
            then LastMousePosition <- position
            else
                let xOffset = (position.X - LastMousePosition.X) * lookSensitivity
                let yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity
                LastMousePosition <- position

                CameraYaw <- CameraYaw + xOffset
                CameraPitch <- CameraPitch - yOffset
                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                CameraPitch <- Math.Clamp(CameraPitch, -89.0f, 89.0f);

                CameraFront <-
                    Vector3(
                        MathF.Cos(MathHelper.degreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.degreesToRadians(CameraPitch)),
                        MathF.Sin(MathHelper.degreesToRadians(CameraPitch)),
                        MathF.Sin(MathHelper.degreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.degreesToRadians(CameraPitch))
                    ) |> Vector3.Normalize
        )
        mice.add_Scroll(fun mouse scrollWheel ->
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            CameraZoom <- Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f)
        )
        
    let gl = GL.GetApi(window)
    let disposables = Collections.Generic.List<IDisposable>()
    
    //Instantiating our new abstractions
    let vbo = new BufferObject<float32>(gl, Vertices, BufferTargetARB.ArrayBuffer)
    let ebo = new BufferObject<uint>(gl, Indices, BufferTargetARB.ElementArrayBuffer)
    let vao = new VertexArrayObject<float32, uint>(gl, vbo, ebo)
    disposables.AddRange([vbo; ebo; vao])
            
    //Telling the VAO object how to lay out the attribute pointers
    vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
    vao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3) 

    let shader = new Shader(gl, "Resources/Shader.vert", "Resources/Shader.frag")
    let texture = new Texture(gl, "Resources/floor.jpeg");
    disposables.AddRange([shader; texture])
    
    window.add_Render(fun deltaTime ->
        let moveSpeed = 2.5f * (float32 deltaTime)

        if primaryKeyboard.IsKeyPressed(Key.W)
        then CameraPosition <- CameraPosition + moveSpeed * CameraFront;
        if primaryKeyboard.IsKeyPressed(Key.S)
        then CameraPosition <- CameraPosition - moveSpeed * CameraFront;
        if primaryKeyboard.IsKeyPressed(Key.A)
        then CameraPosition <- CameraPosition - Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed
        if primaryKeyboard.IsKeyPressed(Key.D)
        then CameraPosition <- CameraPosition + Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed
    )
    
    window.add_Render(fun deltaTime ->
        gl.Enable(EnableCap.DepthTest)
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit);
        
        vao.Bind()
        texture.Bind()
        shader.Use()
        shader.SetUniform("uTexture0", 0f)

        //Use elapsed time to convert to radians to allow our cube to rotate over time
        let difference = float32 <| (window.Time * 100.)

        let model = Matrix4x4.CreateRotationY(MathHelper.degreesToRadians difference)
                        * Matrix4x4.CreateRotationX(MathHelper.degreesToRadians difference);
        let view = Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CameraFront, CameraUp)
        let projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.degreesToRadians(CameraZoom), float32 Width / float32 Height, 0.1f, 100.0f);

        shader.SetUniform("uModel", model)
        shader.SetUniform("uView", view)
        shader.SetUniform("uProjection", projection)

        //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
        gl.DrawArrays(PrimitiveType.Triangles, 0, 36u);
    )

    window.add_Closing(fun _ ->
        for x in disposables do
            x.Dispose()
    )
)

window.Run();