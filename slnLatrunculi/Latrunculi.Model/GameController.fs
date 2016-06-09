namespace Latrunculi.Controller
open Latrunculi.Model

type GameController(gameModel: GameModel) = 

    member private this.Model = gameModel
    
    member this.changePlayerSettings (white, black) =
        this.Model.changePlayerSettings (white, black)

    member this.LoadGame(white, black, activePlayer) =
        this.Model.changePlayerSettings(white, black) |> ignore
        this.Model.setActivePlayer(Some activePlayer) |> ignore
        this.Model.initBoard() |> ignore
