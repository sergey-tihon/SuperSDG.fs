namespace SuperSDG.Engine

open System
open Silk.NET.OpenGL

type BufferObject<'a when 'a: struct and 'a:> ValueType and 'a:(new : unit -> 'a )>
    (gl:GL, bufferType: BufferTargetARB, handle:uint32, length: int) =
    member _.Bind() = gl.BindBuffer(bufferType, handle)
    member _.Length = length
    interface IDisposable with
        member _.Dispose() = gl.DeleteBuffer(handle)
        
    static member Create (gl:GL, bufferType: BufferTargetARB, data:'a[]) =
        let handle = gl.GenBuffer()
        gl.BindBuffer(bufferType, handle)
        gl.BufferData(bufferType, ReadOnlySpan(data), BufferUsageARB.StaticDraw)
        
        new BufferObject<'a>(gl, bufferType, handle, data.Length)
