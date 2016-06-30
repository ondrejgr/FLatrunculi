namespace Latrunculi.Model

module Brain =

    let rnd = System.Random()

    module SearchType =
        [<StructuralEquality;NoComparison>]
        type T =
            | Maximizing of Piece.Colors
            | Minimizing of Piece.Colors
        let getColor (x: T) =
            match x with
            | Maximizing c | Minimizing c -> c
        let createMaximizing (x: Piece.Colors) =
            Maximizing x
        let createMinimizing (x: Piece.Colors) =
            Minimizing x
        let swap (x: T) =
            match x with
            | Maximizing c -> createMinimizing <| Piece.swapColor c
            | Minimizing c -> createMaximizing <| Piece.swapColor c

    let evaluatePosition (position: MoveTree.Position.T): MoveValue.T =
        let board = position.Board
        let mutable result = MoveValue.getZero

        let calcValue =
            // eval from white point of view
            match position.Result with
            | Rules.GameOverResult r ->
                match r with
                // eval victory
                | Rules.Victory Rules.WhiteWinner -> result <- MoveValue.getValue 1000
                | Rules.Victory Rules.BlackWinner -> result <- MoveValue.getValue -1000
                | Rules.Draw -> result <- MoveValue.add result -30
            | Rules.NoResult ->
                // eval by number of pieces
                let ownPieces = Board.whitePiecesCount board
                let enemyPieces = Board.blackPiecesCount board

                result <- MoveValue.add result ((ownPieces - enemyPieces) * 10)

        match position.ActivePlayerColor with
        | Piece.Colors.Black as color -> 
            result <- MoveValue.getInvValue <| result
        | _ -> ()

        if List.length board.History <= 3 then
            result <- MoveValue.add result <| rnd.Next(0, 8)
        result


    let getNodeWithChildren (node: MoveTree.T) =
        let nodePosition = MoveTree.getPosition node
        let board = nodePosition.Board
        let color = nodePosition.ActivePlayerColor

        // get valid moves for current position
        let getValidBoardMoveExn = Rules.getValidBoardMoveExnFn board color
        let boardMoves = List.map getValidBoardMoveExn <| Rules.getValidMoves board color

        // set color after move
        let childColor = Piece.swapColor color
        List.fold (fun result (move: BoardMove.T) ->
                        // apply move
                        Board.move board move
                        Board.addMoveToHistory board move
                        let victory = Rules.checkVictory board 
                        // create child position
                        let childPosition = MoveTree.Position.create (unwrapResultExn <| Board.tryClone board) childColor victory
                        let child = MoveTree.createLeaf childPosition
                        // revert move
                        Board.invmove board move
                        Board.removeMoveFromHistory board
                        // add child to node    
                        match result with
                            | MoveTree.RootNode _ ->
                                MoveTree.getRootNodeWithChildAdded result child move.Move                         
                            | MoveTree.LeafNode _ | MoveTree.InnerNode _ -> 
                                MoveTree.getNodeWithChildAdded result child) 
                    node boardMoves

    let rec minimax (depth: Depth.T) (a: MoveValue.T) (b: MoveValue.T) (n: MoveTree.T) (searchType: SearchType.T): MoveValue.T =
        if (Depth.isZero depth) || (MoveTree.isGameOverNode n)
            then evaluatePosition <| MoveTree.getPosition n
        else
            let mutable alpha = a
            let mutable beta = b
            let node = getNodeWithChildren n 
            match searchType with
                | SearchType.Maximizing color ->
                    let mutable v = MoveValue.getMin
                    let mutable skip = false
                    for child in MoveTree.getChildren node do
                        match skip with
                        | false ->
                            let result = minimax (Depth.dec depth) alpha beta child (SearchType.swap searchType)
                            v <- max v result
                            alpha <- max alpha v
                            if beta <= alpha then
                                skip <- true
                        | true -> ()
                    v
                | SearchType.Minimizing color ->
                    let mutable v = MoveValue.getMax
                    let mutable skip = false
                    for child in MoveTree.getChildren node do
                        match skip with
                        | false ->
                            let result = minimax (Depth.dec depth) alpha beta child (SearchType.swap searchType)
                            v <- min v result
                            beta <- min beta v
                            if beta <= alpha then
                                skip <- true
                        | true -> ()
                    v

    let tryGetBestMove (b: Board.T) (color: Piece.Colors) (depth: Depth.T): Async<Result<Move.T, Error>> =
        async {
            try
                let board = unwrapResultExn <| Board.tryClone b
                let rootPosition = MoveTree.Position.create board color <| Rules.checkVictory board
                // create root node
                let root = getNodeWithChildren <| MoveTree.createRoot rootPosition

                let bestMove = snd 
                                <| (List.fold (fun result data ->
                                    let bestValue = fst result
                                    let move = fst data
                                    let child = snd data
                                    let value = minimax (Depth.dec depth) MoveValue.getMin MoveValue.getMax child <| SearchType.createMaximizing color
                                    if value > bestValue then
                                        (value, Some move)
                                    else
                                        result
                                    ) (MoveValue.getMin, None) <| MoveTree.getRootNodeChildren root)
                       
                match bestMove with
                | Some m -> 
                    return Success m
                | None -> 
                    return Error NoValidMoveExists
            with
            | e -> return Error (BrainException e.Message)
        }


