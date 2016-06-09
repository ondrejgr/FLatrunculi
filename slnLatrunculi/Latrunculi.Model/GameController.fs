namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

type GameController(gameModel: GameModel) = 

    member private this.Model = gameModel
    
    member this.changePlayerSettings (white, black) =
        this.Model.changePlayerSettings (white, black)

    member this.NewGame(white, black) =
        this.Model.changePlayerSettings(white, black) |> ignore
        this.Model.setActiveColor(Some Rules.GetInitialActiveColor) |> ignore
        this.Model.initBoard() |> ignore

    member private this.RunGame() =
        async {
            this.Model.setStatus(GameStatus.Running) |> ignore
            Thread.Sleep(5000)
            this.Model.setStatus(GameStatus.Paused) |> ignore
        }

    member this.Run(cts: CancellationTokenSource) =
        Async.Start (this.RunGame(), cts.Token)
