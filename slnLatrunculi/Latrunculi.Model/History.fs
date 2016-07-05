namespace Latrunculi.Model

module HistoryItem = 
    [<StructuralEquality;NoComparison>]
    type T = {
        ID: int;
        Move: BoardMove.T }

    let create id move =
        { ID = id; Move = move}

module History =

    [<NoEquality;NoComparison>]
    type T() =
        member val UndoStack = MoveStack.create with get, set
        member val RedoStack = MoveStack.create with get, set

        member private this.setUndoStack (stack: MoveStack.T) =
            this.UndoStack <- stack

        member private this.setRedoStack (stack: MoveStack.T) =
            this.RedoStack <- stack

        member this.ClearRedoStack() =
            this.RedoStack <- MoveStack.create

        member private this.tryPopMoveFromStack (target: MoveStack.T -> unit, stack: MoveStack.T) =
            maybe {
                let! popResult = MoveStack.tryPop stack
                target <| fst popResult
                let boardMove = snd popResult
                return boardMove }

        member this.tryPopMoveFromUndoStack() =
            this.tryPopMoveFromStack(this.setUndoStack, this.UndoStack)

        member this.tryPopMoveFromRedoStack() =
            this.tryPopMoveFromStack(this.setRedoStack, this.RedoStack)

        member private this.PushMoveToStack (target: MoveStack.T -> unit, stack: MoveStack.T, move: BoardMove.T) =
            target <| MoveStack.push stack move

        member this.PushMoveToUndoStack (move: BoardMove.T) =
            this.PushMoveToStack(this.setUndoStack, this.UndoStack, move)

        member this.PushMoveToRedoStack (move: BoardMove.T) =
            this.PushMoveToStack(this.setRedoStack, this.RedoStack, move)

        member this.Items =
            let undoItemsResult = MoveStack.foldBack (fun item result ->
                                                        let id = 1 + fst result
                                                        let newItem = HistoryItem.create id item
                                                        (id, newItem::(snd result)))
                                                    this.UndoStack (0, [])
            let undoRedoItemsResult = MoveStack.fold (fun result item ->
                                                        let id = 1 + fst result
                                                        let newItem = HistoryItem.create id item
                                                        (id, newItem::(snd result)))
                                                    undoItemsResult this.RedoStack
            snd undoRedoItemsResult

        member this.UndoItemsCount =
            MoveStack.length this.UndoStack

        member this.RedoItemsCount =
            MoveStack.length this.RedoStack

        member this.NumberOfMovesWithoutRemoval =
            MoveStack.fold (fun result item ->
                                if BoardMove.anyPiecesRemoved item then 0 else result + 1)
                        0 this.UndoStack

    let isUndoStackEmpty (history: T) =
        MoveStack.isEmpty history.UndoStack

    let isRedoStackEmpty (history: T) =
        MoveStack.isEmpty history.RedoStack

    let clone (source: T) =
        let result = T()
        result.UndoStack <- source.UndoStack
        result.RedoStack <- source.RedoStack
        result
        
    let create() =
        T()