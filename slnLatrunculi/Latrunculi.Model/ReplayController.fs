namespace Latrunculi.Controller
open Latrunculi.Model
open System
open System.Threading

module ReplayController =

    type T(model: ReplayModel.T) = 

        member private this.Model = model
        member val private cts: CancellationTokenSource option = None with get, set

        member this.tryIncPosition(x: int) =
            let tryMoveFound (newId: int) (historyItems: HistoryItem.T list) =
                match List.tryHead historyItems with
                | Some i when i.ID = newId -> Success ()
                | _ -> Error RequestedHistoryMoveNotFound
            maybe {
                let! board = Board.trySet this.Model.Board Rules.getInitialBoardSquares
                let newId = x + 1

                match newId with
                | newId when newId > this.Model.getNumberOfMovesInHistory() ->
                    this.Model.RaiseBoardChanged()
                    this.Model.RaisePositionChangedEvent(x)
                    return x

                | newId ->
                    let historyItems = List.filter (fun (a: HistoryItem.T) -> a.ID <= newId) this.Model.Board.History
                    do! tryMoveFound newId historyItems
                    do! List.foldBack (fun (item: HistoryItem.T) result ->
                                    Board.move board item.BoardMove
                                    Success ()) historyItems (Error RequestedHistoryMoveNotFound) 
                    this.Model.RaiseBoardChanged()
                    this.Model.RaisePositionChangedEvent(newId)
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    match newId with
                    | newId when newId = this.Model.getNumberOfMovesInHistory() ->
                        this.Model.setStatus(ReplayStatus.Finished) |> ignore
                        return newId
                    | _ ->
                        return newId }

        member this.tryDecPosition(x: int) =
            let tryMoveFound (newId: int) (historyItems: HistoryItem.T list) =
                match List.tryHead historyItems with
                | Some i when i.ID = newId -> Success ()
                | _ -> Error RequestedHistoryMoveNotFound
            maybe {
                let! board = Board.trySet this.Model.Board Rules.getInitialBoardSquares
                let newId = x - 1

                match newId with
                | 0 -> 
                    this.Model.RaiseBoardChanged()
                    this.Model.RaisePositionChangedEvent(0)
                    return 0
                | newId ->
                    let historyItems = List.filter (fun (a: HistoryItem.T) -> a.ID <= newId) this.Model.Board.History
                    do! tryMoveFound newId historyItems
                    do! List.foldBack (fun (item: HistoryItem.T) result ->
                                    Board.move board item.BoardMove
                                    Success ()) historyItems (Error RequestedHistoryMoveNotFound) 
                    this.Model.RaiseBoardChanged()
                    this.Model.RaisePositionChangedEvent(newId)
                    return newId }

        member private this.TryGetCts() =
            match this.cts with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.cts then 
                this.cts.Value.Dispose()
            this.cts <- Some (new CancellationTokenSource())
            Success (Option.get this.cts)

        member this.RunTimer(id: int) =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setStatus(ReplayStatus.Paused) |> ignore) 
                use timer = new System.Timers.Timer(this.Model.Interval)
                timer.Enabled <- true
                let! result = Async.AwaitEvent(timer.Elapsed)
                timer.Start()
                match this.tryIncPosition(id) with
                | Success newId ->
                    match newId with
                    | newId when newId = -1 -> 
                        this.Model.setStatus ReplayStatus.Finished |> ignore
                        return ()
                    | newId -> return! this.RunTimer(newId)
                | Error e ->
                    this.Model.RaiseGameErrorEvent(e)
                    return () }

        member this.tryPause() =
            let checkGameStatus =
                if this.Model.Status <> ReplayStatus.Running then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryGetCts()
                cts.Cancel()
                this.Model.setStatus ReplayStatus.Paused |> ignore
                return this }

        member this.tryResume(id: int) =
            let checkGameStatus =
                if this.Model.Status = ReplayStatus.Running then Error GameIsAlreadyRunning else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateCts()
                this.Model.setStatus ReplayStatus.Running |> ignore
                Async.Start(this.RunTimer(id), cts.Token)
                return this }
                
        member this.tryGoToPosition (id: int) =
            let tryMoveFound (historyItems: HistoryItem.T list) =
                match List.tryHead historyItems with
                | Some i when i.ID = id -> Success ()
                | _ -> Error RequestedHistoryMoveNotFound
            let tryPause =
                maybe {
                    match this.Model.Status with
                    | ReplayStatus.Running ->
                        let! controller = this.tryPause()
                        return ()
                    | _ ->
                        return () }
                    
            maybe {
                do! tryPause
                let! board = Board.trySet this.Model.Board Rules.getInitialBoardSquares
                match id with
                | 0 ->
                    this.Model.RaiseBoardChanged()
                    this.Model.setStatus ReplayStatus.Paused |> ignore
                    this.Model.RaisePositionChangedEvent(id)
                    return this
                | _ ->
                    let historyItems = List.filter (fun (a: HistoryItem.T) -> a.ID <= id) this.Model.Board.History
                    do! tryMoveFound historyItems
                    do! List.foldBack (fun (item: HistoryItem.T) result ->
                                    Board.move board item.BoardMove
                                    Success ()) historyItems (Error RequestedHistoryMoveNotFound) 

                    this.Model.RaiseBoardChanged()
                    this.Model.setStatus ReplayStatus.Paused |> ignore
                    this.Model.RaisePositionChangedEvent(id)
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    return this }

    let tryCreate (model: ReplayModel.T): Result<T, Error> =
        maybe {
            return T(model) }
