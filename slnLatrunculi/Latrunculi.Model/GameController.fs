namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

[<StructuralEquality;NoComparison>]
type GameLoopCycleResult =
    | Continue
    | Finished

[<StructuralEquality;NoComparison>]
type GameLoopStatus =
    | WaitingForHumanPlayerCoords

module GameController =
    type Error = 
        | UnableToSwapActiveColor
        | UnableToInitializeBoard

    type T(model: GameModel.T) = 

        member private this.Model = model
    
        member this.changePlayerSettingsFromPlayers (white: Player.T) (black: Player.T) =
            let playerSettings = unwrapResultExn <| PlayerSettings.tryCreate white black
            this.Model.changePlayerSettings playerSettings

        member this.changePlayerSettings (types: Player.PlayerTypes) (names: Player.PlayerNames) (levels: Player.PlayerLevels) =
            let white = Player.create (fst types) (fst names) (fst levels) Piece.Colors.White this.Model.Board
            let black = Player.create (snd types) (snd names) (snd levels) Piece.Colors.Black this.Model.Board
            this.changePlayerSettingsFromPlayers white black

        member this.TryNewGame() =
            this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
            match this.Model.tryInitBoard() with
            | Success s -> Success this
            | Error e -> Error UnableToInitializeBoard
            
        member private this.GameLoopCycle() =
            //let waitForPlayerMove =
                //Player.tryGetMove this.Model.getActivePlayer
            async {
//                Async.RunSynchronously(waitForPlayerMove) 
                do! Async.Sleep(5000)

                this.Model.ReportGameError UnableToSwapActiveColor
                return Continue }

//                match this.Model.trySwapActiveColor with
//                | Error e -> 
//                    this.Model.ReportGameError UnableToSwapActiveColor
//                    return Finished
//                | _ -> return Continue }    

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

    let create (model: GameModel.T) =
        T(model)