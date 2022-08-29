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
    
    let floorVerticesWithNorm = [|
        0.f;  0.f; 0.f;  0.0f; -1.0f;  0.0f;  0.0f; 1.0f;
        1.f;  0.f; 0.f;  0.0f; -1.0f;  0.0f;  1.0f; 1.0f;
        1.f;  0.f; 1.f;  0.0f; -1.0f;  0.0f;  1.0f; 0.0f;
        1.f;  0.f; 1.f;  0.0f; -1.0f;  0.0f;  1.0f; 0.0f;
        0.f;  0.f; 1.f;  0.0f; -1.0f;  0.0f;  0.0f; 0.0f;
        0.f;  0.f; 0.f;  0.0f; -1.0f;  0.0f;  0.0f; 1.0f;
    |]
    let cubeVerticesWithNorm = [|
        // positions          // normals           // texture coords
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  0.0f; 0.0f;
         0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  1.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  1.0f; 1.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;  0.0f; 0.0f;

        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   0.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   1.0f; 1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f; 1.0f;   0.0f; 0.0f;

        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;  1.0f; 0.0f;
        -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f;  1.0f; 1.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;  0.0f; 1.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;  0.0f; 1.0f;
        -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f;  0.0f; 0.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;  1.0f; 0.0f;

         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;  1.0f; 0.0f;
         0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f;  1.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;  0.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f;  0.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;  1.0f; 0.0f;

        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;  0.0f; 1.0f;
         0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;  1.0f; 1.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;  1.0f; 0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;  1.0f; 0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;  0.0f; 0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;  0.0f; 1.0f;

        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;  0.0f; 1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;  1.0f; 1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;  1.0f; 0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;  1.0f; 0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;  0.0f; 0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;  0.0f; 1.0f
    |]
    
