namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

[<StructuralEquality;NoComparison>]
type GameLoopCycleResult =
    | Continue
    | Finished

type GameController(gameModel: GameModel) = 

    member private this.Model = gameModel
    
    member this.changePlayerSettings (white, black) =
        this.Model.changePlayerSettings (white, black)

    member this.NewGame(white, black) =
        this.Model.changePlayerSettings(white, black) |> ignore
        this.Model.setActiveColor(Some Rules.GetInitialActiveColor) |> ignore
        this.Model.initBoard() |> ignore
            
    member private this.GameLoopCycle() =
        async {
            do! Async.Sleep(500)

            return Continue }    

    member private this.GameLoop() =
        async {
            use! cancelHandler = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused) |> ignore)

            let! result = this.GameLoopCycle()
            match result with
            | Continue -> return! this.GameLoop()
            | Finished -> 
                this.Model.setStatus(GameStatus.Finished) |> ignore
                return () }

    member this.Run() =
        let cts = new CancellationTokenSource()
        this.Model.setStatus(GameStatus.Running) |> ignore
        Async.Start(this.GameLoop(), cts.Token)
        cts

