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
    let rec loop cnt =
        if cnt > 0 then
            let i = rand.Next(0, map.GetLength(0) - 1)
            let j = rand.Next(0, map.GetLength(1) - 1)
            if (map[i, j] = '.') then
                map[i, j] <- '#'
                loop (cnt - 1)
            else
                loop cnt
    loop wallCount
    
let createMap size seed =
    let map = createEmptyMapWithBorder (size+2)
    generateWalls map seed (size * size * 2 / 5)
    { Map = map }
