namespace Latrunculi.Model

module Brain =

    let rnd = System.Random()

    module MiniMaxState =
        [<StructuralEquality;NoComparison>]
        type MiniMaxData = {
            V: MoveValue.T
            Alpha: MoveValue.T
            Beta: MoveValue.T }

        [<StructuralEquality;NoComparison>]
        type T = 
            | Exit of MoveValue.T
            | Recurse of MiniMaxData

        let getResult x =
            match x with
            | Exit v -> v
            | Recurse data -> data.V

        let createRecurse initialV initialAlpha initialBeta =
            Recurse { V = initialV; Alpha = initialAlpha; Beta = initialBeta }

        let createExit currentV =
            Exit currentV

    module SearchType =
        [<StructuralEquality;NoComparison>]
        type T =
            | Maximizing
            | Minimizing

        let createMaximizing =
            Maximizing
        let createMinimizing =
            Minimizing

        let swap (x: T) =
            match x with
            | Maximizing -> createMinimizing
            | Minimizing -> createMaximizing

    let getPositionEvaluation (position: MoveTree.Position.T): MoveValue.T =
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
            result <- MoveValue.getInvValue result
        | _ -> ()

//        if List.length board.History <= 3 then
//            result <- MoveValue.add result <| rnd.Next(0, 8)
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
                        //TODO: Board.addMoveToHistory board move
                        let victory = Rules.checkVictory board 
                        // create child position
                        let childPosition = MoveTree.Position.create (unwrapResultExn <| Board.tryClone board) childColor victory
                        let child = MoveTree.createLeaf childPosition
                        // revert move
                        Board.invmove board move
                        //TODO: Board.removeMoveFromHistory board
                        // add child to node    
                        match result with
                            | MoveTree.RootNode _ ->
                                MoveTree.getRootNodeWithChildAdded result child move.Move                         
                            | MoveTree.LeafNode _ | MoveTree.InnerNode _ -> 
                                MoveTree.getNodeWithChildAdded result child) 
                    node boardMoves

    let rec minimax (depth: Depth.T) (alpha: MoveValue.T) (beta: MoveValue.T) (n: MoveTree.T) (searchType: SearchType.T): MoveValue.T =
        if (Depth.isZero depth) || (MoveTree.isGameOverNode n) then 
            let position = MoveTree.getPosition n
            let result = getPositionEvaluation position
            printfn "  Minimax depth %A got evaluation for %A: %A" depth n result
            result
        else
            printfn "       Minimax depth %A is computing %A children of %A" depth searchType n
            let node = getNodeWithChildren n 

            let initialV = match searchType with
                            | SearchType.Maximizing -> MoveValue.getMin
                            | SearchType.Minimizing -> MoveValue.getMax

            let initialState = MiniMaxState.createRecurse initialV alpha beta
            let result = MiniMaxState.getResult 
                            <| match searchType with
                                | SearchType.Maximizing ->
                                    List.fold (fun state (child: MoveTree.T) ->
                                                match state with
                                                | MiniMaxState.Recurse data ->
                                                    let v = max data.V <| minimax (Depth.dec depth) data.Alpha data.Beta child (SearchType.swap searchType)
                                                    let alpha = max data.Alpha v    
                                                    let beta = data.Beta
                                                    if beta <= alpha then
                                                        MiniMaxState.createExit v
                                                    else
                                                        MiniMaxState.createRecurse v alpha beta
                                                | _ -> state)
                                          initialState <| MoveTree.getChildren node
                                | SearchType.Minimizing ->
                                    List.fold (fun state (child: MoveTree.T) ->
                                                match state with
                                                | MiniMaxState.Recurse data ->
                                                    let v = min data.V <| minimax (Depth.dec depth) data.Alpha data.Beta child (SearchType.swap searchType)
                                                    let alpha = data.Alpha
                                                    let beta = min data.Beta v    
                                                    if beta <= alpha then
                                                        MiniMaxState.createExit v
                                                    else
                                                        MiniMaxState.createRecurse v alpha beta
                                                | _ -> state)
                                          initialState <| MoveTree.getChildren node
            printfn "              Result %A" result
            result

    let tryGetBestMove (b: Board.T) (color: Piece.Colors) (depth: Depth.T): Async<Result<Move.T, Error>> =
        async {
            try
                printfn "Best move computation started..."
                let board = unwrapResultExn <| Board.tryClone b
                let rootPosition = MoveTree.Position.create board color <| Rules.checkVictory board
                // create root node
                let root = getNodeWithChildren <| MoveTree.createRoot rootPosition

                let bestMove = snd 
                                <| (List.fold (fun result data ->
                                    let bestValue = fst result
                                    let move = fst data
                                    let child = snd data
                                    let value = minimax (Depth.dec depth) MoveValue.getMin MoveValue.getMax child <| SearchType.createMaximizing
                                    if value > bestValue then
                                        printfn "Move %A got value %A and is best move " move value
                                        (value, Some move)
                                    else
                                        printfn "Move %A got value %A and is rejected " move value
                                        result
                                    ) (MoveValue.getMin, None) <| MoveTree.getRootNodeChildren root)
                       
                match bestMove with
                | Some m -> 
                    printfn "Best move computation finished, best move %A..." m
                    return Success m
                | None -> 
                    printfn "Best move computation failed."
                    return Error NoValidMoveExists
            with
            | e -> return Error (BrainException e.Message)
        }


