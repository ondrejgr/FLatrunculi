﻿namespace Latrunculi.Model

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
        let calcValue =
            let board = position.Board
            let ownPieces = Board.whitePiecesCount board
            let enemyPieces = Board.blackPiecesCount board
            let mutable result = MoveValue.getZero
            // eval from white point of view
            match position.Result with
            | Rules.GameOverResult r ->
                match r with
                // eval victory
                | Rules.Victory Rules.WhiteWinner -> result <- MoveValue.getMax
                | Rules.Victory Rules.BlackWinner -> result <- MoveValue.getMin
                | Rules.Draw -> result <- MoveValue.add result -20
            | Rules.NoResult ->
                // first 2 moves random
                if List.length board.History < 2 then
                    result <- MoveValue.add result <| rnd.Next(0, 8)
                // eval by number of pieces
                result <- MoveValue.add result ((ownPieces - enemyPieces) * 10)
            result

        match position.ActivePlayerColor with
        | Piece.Colors.White -> calcValue
        | Piece.Colors.Black -> MoveValue.getInvValue <| calcValue

    let getNodeWithChildren (node: MoveTree.T) =
        async {
            let mutable result = node
            let nodePosition = MoveTree.getPosition result
            let board = nodePosition.Board
            let color = nodePosition.ActivePlayerColor

            // get valid moves for current position
            let getValidBoardMoveExn = Rules.getValidBoardMoveExnFn board color
            let boardMoves = 
                seq {
                    for move in Rules.getValidMoves board color do
                        yield getValidBoardMoveExn move }

            // set color after move
            let childColor = Piece.swapColor color
            for move in boardMoves do
                // apply move to the copy of a board
                let childBoard = Board.clone board
                Board.move childBoard move
                let victory = Rules.checkVictory childBoard 
                let childPosition = MoveTree.Position.create childBoard color victory
                let child = MoveTree.createLeaf childPosition
                // add child to node      
                result <- match result with
                            | MoveTree.RootNode _ ->
                                MoveTree.getRootNodeWithChildAdded result child move.Move                         
                            | MoveTree.LeafNode _ | MoveTree.InnerNode _ -> 
                                MoveTree.getNodeWithChildAdded result child
            return result }

    let rec minimax (depth: Depth.T) (n: MoveTree.T) (searchType: SearchType.T): Async<MoveValue.T> =
        async {
            if (Depth.isZero depth) || (MoveTree.isGameOverNode n)
                then return evaluatePosition <| MoveTree.getPosition n
            else
                let! node = getNodeWithChildren n 
                match searchType with
                    | SearchType.Maximizing color ->
                        let mutable bestValue = MoveValue.getMin
                        for child in MoveTree.getChildren node do
                            let! v = minimax (Depth.dec depth) child (SearchType.swap searchType)
                            bestValue <- max bestValue v
                        return bestValue
                    | SearchType.Minimizing color ->
                        let mutable bestValue = MoveValue.getMax
                        for child in MoveTree.getChildren node do
                            let! v = minimax (Depth.dec depth) child (SearchType.swap searchType)
                            bestValue <- min bestValue v
                        return bestValue }

    let tryGetBestMove (b: Board.T) (color: Piece.Colors) (depth: Depth.T): Async<Result<Move.T, Error>> =
        async {
            try
                let board = Board.clone b
                let rootPosition = MoveTree.Position.create board color <| Rules.checkVictory board
                // create root node
                let! root = getNodeWithChildren <| MoveTree.createRoot rootPosition

                let mutable bestValue = MoveValue.getMin
                let mutable bestMove: Move.T option = None

                for data in MoveTree.getRootNodeChildren root do
                    let move = fst data
                    let child = snd data
                    let! value = minimax (Depth.dec depth) child <| SearchType.createMaximizing color
                    if value > bestValue then
                        bestValue <- value
                        bestMove <- Some move
                        
                match bestMove with
                | Some m -> 
                    return Success m
                | None -> 
                    return Error NoValidMoveExists
            with
            | e -> return Error (BrainException e.Message)
        }



//function nej tah(pozice, hloubka)
//2: tahy ← generuj tahy(pozice)
//3: nejlepsi ohodnoceni ← −MAX
//4: for all tah v kolekci tahy do
//5:    potomek ← zahraj(pozice, tah)
//6:    ohodnoceni ← −minimax(potomek, hloubka − 1)
//7:    if ohodnoceni > nejlepsi ohodnoceni then
//8:        nejlepsi ohodnoceni ← ohodnoceni
//9:        nejlepsi tah ← tah
//10:   end if
//11: end for
//12: return nejlepsi tah
//13: end function
