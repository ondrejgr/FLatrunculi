namespace Latrunculi.Model

type GameModel() = 
    let board = match Board.tryInit <| Board.create with
                | Error e -> failwith (sprintf "Desku se nepodařilo zinicializovat: %A" e)
                | Success s -> s

    member val Board = board
