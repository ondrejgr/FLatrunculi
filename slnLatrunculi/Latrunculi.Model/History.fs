namespace Latrunculi.Model

module History =

    [<NoEquality;NoComparison>]
    type T() =
        member val UndoStack = MoveStack.create with get, set
        member val RedoStack = MoveStack.create with get, set

        member this.ClearRedoStack() =
            this.RedoStack <- MoveStack.create

        member this.PushMoveToUndoStack (move: BoardMove.T) =
            this.UndoStack <- MoveStack.push this.UndoStack move

        member this.Items =
            let undoList = MoveStack.toList this.UndoStack
            let redoList = MoveStack.toList this.RedoStack
            List.concat [undoList; redoList]

        member this.UndoItems =
            MoveStack.toList this.UndoStack

        member this.UndoItemsCount =
            List.length this.UndoItems

        member this.NumberOfMovesWithoutRemoval =
            List.fold (fun result item ->
                            if BoardMove.anyPiecesRemoved item then 0 else result + 1)
                        0 (MoveStack.toList this.UndoStack)

    let tryTakeUndoStackItems (x: int) (history: T)  =
        match x with
        | x when x < 1 -> Error RequestedHistoryMoveNotFound
        | x when x > history.UndoItemsCount -> Error RequestedHistoryMoveNotFound
        | _ ->
            Success (Seq.toList <| Seq.take x history.UndoItems)

    let setUndoStack (history: T) (stack: MoveStack.T) =
        history.UndoStack <- stack

    let setRedoStack (history: T) (stack: MoveStack.T) =
        history.RedoStack <- stack

    let isUndoStackEmpty (history: T) =
        MoveStack.isEmpty history.UndoStack

    let isRedoStackEmpty (history: T) =
        MoveStack.isEmpty history.RedoStack

    let clone (source: T) =
        let result = T()
        setUndoStack source source.UndoStack
        setRedoStack source source.RedoStack
        result
        
    let create() =
        T()