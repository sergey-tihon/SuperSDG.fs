open System
open Microsoft.FSharp.NativeInterop
open Silk.NET.Input
open Silk.NET.Maths
open Silk.NET.OpenGL
open Silk.NET.Windowing

#nowarn "9"

//Vertex shaders are run on each vertex.
let VertexShaderSource = @"
#version 330 core //Using version GLSL version 3.3
layout (location = 0) in vec4 vPos;

void main()
{
    gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
}"

//Fragment shaders are run on each fragment/pixel of the geometry.
let FragmentShaderSource = @"
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}"

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
        0; 1; 3;
        1; 2; 3
    |]
    |> Array.map (uint)

let mutable Gl: GL = null
let mutable Vbo = Unchecked.defaultof<_>
let mutable Ebo = Unchecked.defaultof<_>
let mutable Vao = Unchecked.defaultof<_>
let mutable Shader = Unchecked.defaultof<_>

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
    
    Vao <- Gl.GenVertexArray()
    Gl.BindVertexArray(Vao)
    
    //Initializing a vertex buffer that holds the vertex data.
    do 
        Vbo <- Gl.GenBuffer()
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo)
        use floatPtr = fixed Vertices
        Gl.BufferData(
            BufferTargetARB.ArrayBuffer,
            unativeint <| (Vertices.Length * sizeof<uint>),
            NativePtr.toVoidPtr floatPtr,
            BufferUsageARB.StaticDraw
        )
        
    //Initializing a element buffer that holds the index data.
    do 
        Ebo <- Gl.GenBuffer()
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo)
        use intPtr = fixed Indices
        Gl.BufferData(
            BufferTargetARB.ElementArrayBuffer,
            unativeint <| (Indices.Length * sizeof<uint>),
            NativePtr.toVoidPtr intPtr,
            BufferUsageARB.StaticDraw
        )
        
    //Creating a vertex shader.
    let vertexShader = Gl.CreateShader(ShaderType.VertexShader)
    Gl.ShaderSource(vertexShader, VertexShaderSource)
    Gl.CompileShader(vertexShader)

    //Checking the shader for compilation errors.
    let infoLog = Gl.GetShaderInfoLog(vertexShader);
    if not <| String.IsNullOrWhiteSpace(infoLog)
    then printfn $"Error compiling vertex shader {infoLog}"
            
    //Creating a fragment shader.
    let fragmentShader = Gl.CreateShader(ShaderType.FragmentShader)
    Gl.ShaderSource(fragmentShader, FragmentShaderSource)
    Gl.CompileShader(fragmentShader)

    //Checking the shader for compilation errors.
    let infoLog = Gl.GetShaderInfoLog(fragmentShader);
    if not <| String.IsNullOrWhiteSpace(infoLog)
    then printfn $"Error compiling fragment shader {infoLog}"
            
    //Combining the shaders under one shader program.
    Shader <- Gl.CreateProgram()
    Gl.AttachShader(Shader, vertexShader)
    Gl.AttachShader(Shader, fragmentShader)
    Gl.LinkProgram(Shader)

    //Checking the linking for errors.
    let status = Gl.GetProgram(Shader, GLEnum.LinkStatus)
    if status = 0
    then printfn $"Error linking shader {Gl.GetProgramInfoLog(Shader)}"
            

    //Delete the no longer useful individual shaders;
    Gl.DetachShader(Shader, vertexShader)
    Gl.DetachShader(Shader, fragmentShader)
    Gl.DeleteShader(vertexShader)
    Gl.DeleteShader(fragmentShader)

    //Tell opengl how to give the data to the shaders.
    Gl.VertexAttribPointer(uint 0, 3, VertexAttribPointerType.Float, false, uint <| (3 * sizeof<float32>),
                           IntPtr.Zero.ToPointer())
    Gl.EnableVertexAttribArray(uint 0)
)
window.add_Render(fun _ ->
    Gl.Clear(uint <| ClearBufferMask.ColorBufferBit)
    
    Gl.BindVertexArray(Vao)
    Gl.UseProgram(Shader)
    
    Gl.DrawElements(
        PrimitiveType.Triangles,
        uint <| Indices.Length,
        DrawElementsType.UnsignedInt,
        IntPtr.Zero.ToPointer())
)

window.add_Update(fun _ -> ())
window.add_Closing(fun _ ->
    Gl.DeleteBuffer(Vbo)
    Gl.DeleteBuffer(Ebo)
    Gl.DeleteVertexArray(Vao)
    Gl.DeleteProgram(Shader)
)

window.Run();