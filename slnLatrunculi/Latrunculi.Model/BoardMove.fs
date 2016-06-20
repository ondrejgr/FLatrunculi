namespace Latrunculi.Model

module BoardMove =
    [<StructuralEquality;NoComparison>]
    type T = {
        Move: Move.T;
        RemovedPieces: RemovedPiece.T list }

    let createWithRmPieces (move: Move.T) pcs =
        { Move = move; RemovedPieces = pcs}
        
    let create (move: Move.T) =
        createWithRmPieces move []

    let getSourceCoord (move: T) =
        move.Move.Source

    let getTargetCoord (move: T) =
        move.Move.Target

    let getRemovedPiecesCount (move: T) =
        List.length move.RemovedPieces

    let anyPiecesRemoved (move: T) =
        let result = not (List.isEmpty move.RemovedPieces)
        result