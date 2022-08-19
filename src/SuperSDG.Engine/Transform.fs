namespace SuperSDG.Engine

open System.Numerics

type Transform =
    {
        Position : Vector3
        Scale : float32
        Rotation: Quaternion
    }
    static member Identity =
        {
            Position = Vector3(0f, 0f, 0f)
            Scale = 1f
            Rotation = Quaternion.Identity
        }
    member this.ViewMatrix with get () =
        Matrix4x4.Identity
            * Matrix4x4.CreateFromQuaternion(this.Rotation)
            * Matrix4x4.CreateScale(this.Scale)
            * Matrix4x4.CreateTranslation(this.Position)
