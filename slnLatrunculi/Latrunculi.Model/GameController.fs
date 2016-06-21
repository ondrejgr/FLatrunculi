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

        member val private humanSelectedMove: HumanSelectedMove.T option = None with get, set
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
                let request = HumanSelectedMove.create()
                this.humanSelectedMove <- Some request

                this.Model.setStatus(GameStatus.WaitingForHumanPlayerMove) |> ignore
                let! move = Async.AwaitEvent(request.HumanMoveSelected)
                return move.Move }

        member this.TrySetSelectedMove move =
            match this.humanSelectedMove with
            | Some hsm -> 
                hsm.SetMove move
                Success move
            | _ -> Error HumanSelectedMoveRequestDoesNotExists

        member this.TryClearHistoryBoard() =
            maybe {
                let! historyBoard = Board.tryInit this.Model.HistoryBoard Rules.getInitialBoardSquares
                return this }

        member this.TryNewGame() =
            maybe {
                Player.Board <- Some this.Model.Board
                Player.getHumanPlayerMoveFromUIWorkflow <- Some this.GetHumanMoveFromUI

                // reset game result and set active player
                this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
                this.Model.ResetNumberOfMovesWithoutRemoval() |> ignore
                this.Model.setResult Rules.NoResult |> ignore

                // init board with default positions
                let! board = Board.tryInit this.Model.Board Rules.getInitialBoardSquares
                let! historyBoard = Board.tryInit this.Model.HistoryBoard Rules.getEmptyBoardSquares
                this.Model.RaiseBoardChanged()
                this.Model.RaiseHistoryCleared()
                return this }
            
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
                    // create move with removed pieces list
                    let! color = this.Model.tryGetActiveColor()
                    let! boardMove = Rules.tryValidateAndGetBoardMove this.Model.Board color move

                    // apply move and check number of removed pieces
                    Board.move this.Model.Board boardMove |> ignore
                    this.Model.RaiseHistoryItemAdded(Board.addHistoryItem this.Model.Board color boardMove)
                    this.Model.RaiseBoardChanged()
                    ignore <| match BoardMove.anyPiecesRemoved boardMove with
                                | true -> this.Model.ResetNumberOfMovesWithoutRemoval()
                                | false -> this.Model.IncNumberOfMovesWithoutRemoval()

                    // check game over
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board this.Model.NumberOfMovesWithoutRemoval) |> ignore
                    match this.Model.Result with
                    | Rules.GameOverResult _ ->
                        return Finished
                    | Rules.NoResult ->
                        // continue
                        do! this.Model.trySwapActiveColor()
                        return Continue }
            async {
                match getPlayerMoveWorkflow with
                | Success getPlayerMove ->
                    // get player move
                    let! moveResult = getPlayerMove()
                    match moveResult with
                    | Success move ->
                        // apply move
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
                this.Model.RaiseMoveSuggestionComputed(move)
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

        member this.TryMoveExistsForCoord(coord): Result<bool, Error> =
            let tryGetValidMovesForCoord color =
                let move = Seq.tryHead <| Rules.getValidMovesForCoord this.Model.Board color coord
                match move with
                | Some _ -> true
                | None _ -> false
            maybe {
                let! color = this.Model.tryGetActiveColor()
                let result = tryGetValidMovesForCoord color
                return result }

        member this.TryGetValidMove (src: Coord.T) (tar: Coord.T): Result<Move.T, Error> =
            maybe {
                let! color = this.Model.tryGetActiveColor()
                let! move = match Seq.tryFind (fun (m: Move.T) -> m.Source = src && m.Target = tar) 
                                        <| Rules.getValidMovesForCoord this.Model.Board color src with
                            | Some m -> Success m
                            | _ -> Error NoValidMoveExists
                return move }

        member this.GetPossibleTargetCoords (src: Coord.T) =
            match this.Model.tryGetActiveColor() with
            | Success color ->
                        System.Collections.Generic.List<Coord.T>(Seq.map (fun (coord: Move.T) -> coord.Target) 
                                <| Rules.getValidMovesForCoord this.Model.Board color src)
            | _ -> System.Collections.Generic.List<Coord.T>(List.empty)

    let create (model: GameModel.T) =
        T(model)