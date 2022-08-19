namespace SuperSDG.Engine

open System
open System.Numerics

module MathHelper =
    let degreesToRadians degrees =
        degrees * MathF.PI / 180f

type Camera =
    {
        Position : Vector3
        Front : Vector3
        Up : Vector3
        AspectRatio : float32
        Yaw : float32
        Pitch : float32
        Zoom : float32
    }
    static member Default =
        {
            Position = Vector3.Zero
            Front = Vector3.Zero
            Up = Vector3.Zero
            AspectRatio = 0f
            Yaw = -90f
            Pitch = 0f
            Zoom = 45f
        }
    member this.ModifyZoom(zoomAmount) =
        //We don't want to be able to zoom in too close or too far away so clamp to these values
        { this with Zoom = Math.Clamp(this.Zoom - zoomAmount, 1.0f, 45f) }
    member this.ModifyDirection(xOffset:float32, yOffset:float32) =
        let yaw = this.Yaw + xOffset
        let pitch = this.Pitch - yOffset
        //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
        let pitch = Math.Clamp(pitch, -89f, 89f)
        let cameraDirection = Vector3(
            MathF.Cos(MathHelper.degreesToRadians(yaw)) * MathF.Cos(MathHelper.degreesToRadians(pitch)),
            MathF.Sin(MathHelper.degreesToRadians(pitch)),
            MathF.Sin(MathHelper.degreesToRadians(yaw)) * MathF.Cos(MathHelper.degreesToRadians(pitch));
        )
        { this with Yaw = yaw; Pitch = pitch; Front = Vector3.Normalize(cameraDirection) }
    member this.GetViewMatrix() =
        Matrix4x4.CreateLookAt(this.Position, this.Position + this.Front, this.Up)
    member this.GetProjectionMatrix() =
        Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.degreesToRadians(this.Zoom), this.AspectRatio, 0.1f, 100.0f)
