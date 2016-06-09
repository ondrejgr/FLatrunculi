namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

[<StructuralEquality;NoComparison>]
type GameLoopCycleResult =
    | Continue
    | Quit

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
            printfn "Start of Game Loop"
            do! Async.Sleep(500)
            printfn "End of Game Loop"

            return Continue }

    // infinite GameLoop
    member private this.GameLoop() =
        async {
            while true do
                let! result = this.GameLoopCycle()
                match result with
                | Continue -> ()
                | Quit -> return () }

    // start infinite GameLoop with cancellation support
    member this.Run(ct: CancellationToken) =
        let RunGameLoop =
            async {
                match Async.RunSynchronously <| Async.Catch(this.GameLoop()) with
                | Choice1Of2 _ -> printfn "Success"
                | Choice2Of2 exn -> printfn "Failed %s" exn.Message
                () }

        Async.Start(RunGameLoop, ct)

