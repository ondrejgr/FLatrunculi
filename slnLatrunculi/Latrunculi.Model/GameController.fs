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

    member val index = 0 with get, set

    // GameLoopCycle returns Success true to repeat game cycle, Success false to stop game loop
    member private this.tryGameLoopCycle() =
        async {
            printfn "Start of Game Loop"
            do! Async.Sleep(5000)
            printfn "End of Game Loop"

            return Success true }

    // infinite GameLoop
    member private this.GameLoop() =
        async {
            let! result = this.tryGameLoopCycle()
            match result with
            | Error e -> return Error e
            | Success s -> if s then return! this.GameLoop() else return Success s }

    // start infinite GameLoop with cancellation support
    member this.Run(ct: CancellationToken) =
        let RunGameLoop =
            async {
                match Async.RunSynchronously <| Async.Catch(this.GameLoop()) with
                | Choice1Of2 _ -> printfn "Success"
                | Choice2Of2 exn -> printfn "Failed %s" exn.Message
                () }

        Async.Start(RunGameLoop, ct)

