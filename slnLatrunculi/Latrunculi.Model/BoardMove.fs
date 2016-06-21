namespace Latrunculi.Model

module BoardMove =
    [<StructuralEquality;NoComparison>]
    type T = {
        Color: Piece.Colors;
        Move: Move.T;
        RemovedPieces: RemovedPiece.T list }

    let createWithRmPieces (color: Piece.Colors) (move: Move.T) pcs =
        { Color = color; Move = move; RemovedPieces = pcs}
        
    let create (color: Piece.Colors) (move: Move.T) =
        createWithRmPieces color move []

    let getSourceCoord (move: T) =
        move.Move.Source

    let getTargetCoord (move: T) =
        move.Move.Target

    let getRemovedPiecesCount (move: T) =
        List.length move.RemovedPieces

    let anyPiecesRemoved (move: T) =
        let result = not (List.isEmpty move.RemovedPieces)
        result