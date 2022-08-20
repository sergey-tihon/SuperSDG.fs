namespace SuperSDG

open System.IO
open System.Reflection

type AssetManager(assembly:Assembly) =
    let asmName = assembly.GetName().Name
        
    member _.LoadEmbeddedStream (name: string) =
        let fullName = $"{asmName}.Resources.{name}"
        assembly.GetManifestResourceStream fullName

    member this.LoadEmbeddedText (name: string) =
        use stream = this.LoadEmbeddedStream(name)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()
