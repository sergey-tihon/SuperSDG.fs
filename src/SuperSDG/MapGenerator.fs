module SuperSDG.MapGenerator

open System
open System.Numerics
open Silk.NET.Maths

let to3D(target:Vector2D<int>) =
    Vector3(0.5f + float32(target.X), 0.5f, -float32(target.Y)-0.5f)
    
type WordMap = {
    Map: char[,]
    
    Player2: Vector2D<int>
    Exit2: Vector2D<int>
    
    Player3: Vector3
}

module Mover =
    open SuperSDG.Engine
    open System.Numerics
    
    let private moveVectors = [
        Vector2D(-1,0)  // Forward
        Vector2D(0,1)   // Right
        Vector2D(1, 0)  // Backward
        Vector2D(0, -1) // Left
    ]
    let private getDirectionOffset = function
        | Forward -> 0
        | Right -> 1
        | Backward -> 2
        | Left -> 3
        
    let private moveVectorsF =
        moveVectors |> List.map (fun v -> Vector2(float32 v.X, float32 v.Y))
        
    let private getCurrentForward (front:Vector3) =
        let front = Vector2(front.X, -front.Z)
        moveVectorsF
        |> List.mapi (fun i v -> i, Vector2.Dot(front, v))
        |> List.maxBy snd
        |> fst
        
    let getDirection front direction =
        let currentForward = getCurrentForward front
        let offset = getDirectionOffset direction
        moveVectors[(currentForward + offset) % 4]
    
let private generateWalls size (rand:Random) wallCount =
    let isBorder i j = i = 0 || j = 0 || i = size - 1 || j = size - 1
    let map = Array2D.init size size (fun i j ->
        if isBorder i j then '#' else '.')
    let l1, l2 = map.GetLength(0), map.GetLength(1)
    
    let dir = [-1,0; 0,-1; 1,0; 0,1]
    let getNeighbor x y =
        dir
        |> List.map (fun (dx, dy) -> x+dx, y+dy)
        |> List.filter (fun (x, y) -> 0<=x && x<l1 && 0<=y && y<l2)
    let countNeighbors (map:char[,]) x y =
        getNeighbor x y
        |> List.filter (fun (x, y) -> map[x, y] = '#')
        |> List.length
        
    let isGoodPosition x y =
        let m = Array2D.init l1 l2 (fun i j -> map[i,j])
        m[x,y] <- '#'
        let isBusyNeighbor =
            getNeighbor x y
            |> List.map (fun (x, y) -> countNeighbors m x y)
            |> List.exists (fun n -> n > 3)
        if isBusyNeighbor || countNeighbors m x y > 3
        then false
        else
            let mutable zones = 0
            let rec fill x y =
                m[x,y] <- '#'
                getNeighbor x y
                |> List.iter (fun (x, y) -> if m[x,y] = '.' then fill x y)
            m |> Array2D.iteri (fun x y c ->
                if c = '.' then
                    fill x y
                    zones <- zones+1
            )
            zones = 1
    
    let rec loop cnt =
        if cnt > 0 then
            let i = rand.Next(0, l1 - 1)
            let j = rand.Next(0, l2 - 1)
            if (map[i, j] = '.' && isGoodPosition i j) then
                map[i, j] <- '#'
                loop (cnt - 1)
            else
                loop cnt
    loop wallCount
        
    map |> Array2D.iteri (fun i j c ->
        if c = '#' && not(isBorder i j) then
            if countNeighbors map i j >= 3
            then map[i, j] <- '.'
    )
    map |> Array2D.iteri (fun i j c ->
        if c = '.' then
            if countNeighbors map i j < 2 && isGoodPosition i j
            then map[i, j] <- '#'
    )
    map
    
let rec private pickStart (map:char[,]) (rand:Random) =
    let x = rand.Next(map.GetLength(0))
    let y = rand.Next(map.GetLength(1))
    if map[x,y] = '.' then Vector2D(x,y)
    else pickStart map rand
    
let createMap size seed =
    let rand = Random(seed)
    let map = generateWalls (size+2) rand (size * size * 2 / 5)
    let start = pickStart map rand
    {
        Map = map
        Exit2 = Vector2D<int>(size, size)
        Player2 = start
        Player3 = to3D(start)
    }
