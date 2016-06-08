﻿namespace Latrunculi.Model

type GameModel() = 
    
    let board = match Board.tryInit Board.create Rules.GetInitialBoardSquares with
                | Error e -> failwith (sprintf "Desku se nepodařilo zinicializovat: %A" e)
                | Success s -> s
       
    let playerSettings = PlayerSettings.createDefault

    member val Board = board
    member val PlayerSettings = playerSettings
