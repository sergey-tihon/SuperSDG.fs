namespace SuperSDG

open System.IO
open System.Reflection
open Silk.NET.OpenGL
open SuperSDG.Engine

type AssetManager(gl:GL, assembly:Assembly) =
    let asmName = assembly.GetName().Name
        
    member _.LoadEmbeddedStream (name: string) =
        let fullName = $"{asmName}.Resources.{name}"
        assembly.GetManifestResourceStream fullName

    member this.LoadEmbeddedText name =
        use stream = this.LoadEmbeddedStream(name)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()

    member this.LoadTexture name =
        use stream = this.LoadEmbeddedStream name
        Texture.Load(gl, stream)
        
    member this.LoadShaderProgram(vertShaderFileName, fragShaderFileName) =
        Shader.Create(gl,
            this.LoadEmbeddedText vertShaderFileName,
            this.LoadEmbeddedText fragShaderFileName)
