namespace Latrunculi.Model

module HistoryItem =

    [<NoEquality;NoComparison>]
    type T = {
        ID: int;
        PlayerColor: Piece.Colors;
        BoardMove: BoardMove.T; }
       
    let create (id: int) (playerColor: Piece.Colors) (boardMove: BoardMove.T) =
        { ID = id; PlayerColor = playerColor; BoardMove = boardMove }

