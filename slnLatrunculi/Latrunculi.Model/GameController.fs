namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

[<StructuralEquality;NoComparison>]
type GameLoopCycleResult =
    | Continue
    | Finished

type GameController(gameModel: GameModel) = 

    member private this.Model = gameModel
    
    member this.changePlayerSettingsFromPlayers (white: Player.T) (black: Player.T) =
        let playerSettings = unwrapResultExn <| PlayerSettings.tryCreate white black
        this.Model.changePlayerSettings playerSettings

    member this.changePlayerSettings (types: Player.PlayerTypes) (names: Player.PlayerNames) (levels: Player.PlayerLevels) =
        let white = Player.create (fst types) (fst names) (fst levels) Piece.Colors.White this.Model.Board
        let black = Player.create (snd types) (snd names) (snd levels) Piece.Colors.Black this.Model.Board
        this.changePlayerSettingsFromPlayers white black

    member this.NewGame() =
        this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
        this.Model.initBoard() |> ignore
            
    member private this.GameLoopCycle() =
        async {
            do! Async.Sleep(500)
            //Async.

            return Continue }    

    member private this.GameLoop() =
        async {
            use! cancelHandler = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused GamePausedStatus.PausedByUser) |> ignore)

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

