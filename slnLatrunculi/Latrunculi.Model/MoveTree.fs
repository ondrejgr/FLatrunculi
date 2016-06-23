namespace Latrunculi.Model

module MoveTree =

    module Position =
        [<StructuralEquality;NoComparison>]
        type T = {
            Board: Board.T;
            ActivePlayerColor: Piece.Colors;
            Result: Rules.GameResult }

        let create (board: Board.T) (activePlayerColor: Piece.Colors) (result: Rules.GameResult) =
            { Board = board; ActivePlayerColor = activePlayerColor; Result = result }
        
    [<NoEquality;NoComparison>]        
    type T = 
        | RootNode of Position.T * ((Move.T * T) list)
        | LeafNode of Position.T
        | InnerNode of Position.T * T list

    let getPosition (node: T) =
        match node with
        | RootNode (position, _) | LeafNode position | InnerNode (position, _) -> position

    let getChildren (node: T) =
        match node with
        | LeafNode _ -> List.empty
        | InnerNode (_, children) -> children
        | RootNode (_, data) -> List.map snd data

    let getRootNodeChildren (node: T) =
        match node with
        | RootNode (_, children) -> children
        | _ -> invalidArg "node" "Neplatný argument, musí být předán kořen herního stromu."
        
    let isGameOverNode (node: T) =
        match node with
        | LeafNode position | InnerNode (position, _) -> 
            match position.Result with
            | Rules.GameOverResult _ -> true
            | _ -> false
        | _ -> false

    let createRoot (position: Position.T) =
        let lst: (Move.T * T) list = List.Empty
        RootNode (position, lst)

    let createLeaf (position: Position.T) =
        LeafNode position

    let getNodeWithChildAdded (parent: T) (child: T) =
        match parent with
        | RootNode _ ->
            invalidArg "parent" "Neplatný argument, volanou funkci nelze použít na kořen herního stromu."
        | LeafNode position ->
            InnerNode (position, child::[])
        | InnerNode (position, children) ->
            InnerNode (position, child::children)

    let getRootNodeWithChildAdded (parent: T) (child: T) (move: Move.T) =
        match parent with
        | RootNode (position, data) ->
            let newDataItem = (move, child)
            RootNode (position, newDataItem::data)
        | _ ->
            invalidArg "parent" "Neplatný argument, volanou funkci lze použít pouze na kořen herního stromu."

