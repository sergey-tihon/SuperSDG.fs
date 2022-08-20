namespace SuperSDG.Engine

open System
open Silk.NET.OpenGL

type VertexArrayObject<'a when 'a: struct and 'a:> ValueType and 'a:(new : unit -> 'a )>
    (gl:GL, handle:uint32) =
    member _.Bind() = gl.BindVertexArray(handle)
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
        if ebo.IsSome then ebo.Value.Bind()
        
        new VertexArrayObject<'a>(gl, handle)
