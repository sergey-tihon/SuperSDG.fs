namespace SuperSDG.Engine

open System
open Silk.NET.OpenGL

type VertexArrayObject<'a when 'a: struct and 'a:> ValueType and 'a:(new : unit -> 'a )>
    (gl:GL, handle:uint32, indexCount: int) =
    member _.Bind() = gl.BindVertexArray(handle)
    member _.IndexCount = indexCount
    interface IDisposable with
        member _.Dispose() = gl.DeleteVertexArray(handle)
        
    member this.VertexAttributePointer(index:uint, count:int, ty:VertexAttribPointerType, vertexSize:uint, offSet) =
        let stride = uint32 <| vertexSize * uint(sizeof<'a>)
        let offsetPtr = IntPtr(offSet * sizeof<'a>).ToPointer()
        gl.VertexAttribPointer(index, count, ty, false, stride, offsetPtr)
        gl.EnableVertexAttribArray(index)
        
    static member Create (gl:GL, vbo: BufferObject<'a>, ?ebo: BufferObject<_>) =
        let handle = gl.GenVertexArray()
        gl.BindVertexArray(handle)
        vbo.Bind()
        ebo |> Option.iter (fun x-> x.Bind())
        let indexCount = ebo |> Option.map (fun x->x.Length) |> Option.defaultValue 0
        
        new VertexArrayObject<'a>(gl, handle, indexCount)
