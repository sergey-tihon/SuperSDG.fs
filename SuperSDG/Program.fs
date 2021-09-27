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
let CameraPosition = Vector3(0.0f, 0.0f, 3.0f)
let CameraTarget = Vector3.Zero
let CameraDirection = Vector3.Normalize(CameraPosition - CameraTarget)
let CameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, CameraDirection))
let CameraUp = Vector3.Cross(CameraDirection, CameraRight)

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
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            if key = Key.Escape then window.Close()
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
    
    window.add_Render(fun _ ->
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
        let view = Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUp)
        let projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.degreesToRadians 45.0f, float32 Width / float32 Height, 0.1f, 100.0f);

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