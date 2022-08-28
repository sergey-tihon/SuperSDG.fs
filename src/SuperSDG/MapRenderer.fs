module SuperSDG.MapRenderer

open System
open System.Numerics
open Silk.NET.Maths
open Silk.NET.OpenGL
open SuperSDG.Engine
open SuperSDG.MapGenerator

module Data =
    let floorVertices = [|
        0.f;  0.f; 0.f;  0.0f; 1.0f;
        1.f;  0.f; 0.f;  1.0f; 1.0f;
        1.f;  0.f; 1.f;  1.0f; 0.0f;
        1.f;  0.f; 1.f;  1.0f; 0.0f;
        0.f;  0.f; 1.f;  0.0f; 0.0f;
        0.f;  0.f; 0.f;  0.0f; 1.0f
    |]
    
    let cubeVertices = [|
        -0.5f; -0.5f; -0.5f;  0.0f; 0.0f;
         0.5f; -0.5f; -0.5f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 0.0f;

        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;

        -0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f;  0.5f;  0.5f;  1.0f; 0.0f;

         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;

        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  1.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; 1.0f;

        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f; 0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f; 0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f; 1.0f
    |]
    
    let cubeVerticesWithNorm = [|
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 
        -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f; 

        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;

        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;

         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;

        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;

        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f
    |]
    
type WorldRenderer (gl:GL, assets:AssetManager) =
    let floorTexture = assets.LoadTexture "floor.jpeg"
    let floorShaderProgram = assets.LoadShaderProgram("floor.vert", "floor.frag")
    do  floorShaderProgram.Use()
        floorShaderProgram.SetUniform("texture1", 0)
    
    let floorVbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Data.floorVertices)    
    let floorVao = VertexArrayObject.Create(gl, floorVbo)
    do  floorVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
        floorVao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3)
        
    let wallTexture = assets.LoadTexture "wall.jpeg"
    let wallShaderProgram = assets.LoadShaderProgram("wall.vert", "wall.frag")
    do  wallShaderProgram.Use()
        wallShaderProgram.SetUniform("texture1", 0)
    
    let wallVbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Data.cubeVertices)   
    let wallVao = VertexArrayObject.Create(gl, wallVbo)
    do  wallVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
        wallVao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3) 
        
    let resources: IDisposable[] =
        [|floorTexture; floorShaderProgram; floorVbo; floorVao
          wallTexture; wallShaderProgram; wallVbo; wallVao|] 
    
    member _.Render(map:WordMap, camera:ICamera) =
        floorTexture.Bind(TextureUnit.Texture0)
        floorShaderProgram.Use()
        
        floorShaderProgram.SetUniform("view", camera.GetViewMatrix())
        let model = {
            Transform.Identity with
                Scale = float32 <| map.Map.GetLength(0)
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathH.radians 180f)
        }
        floorShaderProgram.SetUniform("model", model.ViewMatrix)
        floorShaderProgram.SetUniform("projection", camera.GetProjectionMatrix())
        
        floorVao.Bind()
        gl.DrawArrays(GLEnum.Triangles, 0, 6u)
        
        wallTexture.Bind(TextureUnit.Texture0)
        wallShaderProgram.Use()
        
        floorShaderProgram.SetUniform("view", camera.GetViewMatrix())
        floorShaderProgram.SetUniform("projection", camera.GetProjectionMatrix())
        
        wallVao.Bind()
        
        map.Map
        |>Array2D.iteri(fun x y c ->
            if c = '#' then
                let model = {
                    Transform.Identity with
                        Position = Vector2D(x, y) |> to3D
                }
                wallShaderProgram.SetUniform("model", model.ViewMatrix)
                gl.DrawArrays(GLEnum.Triangles, 0, 36u)
        )                
                
        let model = {
            Transform.Identity with
                Position = map.Player3
                Rotation = Quaternion.CreateFromYawPitchRoll(MathH.radians 45f, MathH.radians 45f, MathH.radians 0f)
        }
        wallShaderProgram.SetUniform("model", model.ViewMatrix)
        gl.DrawArrays(GLEnum.Triangles, 0, 36u)

        
    interface IDisposable with
        member _.Dispose() = resources |> Array.iter (fun x -> x.Dispose())
