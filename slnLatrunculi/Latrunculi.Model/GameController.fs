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
        member val private ctsMoveSuggestion: CancellationTokenSource option = None with get, set
        member private this.Model = model
    
        member this.changePlayerSettingsFromPlayers (white: Player.T) (black: Player.T) =
            let playerSettings = unwrapResultExn <| PlayerSettings.tryCreate white black
            this.Model.changePlayerSettings playerSettings

        member this.changePlayerSettings (types: Player.PlayerTypes) (names: Player.PlayerNames) (levels: Player.PlayerLevels) =
            let white = Player.create (fst types) (fst names) (fst levels) Piece.Colors.White
            let black = Player.create (snd types) (snd names) (snd levels) Piece.Colors.Black
            this.changePlayerSettingsFromPlayers white black

        member private this.GetHumanMoveFromUI() =
            async {
                this.Model.setStatus(GameStatus.WaitingForHumanPlayerMove) |> ignore
                let request = HumanSelectedMove.create
                let! move = Async.AwaitEvent(request.HumanMoveSelected)
                return move.Move }

        member this.TryNewGame() =
            Player.Board <- Some model.Board
            Player.getHumanPlayerMoveFromUIWorkflow <- Some this.GetHumanMoveFromUI

            this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
            match this.Model.tryInitBoard() with
            | Success s -> Success this
            | Error _ -> Error UnableToInitializeBoard
            
        member private this.ReportGameError e =
            this.Model.setStatus(GameStatus.Paused) |> ignore
            this.Model.RaiseGameErrorEvent(e)

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
                use! cnl = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused) |> ignore)

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

        member private this.TryGetCts() =
            match this.cts with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.cts then 
                this.cts.Value.Dispose()
            this.cts <- Some (new CancellationTokenSource())
            Success (Option.get this.cts)

        member private this.TryGetMoveSuggestionCts() =
            match this.ctsMoveSuggestion with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateMoveSuggestionCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.ctsMoveSuggestion then
                this.ctsMoveSuggestion.Value.Dispose()
            this.ctsMoveSuggestion <- Some (new CancellationTokenSource())
            Success (Option.get this.ctsMoveSuggestion)

        member this.TryCancelSuggestMove() =
            maybe {
                let! cts = this.TryGetMoveSuggestionCts()
                cts.Cancel()
                return this }

        member private this.SuggestMove(color) =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setIsMoveSuggestionComputing false |> ignore)
                let! move = Brain.tryGetBestMove this.Model.Board color
                this.Model.setIsMoveSuggestionComputing false |> ignore
                this.Model.RaiseMoveSuggestionComputedEvent(move)
                () }

        member this.TrySuggestMove() =
            let checkGameStatus =
                if this.Model.IsMoveSuggestionComputing 
                    then Error MoveSuggestionAlreadyComputing 
                    else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateMoveSuggestionCts()
                let! color = this.Model.tryGetActiveColor()
                this.Model.setIsMoveSuggestionComputing true |> ignore
                Async.Start(this.SuggestMove(color), cts.Token)
                return this }

        member this.TryPause() =
            maybe {
                do! (match this.Model.IsMoveSuggestionComputing with
                    | true -> Success (ignore <| this.TryCancelSuggestMove())
                    | false -> Success ())
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
                if Seq.contains this.Model.Status 
                    <| seq { 
                            yield GameStatus.Running 
                            yield GameStatus.WaitingForHumanPlayerMove } 
                    then Error GameIsAlreadyRunning 
                    else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateCts()
                this.Model.setStatus(GameStatus.Running) |> ignore
                Async.Start(this.GameLoop(), cts.Token)
                return this }

    let create (model: GameModel.T) =
        T(model)