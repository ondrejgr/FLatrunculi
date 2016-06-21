namespace Latrunculi.Model

module MoveTree =

    [<StructuralEquality;NoComparison>]
    type Position = {
        Result: Rules.GameResult;
        Board: Board.T }

    [<StructuralEquality;NoComparison>]
    type NodeData = {
        BoardMove: BoardMove.T option
        Value: MoveValue.T
        Position: Position }

    [<NoEquality;NoComparison>]        
    type T = 
        | LeafNode of NodeData
        | InnerNode of NodeData * T seq


//    let create (depth: int) (board: Board.T) =
//        let createNode =
//            
//        let root = LeafNode { BoardMove = None; Board = Board.clone board }
//        root
