namespace SuperSDG.Engine

open System
open Microsoft.FSharp.NativeInterop
open Silk.NET.OpenGL

#nowarn "9"

type BufferObject<'a when 'a: unmanaged>
    (gl:GL, bufferType: BufferTargetARB, handle:uint32) =
    member _.Bind() = gl.BindBuffer(bufferType, handle)
    interface IDisposable with
        member _.Dispose() = gl.DeleteBuffer(handle)
        
    static member Create (gl:GL, bufferType: BufferTargetARB, data:'a[] when 'a: unmanaged) =
        let handle = gl.GenBuffer()
        gl.BindBuffer(bufferType, handle)
        use ptr = fixed data
        let size = unativeint <| (data.Length * sizeof<'a>)
        gl.BufferData(bufferType, size, NativePtr.toVoidPtr ptr, BufferUsageARB.StaticDraw)
        
        new BufferObject<'a>(gl, bufferType, handle)
