namespace Latrunculi.Model

module MoveTree =

    type NodeData = {
        BoardMove: BoardMove.T option
        Board: Board.T }

 
    [<NoEquality;NoComparison>]        
    type T = 
        | LeafNode of NodeData
        | InnerNode of NodeData * T seq


    let create (depth: int) (board: Board.T) =
        let root = LeafNode { BoardMove = None; Board = Board.clone board }
        root
