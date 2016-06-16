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

    type T(model: GameModel.T) = 

        member val private cts: CancellationTokenSource option = None with get, set
        member private this.Model = model
    
        member this.changePlayerSettingsFromPlayers (white: Player.T) (black: Player.T) =
            let playerSettings = unwrapResultExn <| PlayerSettings.tryCreate white black
            this.Model.changePlayerSettings playerSettings

        member this.changePlayerSettings (types: Player.PlayerTypes) (names: Player.PlayerNames) (levels: Player.PlayerLevels) =
            let white = Player.create (fst types) (fst names) (fst levels) Piece.Colors.White
            let black = Player.create (snd types) (snd names) (snd levels) Piece.Colors.Black
            this.changePlayerSettingsFromPlayers white black

        member this.TryNewGame() =
            Player.Board <- Some model.Board
            this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
            match this.Model.tryInitBoard() with
            | Success s -> Success this
            | Error _ -> Error UnableToInitializeBoard
            
        member private this.ReportGameError e =
            this.Model.setStatus(GameStatus.Paused) |> ignore
            this.Model.ReportGameError(e)

        member private this.RequestHumanPlayerMove =
            this.Model.setStatus(GameStatus.WaitingForHumanPlayerMove) |> ignore

        member private this.GameLoopCycle(): Async<Result<GameLoopCycleResult, Error>> =
            let getPlayerMoveWorkflow =
                maybe {
                    let! player = this.Model.tryGetActivePlayer()
                    return player.TryGetMove }    
            let applyMoveAndChangePlayer move =
                maybe {
                    let! color = this.Model.tryGetActiveColor()
                    let! boardMove = Rules.tryValidateAndGetBoardMove this.Model.Board color move
                    do! this.Model.tryBoardMove boardMove
                    do! this.Model.trySwapActiveColor()
                    return Continue }
            async {
                match getPlayerMoveWorkflow with
                | Success getPlayerMove ->
                    let! moveResult = getPlayerMove()
                    match moveResult with
                    | Success move ->
                        return applyMoveAndChangePlayer move
                    | Error e ->
                        return Error e
                | Error e ->
                    return Error e }

        member private this.GameLoop(): Async<unit> =
            async {
                use! cancelHandler = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused) |> ignore)

                let! a = this.GameLoopCycle()
                match a with
                | Success result ->
                    match result with
                    | Continue -> return! this.GameLoop()
                    | Finished -> 
                        this.Model.setStatus(GameStatus.Finished) |> ignore
                        return ()
                | Error e ->
                    this.ReportGameError e
                    return () }

        member this.TryGetCts() =
            match this.cts with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member this.TryCreateCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.cts then 
                this.cts.Value.Dispose()
                this.cts <- None
            this.cts <- Some (new CancellationTokenSource())
            Success (Option.get this.cts)


        member this.TryPause() =
            let checkGameStatus =
                if this.Model.Status <> GameStatus.Running then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryGetCts()
                cts.Cancel()
                return this }

        member this.TryResume() =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Running then Error GameIsAlreadyRunning else Success ()
            maybe {
                do! checkGameStatus
                return! this.TryRun() }

        member this.TryRun() =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Running then Error GameIsAlreadyRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateCts()
                this.Model.setStatus(GameStatus.Running) |> ignore
                Async.Start(this.GameLoop(), cts.Token)
                return this }

    let create (model: GameModel.T) =
        T(model)