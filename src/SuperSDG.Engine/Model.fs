module SuperSDG.Engine.Model

open System
open System.Numerics
open System.Runtime.InteropServices
open Silk.NET.OpenGL
open SuperSDG

[<Struct; StructLayout(LayoutKind.Sequential, Pack = 1)>]
type Vertex = {
    Position : Vector3
    Normal : Vector3
    TexCoords : Vector2
    Tangent: Vector3
    Bitangent: Vector3
    
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst=16)>]
    boneIds: int[] //bone indexes which will influence this vertex
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst=16)>]
    weights: float32[] //weights from each bone
}

type TextureKing =
    | diffuse = 0
    | specular = 1
    | normal = 2
    | height = 3
    
type Texture = {
    texture: Engine.Texture
    kind: TextureKing
    path: string
}

type Mesh<'a when 'a: struct and 'a:> ValueType and 'a:(new : unit -> 'a )>
    (gl:GL, vao: VertexArrayObject<'a>, textures:Texture[]) =
    
    member _.Draw(shader: Engine.Shader) =
        let mutable nums = Map.empty<int, int>
        textures |> Array.iteri(fun i texture ->
            let name =
                let kindId = texture.kind |> int
                let nextId = nums |> Map.tryFind kindId |> Option.defaultValue 1
                nums <- nums |> Map.add kindId (nextId + 1)
                $"texture_%O{texture.kind}%d{nextId}"
            shader.SetUniform(name, i)
            
            let textureUnit: TextureUnit = (int TextureUnit.Texture0 + i) |> LanguagePrimitives.EnumOfValue
            texture.texture.Bind(textureUnit)
        )
        vao.Bind()
        gl.DrawElements(PrimitiveType.Triangles, uint32 vao.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero.ToPointer())
        gl.BindVertexArray(0u)
        gl.ActiveTexture(TextureUnit.Texture0)
    
    static member Load (gl:GL, vertices: Vertex[], indices: uint[], textures:Texture[]) =
        let vbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, vertices)
        let ebo = BufferObject.Create(gl, BufferTargetARB.ElementArrayBuffer, indices)
        let vao = VertexArrayObject.Create(gl, vbo, ebo)
        
        let ty, v = typeof<Vertex>, Unchecked.defaultof<Vertex>
        let sizeOfVertex = Marshal.SizeOf(ty) |> uint
        let offsetOf prop = Marshal.OffsetOf(ty, nameof prop).ToInt32()
        vao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, sizeOfVertex, 0)
        vao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, sizeOfVertex, offsetOf v.Normal)
        vao.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, sizeOfVertex, offsetOf v.TexCoords)
        vao.VertexAttributePointer(3u, 3, VertexAttribPointerType.Float, sizeOfVertex, offsetOf v.Tangent)
        vao.VertexAttributePointer(4u, 3, VertexAttribPointerType.Float, sizeOfVertex, offsetOf v.Bitangent)
        vao.VertexAttributePointer(5u, 4, VertexAttribPointerType.Int, sizeOfVertex, offsetOf v.boneIds)
        vao.VertexAttributePointer(6u, 4, VertexAttribPointerType.Int, sizeOfVertex, offsetOf v.weights)
        gl.BindVertexArray(0u)
        
        Mesh(gl, vao, textures)
