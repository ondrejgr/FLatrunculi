namespace Latrunculi.Model

module HistoryItem =

    [<StructuralEquality;NoComparison>]
    type T = {
        PlayerColor: Piece.Colors;
        Move: Move.T;
        RemovedPiecesCount: int; }
       
    let create (playerColor: Piece.Colors) (boardMove: BoardMove.T) =
        let count = List.length boardMove.RemovedPieces
        { PlayerColor = playerColor; Move = boardMove.Move; RemovedPiecesCount = count }

