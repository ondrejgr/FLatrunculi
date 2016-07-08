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

    type getPositionEvaluationFunction = MoveTree.Position.T -> MoveValue.T
    let getPositionEvaluationForColor (maximizingColor: Piece.Colors) (position: MoveTree.Position.T): MoveValue.T =
        let board = position.Board
        let mutable result = MoveValue.getZero

        // eval from white point of view
        match position.Result with
        | Rules.GameOverResult r ->
            match r with
            // eval victory
            | Rules.Victory Rules.WhiteWinner -> result <- MoveValue.getMax
            | Rules.Victory Rules.BlackWinner -> result <- MoveValue.getMin
            | Rules.Draw -> result <- MoveValue.add result -30
        | Rules.NoResult ->
            // eval by number of pieces
            let ownPieces = Board.whitePiecesCount board
            let enemyPieces = Board.blackPiecesCount board
            result <- MoveValue.add result ((ownPieces - enemyPieces) * 10)

            // add random number
            match board.History.UndoItemsCount <= 4 with
            | true ->
                match result with
                | x when x = MoveValue.getZero ->
                    result <- MoveValue.add result <| rnd.Next(5, 16)
                | _ -> ()
            | false -> ()


        match maximizingColor with
        | Piece.Colors.Black as color -> 
            result <- MoveValue.getInvValue result
        | _ -> ()

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
                        board.History.PushMoveToUndoStack(move)
                        let victory = Rules.checkVictory board 
                        // create child position
                        let childPosition = MoveTree.Position.create (Board.clone board) childColor victory
                        let child = MoveTree.createLeaf childPosition
                        // revert move
                        board.History.tryPopMoveFromUndoStack() |> ignore
                        Board.invmove board move
                        // add child to node    
                        match result with
                            | MoveTree.RootNode _ ->
                                MoveTree.getRootNodeWithChildAdded result child move.Move                         
                            | MoveTree.LeafNode _ | MoveTree.InnerNode _ -> 
                                MoveTree.getNodeWithChildAdded result child) 
                    node boardMoves

    let rec minimax (getPositionEvaluation: getPositionEvaluationFunction) (depth: Depth.T) (alpha: MoveValue.T) (beta: MoveValue.T) (n: MoveTree.T) (searchType: SearchType.T): Async<MoveValue.T> =
        async {
            if (Depth.isZero depth) || (MoveTree.isGameOverNode n) then 
                let position = MoveTree.getPosition n
                let result = getPositionEvaluation position
                return result
            else
                let node = getNodeWithChildren n 

                let initialV = match searchType with
                                | SearchType.Maximizing -> MoveValue.getMin
                                | SearchType.Minimizing -> MoveValue.getMax

                let initialState = MiniMaxState.createRecurse initialV alpha beta

                let compute (state: MiniMaxState.T) (child: MoveTree.T) =
                    async {
                        match searchType with
                        | SearchType.Maximizing ->
                            match state with
                            | MiniMaxState.Recurse data ->
                                let! newV = minimax getPositionEvaluation (Depth.dec depth) data.Alpha data.Beta child (SearchType.swap searchType)
                                let v = max data.V newV
                                let alpha = max data.Alpha v    
                                let beta = data.Beta
                                if beta <= alpha then
                                    return MiniMaxState.createExit v
                                else
                                    return MiniMaxState.createRecurse v alpha beta
                            | _ -> return state 
                        | SearchType.Minimizing ->
                                match state with
                                | MiniMaxState.Recurse data ->
                                    let! newV = minimax getPositionEvaluation (Depth.dec depth) data.Alpha data.Beta child (SearchType.swap searchType)
                                    let v = min data.V newV
                                    let alpha = data.Alpha
                                    let beta = min data.Beta v    
                                    if beta <= alpha then
                                        return MiniMaxState.createExit v
                                    else
                                        return MiniMaxState.createRecurse v alpha beta
                                | _ -> return state }

                let mutable state = initialState
                for child in MoveTree.getChildren node do
                    let! newState = compute state child
                    state <- newState

                return MiniMaxState.getResult state }

    let tryGetBestMove (board: Board.T) (color: Piece.Colors) (depth: Depth.T): Async<Result<Move.T, Error>> =
        async {
            try
                
                let getPositionEvaluation = getPositionEvaluationForColor color
                let rootPosition = MoveTree.Position.create (Board.clone board) color <| Rules.checkVictory board
                // create root node
                let root = getNodeWithChildren <| MoveTree.createRoot rootPosition

                let compute result data =
                    async {
                        let bestValue = fst result
                        let move = fst data
                        let child = snd data
                        let! value = minimax getPositionEvaluation (Depth.dec depth) MoveValue.getMin MoveValue.getMax child <| SearchType.createMaximizing
                        if value > bestValue then
                            return (value, Some move)
                        else
                            return result }

                let mutable state = (MoveValue.getMin, None)
                for data in MoveTree.getRootNodeChildren root do
                    let! newState = compute state data
                    state <- newState
                       
                let bestMove = snd state
                match bestMove with
                | Some m -> 
                    return Success m
                | None -> 
                    return Error NoValidMoveExists
            with
            | e -> return Error (BrainException e.Message) }


