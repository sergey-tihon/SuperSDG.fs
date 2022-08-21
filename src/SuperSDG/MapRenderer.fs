module SuperSDG.MapRenderer

open System
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

type WorldRenderer (gl:GL, assets:AssetManager) =
    let floorTexture = assets.LoadTexture "floor.jpeg"
    let floorShaderProgram = assets.LoadShaderProgram("floor.vert", "floor.frag")
    do  floorShaderProgram.Use()
        floorShaderProgram.SetUniform("texture1", 0)
    
    let floorVbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Data.floorVertices)    
    let floorVao = VertexArrayObject.Create(gl, floorVbo)
    do  floorVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 5u, 0)
        floorVao.VertexAttributePointer(1u, 2, VertexAttribPointerType.Float, 5u, 3)
        
    let resources: IDisposable[] =
        [|floorTexture; floorShaderProgram; floorVbo; floorVao|]
        
    member _.Render(map:WordMap, camera:Camera) =
        floorTexture.Bind(TextureUnit.Texture0)
        floorShaderProgram.Use()
        
        floorShaderProgram.SetUniform("view", camera.GetViewMatrix())
        let model = { Transform.Identity with Scale = float32 <| map.Map.GetLength(0) }
        floorShaderProgram.SetUniform("model", model.ViewMatrix)
        floorShaderProgram.SetUniform("projection", camera.GetProjectionMatrix())
        
        floorVao.Bind()
        gl.DrawArrays(GLEnum.Triangles, 0, 6u)

        
    interface IDisposable with
        member _.Dispose() = resources |> Array.iter (fun x -> x.Dispose())
