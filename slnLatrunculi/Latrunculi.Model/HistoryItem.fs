namespace Latrunculi.Model

module HistoryItem =

    [<NoEquality;NoComparison>]
    type T = {
        ID: int;
        BoardMove: BoardMove.T; }
       
    let create (id: int) (boardMove: BoardMove.T) =
        { ID = id; BoardMove = boardMove }

