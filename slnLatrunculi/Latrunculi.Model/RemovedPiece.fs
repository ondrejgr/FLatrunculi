namespace Latrunculi.Model

module RemovedPiece =

    type T = {
        Coord: Coord.T;
        Piece: Piece.T }
    
    let create x y =
        { Coord = x; Piece = y }

