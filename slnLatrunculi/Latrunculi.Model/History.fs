﻿namespace Latrunculi.Model

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

        member this.NumberOfMoves =
            List.fold (fun result i -> result + 1) 0 <| this.Items

        member this.NumberOfMovesWithoutRemoval =
            List.foldBack (fun item result ->
                            if BoardMove.anyPiecesRemoved item then 0 else result + 1)
                        (MoveStack.toList this.UndoStack) 0

    let setUndoStack (history: T) (stack: MoveStack.T) =
        history.UndoStack <- stack

    let setRedoStack (history: T) (stack: MoveStack.T) =
        history.RedoStack <- stack

    let clone (source: T) =
        let result = T()
        setUndoStack source source.UndoStack
        setRedoStack source source.RedoStack
        result
        
    let create() =
        T()