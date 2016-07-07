namespace Latrunculi.Controller
open Latrunculi.Model
open System
open System.Threading

module ReplayController =

    type T(model: ReplayModel.T) = 

        member private this.Model = model
        member val private cts: CancellationTokenSource option = None with get, set

        member private this.TryGetCts() =
            match this.cts with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.cts then 
                this.cts.Value.Dispose()
            this.cts <- Some (new CancellationTokenSource())
            Success (Option.get this.cts)

        member this.tryPause() =
            let checkGameStatus =
                if this.Model.Status <> ReplayStatus.Running then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryGetCts()
                cts.Cancel()
                this.Model.setStatus ReplayStatus.Paused |> ignore
                return this }

        member this.tryResume() =
            let checkGameStatus =
                if this.Model.Status = ReplayStatus.Running then Error GameIsAlreadyRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateCts()
                this.Model.setStatus ReplayStatus.Running |> ignore
                Async.Start(this.RunTimer(), cts.Token)
                return this }
                
        member this.tryGoToPosition (id: int): Result<T, Error> =
            match this.Model.Status with
            | ReplayStatus.Created -> this.Model.setStatus(ReplayStatus.Paused) |> ignore
            | _ -> ()
            maybe {
                let board = Board.set this.Model.Board Rules.getInitialBoardSquares
                match id with
                | 0 ->
                    this.Model.RaiseBoardChanged()
                    this.Model.setResult Rules.NoResult |> ignore
                    this.Model.setPosition(id) |> ignore
                    return this
                | id when id < this.Model.getNumberOfMovesInHistory() ->
                    let boardMoves = this.Model.Board.History.takeBoardMoves id
                    let boardMoveFn = Board.move board
                    List.iter boardMoveFn boardMoves

                    this.Model.RaiseBoardChanged()
                    this.Model.setPosition(id) |> ignore
                    this.Model.setResult Rules.NoResult |> ignore
                    return this
                | _ ->
                    let boardMoves = this.Model.Board.History.takeBoardMoves id
                    let boardMoveFn = Board.move board
                    List.iter boardMoveFn boardMoves

                    this.Model.RaiseBoardChanged()
                    this.Model.setStatus(ReplayStatus.Paused) |> ignore
                    this.Model.setPosition(id) |> ignore
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    return this }

        member this.tryIncPosition(): Result<T, Error> =
            this.tryGoToPosition (this.Model.Position + 1)

        member this.tryDecPosition(): Result<T, Error> =
            this.tryGoToPosition (this.Model.Position - 1)

        member this.RunTimer() =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setStatus(ReplayStatus.Paused) |> ignore) 
                use timer = new System.Timers.Timer(this.Model.Interval)
                timer.Start()
                let! result = Async.AwaitEvent(timer.Elapsed)

                match this.tryIncPosition() with
                | Success _ ->
                    match this.Model.Status with
                    | ReplayStatus.Running ->
                        return! this.RunTimer()
                    | _ -> return ()
                | Error RequestedHistoryMoveNotFound ->
                    this.Model.setStatus ReplayStatus.Paused |> ignore
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    return ()
                | Error e ->
                    this.Model.setStatus ReplayStatus.Paused |> ignore
                    this.Model.RaiseGameErrorEvent(e)
                    return () }

    let tryCreate (model: ReplayModel.T): Result<T, Error> =
        maybe {
            return T(model) }
