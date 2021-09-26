open System
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing
open SuperSDG.Core
#nowarn "9"

//Vertex data, uploaded to the VBO.
let Vertices = [|
     //X    Y      Z
     0.5f;  0.5f; 0.0f;
     0.5f; -0.5f; 0.0f;
    -0.5f; -0.5f; 0.0f;
    -0.5f;  0.5f; 0.5f
|]

//Index data, uploaded to the EBO.
let Indices =
    [|
        0u; 1u; 3u;
        1u; 2u; 3u
    |]

let mutable Gl = Unchecked.defaultof<_>
let mutable Vao = Unchecked.defaultof<_>
let mutable Shader = Unchecked.defaultof<_>
let disposables = Collections.Generic.List<IDisposable>();

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
        
    Gl <- GL.GetApi(window)
        
    //Instantiating our new abstractions
    let Vbo = new BufferObject<float32>(Gl, Vertices, BufferTargetARB.ArrayBuffer)
    let Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer)
    Vao <- new VertexArrayObject<float32, uint>(Gl, Vbo, Ebo)
    disposables.AddRange([Vbo; Ebo; Vao])
            
    //Telling the VAO object how to lay out the attribute pointers
    Vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 7u, 0)
    Vao.VertexAttributePointer(1u, 4, VertexAttribPointerType.Float, 7u, 3) 

    Shader <- new Shader(Gl, "Shader.vert", "Shader.frag")
    disposables.Add(Shader)
)
window.add_Render(fun _ ->
    Gl.Clear(uint <| ClearBufferMask.ColorBufferBit)
    
    Vao.Bind()
    Shader.Use()
    Shader.SetUniform("uBlue", float32 <| Math.Sin(float(DateTime.Now.Millisecond) / 1000. * Math.PI))
    
    Gl.DrawElements(
        PrimitiveType.Triangles,
        uint <| Indices.Length,
        DrawElementsType.UnsignedInt,
        IntPtr.Zero.ToPointer())
)

window.add_Update(fun _ -> ())

window.add_Closing(fun _ ->
    for x in disposables do
        x.Dispose()
)

window.Run();