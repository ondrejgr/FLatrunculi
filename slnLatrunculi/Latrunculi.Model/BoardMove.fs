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

