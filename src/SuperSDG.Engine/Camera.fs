namespace SuperSDG.Engine

open System
open System.Numerics

module MathH =
    let radians degrees =
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
            MathF.Cos(MathH.radians(yaw)) * MathF.Cos(MathH.radians(pitch)),
            MathF.Sin(MathH.radians(pitch)),
            MathF.Sin(MathH.radians(yaw)) * MathF.Cos(MathH.radians(pitch));
        )
        { this with Yaw = yaw; Pitch = pitch; Front = Vector3.Normalize(cameraDirection) }
    member this.GetViewMatrix() =
        Matrix4x4.CreateLookAt(this.Position, this.Position + this.Front, this.Up)
    member this.GetProjectionMatrix() =
        Matrix4x4.CreatePerspectiveFieldOfView(MathH.radians(this.Zoom), this.AspectRatio, 0.1f, 100.0f)

type CameraMovement =
    | Forward
    | Backward
    | Left
    | Right
    
type Camera2 =
    {
        // camera Attributes
        Position : Vector3
        WorldUp: Vector3
        // euler Angles
        Yaw : float32
        Pitch : float32
        // camera options
        MovementSpeed: float32
        MouseSensitivity: float32
        Zoom : float32 // view angle
    }
    static member Default: Camera2 =
        {
            Position = Vector3.Zero
            WorldUp = Vector3.UnitY
            Yaw = -90f
            Pitch = 0f
            MovementSpeed = 2.5f
            MouseSensitivity = 0.1f
            Zoom = 45f
        }
        
    member this.Front =
        let cameraDirection = Vector3(
            MathF.Cos(MathH.radians(this.Yaw)) * MathF.Cos(MathH.radians(this.Pitch)),
            MathF.Sin(MathH.radians(this.Pitch)),
            MathF.Sin(MathH.radians(this.Yaw)) * MathF.Cos(MathH.radians(this.Pitch));
        )
        Vector3.Normalize(cameraDirection)  
    member this.Right =
        Vector3.Normalize(Vector3.Cross(this.Front, this.WorldUp))   
    member this.Up =
        Vector3.Normalize(Vector3.Cross(this.Right, this.Front))

    member this.GetViewMatrix() =
        Matrix4x4.CreateLookAt(this.Position, this.Position + this.Front, this.Up)
    member this.GetProjectionMatrix(aspectRatio) =
        Matrix4x4.CreatePerspectiveFieldOfView(MathH.radians(this.Zoom), aspectRatio, 0.1f, 100.0f)

        
    member this.ProcessKeyboard(direction:CameraMovement, deltaTime:float32) =
        let velocity = this.MovementSpeed * deltaTime
        match direction with
        | Forward   -> {this with Position = this.Position + this.Front * velocity}
        | Backward  -> {this with Position = this.Position - this.Front * velocity}
        | Left      -> {this with Position = this.Position - this.Right * velocity}
        | Right     -> {this with Position = this.Position + this.Right * velocity}
        
    member this.ProcessMouseMovement(xOffset:float32, yOffset:float32) =
        { this with
            Yaw = this.Yaw + xOffset * this.MouseSensitivity
            Pitch = Math.Clamp(this.Pitch + yOffset * this.MouseSensitivity, -89f, 89f)
        }
    member this.ProcessMouseScroll(yOffset:float32) =
        { this with Zoom = Math.Clamp(this.Zoom - yOffset, 1.0f, 45f) }
