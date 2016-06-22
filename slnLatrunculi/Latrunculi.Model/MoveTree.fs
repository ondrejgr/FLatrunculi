namespace Latrunculi.Model

module MoveTree =

    [<StructuralEquality;NoComparison>]
    type Position = {
        Board: Board.T;
        ActivePlayerColor: Piece.Colors;
        Result: Rules.GameResult }
        
    [<NoEquality;NoComparison>]        
    type T = 
        | LeafNode of Position
        | InnerNode of Position * T list

    let getPosition (node: T) =
        match node with
        | LeafNode position | InnerNode (position, _) -> position

    let getChildren (node: T) =
        match node with
        | LeafNode _ -> List.empty
        | InnerNode (_, children) -> children

    let isGameOverNode (node: T) =
        match node with
        | LeafNode position | InnerNode (position, _) -> 
            match position.Result with
            | Rules.GameOverResult _ -> true
            | _ -> false
        | _ -> false

    let createPosition (board: Board.T) (activePlayerColor: Piece.Colors) (result: Rules.GameResult) =
        { Board = board; ActivePlayerColor = activePlayerColor; Result = result }

    let createLeaf (position: Position) =
        LeafNode position

    let getNodeWithChildAdded (parent: T) (child: T) =
        match parent with
        | LeafNode position ->
            InnerNode (position, child::[])
        | InnerNode (position, children) ->
            InnerNode (position, child::children)

