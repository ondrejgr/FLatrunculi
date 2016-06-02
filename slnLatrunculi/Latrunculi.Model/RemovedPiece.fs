namespace Latrunculi.Model

module RemovedPiece =

    [<StructuralEquality;NoComparison>]
    type T = {
        Coord: Coord.T;
        Piece: Piece.T }
    
    let create x y =
        { Coord = x; Piece = y }

