namespace SuperSDG.Engine

open System
open System.Numerics

module MathH =
    let radians degrees =
        degrees * MathF.PI / 180f

type CameraMovement =
    | Forward
    | Backward
    | Left
    | Right
    
type ICamera =
    abstract member GetViewMatrix: unit -> Matrix4x4
    abstract member GetProjectionMatrix: unit -> Matrix4x4
    
type FreeCamera =
    {
        // camera attributes
        Position : Vector3
        WorldUp: Vector3
        // euler angles
        Yaw : float32
        Pitch : float32
        // view options
        Zoom : float32 // view angle
        AspectRatio: float32
        // camera options
        MovementSpeed: float32
        MouseSensitivity: float32
    }
    static member Default =
        {
            Position = Vector3.Zero
            WorldUp = Vector3.UnitY
            Yaw = -90f
            Pitch = 0f
            Zoom = 45f
            AspectRatio =  16f / 9f
            MovementSpeed = 10f
            MouseSensitivity = 0.3f
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

    interface ICamera with
        member this.GetViewMatrix() =
            Matrix4x4.CreateLookAt(this.Position, this.Position + this.Front, this.Up)
        member this.GetProjectionMatrix() =
            Matrix4x4.CreatePerspectiveFieldOfView(MathH.radians(this.Zoom), this.AspectRatio, 0.1f, 100.0f)

        
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

type FollowCamera =
    {
        CameraTarget : Vector3
        WorldUp: Vector3
        // camera position
        Distance: float32
        Yaw : float32
        Pitch : float32
        // camera options
        Zoom : float32 // view angle
        AspectRatio: float32
        MouseSensitivity: float32
    }
    static member Default =
        {
            CameraTarget = Vector3.Zero
            WorldUp = Vector3.UnitY

            Distance = 5.0f
            Yaw = -90f
            Pitch = 0f

            Zoom = 45f
            AspectRatio =  16f / 9f
            MouseSensitivity = 0.3f
        }

    member this.Front =
        let cameraDirection = Vector3(
            MathF.Cos(MathH.radians(this.Yaw)) * MathF.Cos(MathH.radians(this.Pitch)),
            MathF.Sin(MathH.radians(this.Pitch)),
            MathF.Sin(MathH.radians(this.Yaw)) * MathF.Cos(MathH.radians(this.Pitch));
        )
        Vector3.Normalize(cameraDirection)
    member this.CameraPosition =
        this.CameraTarget - this.Distance * this.Front
    member this.Up =
        let right = Vector3.Normalize(Vector3.Cross(this.Front, this.WorldUp))  
        Vector3.Normalize(Vector3.Cross(right, this.Front))
    
    interface ICamera with
        member this.GetViewMatrix() =
            Matrix4x4.CreateLookAt(this.CameraPosition, this.CameraTarget, this.Up)
        member this.GetProjectionMatrix() =
            Matrix4x4.CreatePerspectiveFieldOfView(MathH.radians(this.Zoom), this.AspectRatio, 0.1f, 100.0f)
        
    member this.ProcessMouseMovement(xOffset:float32, yOffset:float32) =
        { this with
            Yaw = this.Yaw + xOffset * this.MouseSensitivity
            Pitch = Math.Clamp(this.Pitch + yOffset * this.MouseSensitivity, -89f, -30f)
        }
    member this.ProcessMouseScroll(yOffset:float32) =
        { this with Zoom = Math.Clamp(this.Zoom - yOffset, 10.0f, 45f) }
