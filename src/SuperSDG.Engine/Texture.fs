namespace SuperSDG.Engine

open System
open System.IO
open Microsoft.FSharp.NativeInterop
open Silk.NET.OpenGL
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

#nowarn "9"

type Texture(gl:GL, handle:uint32) =
    member _.Bind (?textureSlot: TextureUnit) =
        let textureSlot' = defaultArg textureSlot TextureUnit.Texture0
        gl.ActiveTexture(textureSlot')
        gl.BindTexture(TextureTarget.Texture2D, handle)
        
    interface IDisposable with
        member _.Dispose() = gl.DeleteTexture(handle)
        
    member _.GetUniformLocation(name:string) =
        gl.GetUniformLocation(handle, name)
        
    static member Load (gl:GL, stream: Stream) =
        let handle = gl.GenTexture()
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, handle)
        do
            use img = Image.Load<Rgba32>(stream)
            let width, height = uint32 <| img.Width, uint32 <| img.Height
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero.ToPointer())
            img.ProcessPixelRows(fun accessor ->
                for y in [0..accessor.Height-1] do
                    let data = &&accessor.GetRowSpan(y).GetPinnableReference()
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, width, 1u, PixelFormat.Rgba, PixelType.UnsignedByte, NativePtr.toVoidPtr data);
            )
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int GLEnum.Repeat)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int GLEnum.Repeat)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int GLEnum.LinearMipmapLinear)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int GLEnum.Linear)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0)
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8)
        gl.GenerateMipmap(TextureTarget.Texture2D)
                    
        new Texture(gl, handle)
