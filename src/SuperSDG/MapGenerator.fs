module SuperSDG.MapGenerator

open System

type WordMap = {
    Map: char[,]
}

let private createEmptyMapWithBorder size =
    Array2D.init size size (fun i j ->
        if i = 0 || j = 0 || i = size - 1 || j = size - 1
        then '#'
        else '.'
    )
    
let private generateWalls (map:char[,]) seed wallCount =
    let rand = Random(seed)
    let l1, l2 = map.GetLength(0), map.GetLength(1)
    
    let dir = [-1,0; 0,-1; 1,0; 0,1]
    let getNeighbor (map:char[,]) x y =
        dir |> List.map (fun (dx, dy) -> x+dx, y+dy)
        |> List.filter (fun (x, y) -> 0<=x && x<l1 && 0<=y && y<l2)
    let countNeighbors (map:char[,]) x y =
        getNeighbor map x y
        |> List.filter (fun (x, y) -> map[x, y] = '#')
        |> List.length
        
    let isGoodPosition x y =
        let m = Array2D.init l1 l2 (fun i j -> map[i,j])
        m[x,y] <- '#'
        let isBusyNeighbor =
            getNeighbor m x y
            |> List.map (fun (x, y) -> countNeighbors m x y)
            |> List.exists (fun n -> n > 3)
        if isBusyNeighbor || countNeighbors m x y > 3
        then false
        else
            let mutable zones = 0
            let rec fill x y =
                m[x,y] <- '#'
                getNeighbor m x y
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
        if c = '#' then
            if countNeighbors map i j > 3
            then map[i, j] <- '.'
    )
    map |> Array2D.iteri (fun i j c ->
        if c = '.' then
            if countNeighbors map i j < 2 && isGoodPosition i j
            then map[i, j] <- '#'
    )
    
let createMap size seed =
    let map = createEmptyMapWithBorder (size+2)
    generateWalls map seed (size * size * 2 / 5)
    { Map = map }
