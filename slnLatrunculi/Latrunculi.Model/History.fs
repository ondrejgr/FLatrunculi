namespace Latrunculi.Model

module History =

    [<NoEquality;NoComparison>]
    type T() =
        member val UndoStack = MoveStack.create with get, set
        member val RedoStack = MoveStack.create with get, set

        member this.PushMove (move: BoardMove.T) =
            this.UndoStack <- MoveStack.push this.UndoStack move

        member this.Items =
            let undoList = MoveStack.toList this.UndoStack
            let redoList = MoveStack.toList this.RedoStack
            let items = Seq.concat [undoList; redoList]
            snd <| Seq.foldBack (fun item state ->
                                let id = 1 + fst state
                                let lst = snd state
                                let newItem = HistoryItem.create id item
                                (id, newItem::lst)) 
                            items
                            (0, [])

        member this.getNumberOfMoves() =
            List.fold (fun result i -> result + 1) 0 <| MoveStack.toList this.UndoStack

        member this.getNumberOfMovesWithoutRemoval() =
            List.foldBack (fun item result ->
                            if BoardMove.anyPiecesRemoved item then 0 else result + 1)
                        (MoveStack.toList this.UndoStack) 0

    let clone (source: T) =
        let result = T()
        result.UndoStack <- source.UndoStack
        result.RedoStack <- source.RedoStack
        result
        
    let create() =
        T()