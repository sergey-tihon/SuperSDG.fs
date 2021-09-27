open System
open System.Numerics
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG.Core
#nowarn "9"

let Vertices = [|
    //X    Y      Z     U   V
     0.5f;  0.5f; 0.0f; 1f; 1f;
     0.5f; -0.5f; 0.0f; 1f; 0f;
    -0.5f; -0.5f; 0.0f; 0f; 0f;
    -0.5f;  0.5f; 0.5f; 0f; 1f
|]
let Indices = [|
    0u; 1u; 3u;
    1u; 2u; 3u
|]

let mutable options = WindowOptions.Default
options.Size <- Vector2D<int>(800, 600)
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
    
    let transforms = [|
        { Transform.Identity with Position = Vector3(0.5f, 0.5f, 0f) }
        { Transform.Identity with Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f) }
        { Transform.Identity with Scale = 0.5f}
        { Transform.Identity with
           Position = Vector3(-0.5f, 0.5f, 0f)
           Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f)
           Scale = 0.5f}
    |]
    
    window.add_Render(fun _ ->
        gl.Clear(uint <| ClearBufferMask.ColorBufferBit)
        
        vao.Bind()
        shader.Use()
        texture.Bind(TextureUnit.Texture0)
        shader.SetUniform("uTexture0", 0f)

        for t in transforms do
            shader.SetUniform("uModel", t.ViewMatrix)
            gl.DrawElements(
                PrimitiveType.Triangles,
                uint <| Indices.Length,
                DrawElementsType.UnsignedInt,
                IntPtr.Zero.ToPointer())
    )

    window.add_Closing(fun _ ->
        for x in disposables do
            x.Dispose()
    )
)

window.Run();