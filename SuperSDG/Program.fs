open Silk.NET.Maths
open Silk.NET.Windowing
open Silk.NET.Input

let mutable options = WindowOptions.Default
options.Size <- Vector2D<int>(800, 600)
options.Title <- "SuperSDG v3"

let window = Window.Create options
window.add_Load(fun _ ->
    let input = window.CreateInput()
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown(fun keyboard key _ ->
            if key = Key.Escape then window.Close()
        )
)

window.Run();