type WorldRenderer (gl:GL, assets:AssetManager, initialMap:char[,]) =
    
    let lightsCount = 20
    let loadUniforms (shader:Shader) =
        shader.Use()
        shader.SetUniform("material.diffuse", 0)
        shader.SetUniform("material.specular", 1)
        shader.SetUniform("material.shininess", 32.0f)
        
        // directional light
        shader.SetUniform("dirLight.direction", Vector3(-0.2f, -1.0f, -0.3f))
        shader.SetUniform("dirLight.ambient", Vector3(0.05f, 0.05f, 0.05f))
        shader.SetUniform("dirLight.diffuse", Vector3(0.4f, 0.4f, 0.4f))
        shader.SetUniform("dirLight.specular", Vector3(0.5f, 0.5f, 0.5f))
        
        // point lights
        [0..lightsCount-1] |> Seq.iter (fun i  ->
            let lightName = $"pointLights[{i}]"
            shader.SetUniform($"{lightName}.ambient", Vector3(0.05f, 0.05f, 0.05f))
            shader.SetUniform($"{lightName}.diffuse", Vector3(0.8f, 0.8f, 0.8f))
            shader.SetUniform($"{lightName}.specular", Vector3(1.0f, 1.0f, 1.0f))
            shader.SetUniform($"{lightName}.constant", 1.0f)
            shader.SetUniform($"{lightName}.linear", 0.09f)
            shader.SetUniform($"{lightName}.quadratic", 0.032f)
        )
        
        // spotLight
        shader.SetUniform("spotLight.ambient", Vector3(0.0f, 0.0f, 0.0f))
        shader.SetUniform("spotLight.diffuse", Vector3(1.0f, 1.0f, 1.0f))
        shader.SetUniform("spotLight.specular", Vector3(1.0f, 1.0f, 1.0f))
        shader.SetUniform("spotLight.constant", 1.0f)
        shader.SetUniform("spotLight.linear", 0.09f)
        shader.SetUniform("spotLight.quadratic", 0.032f)
        shader.SetUniform("spotLight.cutOff", cos(MathH.radians(8.5f)))
        shader.SetUniform("spotLight.outerCutOff", cos(MathH.radians(12.5f)))
    
    let floorTexture = assets.LoadTexture "floor.jpeg"
    let floorShader = assets.LoadShaderProgram("floor.vert", "container.frag")
    do  loadUniforms floorShader
    
    let floorVbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Data.floorVerticesWithNorm)    
    let floorVao = VertexArrayObject.Create(gl, floorVbo)
    do  floorVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)
        floorVao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 8u, 3)
        floorVao.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, 8u, 6) 
        
    let containerTexture = assets.LoadTexture "container.png"
    let containerSpecularTexture = assets.LoadTexture "container_specular.png"
    let containerShader = assets.LoadShaderProgram("container.vert", "container.frag")
    do  loadUniforms containerShader
    
    let wallVbo = BufferObject.Create(gl, BufferTargetARB.ArrayBuffer, Data.cubeVerticesWithNorm)   
    let wallVao = VertexArrayObject.Create(gl, wallVbo)
    do  wallVao.VertexAttributePointer(0u, 3, VertexAttribPointerType.Float, 8u, 0)
        wallVao.VertexAttributePointer(1u, 3, VertexAttribPointerType.Float, 8u, 3)
        wallVao.VertexAttributePointer(2u, 2, VertexAttribPointerType.Float, 8u, 6) 
        
    let resources: IDisposable[] =
        [|floorTexture; floorShader; floorVbo; floorVao
          containerTexture; containerSpecularTexture; containerShader; wallVbo; wallVao|]
        
    let rnd = Random()
    let walls =
        initialMap
        |> Array2D.mapi(fun x y c ->
            if c = '#' then
                let model = {
                    Transform.Identity with
                        Position = Vector2D(x, y) |> to3D
                        Rotation =
                            Quaternion.CreateFromYawPitchRoll(
                                MathH.radians(90.f * float32(rnd.Next(0, 4))),
                                0.f, 0.f
                            )
                }
                Some <| model.ViewMatrix
            else None)
    let lights =
        [0..lightsCount-1]
        |> List.map (fun _ ->
            Vector2D(
                rnd.Next(0, initialMap.GetLength(0)),
                rnd.Next(0, initialMap.GetLength(1))
            ))
        |> List.map (fun pos -> (to3D pos) + Vector3(0.f, 1f * float32(rnd.Next(2,7)), 0.f))
            
            
    member _.Render(map:WordMap, camera:FollowCamera) =
        let icam = camera :> ICamera
        let bindViewAndLight (shader:Shader) =
            shader.Use()
        
            shader.SetUniform("spotLight.position", camera.CameraPosition)
            shader.SetUniform("spotLight.direction", camera.Front)
            shader.SetUniform("view", icam.GetViewMatrix())
            shader.SetUniform("projection", icam.GetProjectionMatrix())
            
            lights |> Seq.iteri (fun ind position ->
                shader.SetUniform($"pointLights[{ind}].position", position)
            )
        
        floorTexture.Bind(TextureUnit.Texture0)
        containerSpecularTexture.Bind(TextureUnit.Texture1)
        bindViewAndLight floorShader
        let model = {
            Transform.Identity with
                Scale = float32 <| map.Map.GetLength(0)
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathH.radians 180f)
        }
        containerShader.SetUniform("model", model.ViewMatrix)
        floorVao.Bind()
        gl.DrawArrays(GLEnum.Triangles, 0, 6u)
        
        containerTexture.Bind(TextureUnit.Texture0)
        containerSpecularTexture.Bind(TextureUnit.Texture1)
        bindViewAndLight containerShader
        
        wallVao.Bind()
        walls
        |>Array2D.iter(function
            | Some model ->
                containerShader.SetUniform("model", model)
                gl.DrawArrays(GLEnum.Triangles, 0, 36u)
            | _ -> ()
        )
                
        let model = {
            Transform.Identity with
                Position = map.Player3
                Rotation = Quaternion.CreateFromYawPitchRoll(MathH.radians 45f, MathH.radians 45f, MathH.radians 0f)
        }
        containerShader.SetUniform("model", model.ViewMatrix)
        gl.DrawArrays(GLEnum.Triangles, 0, 36u)

        
    interface IDisposable with
        member _.Dispose() = resources |> Array.iter (fun x -> x.Dispose())
