namespace Latrunculi.Model

module Brain =

    module Depth =
        [<StructuralEquality;StructuralComparison>]
        type T = T of int
        let create (x: int) =
            if x < 0 then invalidArg "x" "Hloubka výpočtu nesmí být menší než nula."
                else T x
        let isZero (depth: T) =
            match depth with
            | T d when d = 0 -> true
            | _ -> false
        let dec (depth: T) =
            let (T value) = depth
            T (value - 1)

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

    let evaluatePosition (position: MoveTree.Position): MoveValue.T =
        let calcValue =
            let board = position.Board
            let ownPieces = Board.whitePiecesCount board
            let enemyPieces = Board.blackPiecesCount board
            let mutable result = MoveValue.getZero
           
            match position.Result with
            | Rules.GameOverResult r ->
                match r with
                | Rules.Victory v when v = Rules.WhiteWinner -> result <- MoveValue.getMax
                | Rules.Victory v when v = Rules.BlackWinner -> result <- MoveValue.getMin
                | Rules.Draw -> result <- MoveValue.add result -20
            | Rules.NoResult ->
                result <- MoveValue.add result (ownPieces * 5 - enemyPieces * 5)

            result

        match position.ActivePlayerColor with
        | Piece.Colors.White -> calcValue
        | Piece.Colors.Black -> MoveValue.getInvValue <| calcValue
        | _ -> MoveValue.getZero

    let rec minimax (node: MoveTree.T) (depth: Depth.T) (searchType: SearchType.T): Async<MoveValue.T> =
        async {
            if (Depth.isZero depth) || (MoveTree.isTerminalNode node)
                then return evaluatePosition <| MoveTree.getPosition node
            else match searchType with
                    | SearchType.Maximizing ->
                        let mutable bestValue = MoveValue.getMin
                        for child in MoveTree.getChildren node do
                            let! v = minimax child (Depth.dec depth) (SearchType.swap searchType)
                            bestValue <- max bestValue v
                        return bestValue
                    | SearchType.Minimizing ->
                        let mutable bestValue = MoveValue.getMax
                        for child in MoveTree.getChildren node do
                            let! v = minimax child (Depth.dec depth) (SearchType.swap searchType)
                            bestValue <- min bestValue v
                        return bestValue }

    let tryGetBestMove (board: Board.T) (color: Piece.Colors): Async<Result<Move.T, Error>> =
        async {
            let depth = Depth.create 3
            let initialPosition = MoveTree.createPosition board color Rules.NoResult
            let root = MoveTree.createLeaf initialPosition

            let getBoardMoveExnFn = Rules.getBoardMoveExn board color
            let moves = List.map getBoardMoveExnFn <| Rules.getValidMoves board color 
                        
            match List.isEmpty moves with
            | true -> return Error NoValidMoveExists
            | false ->
                let mutable bestValue = MoveValue.getMin
                let mutable bestMove = List.head moves

                for move in moves do
                    Board.move board move

                    // TODO: change color + get game result after move
                    let position = MoveTree.createPosition board color Rules.NoResult

                    let child = MoveTree.createWithChildAdded root position
                    // TODO: search type minim/maximi
                    let! value = minimax child depth SearchType.createMaximizing
                    if value > bestValue then
                        bestValue <- value
                        bestMove <- move

                return Success bestMove.Move }

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

//
//01 function minimax(node, depth, maximizingPlayer)
//02     if depth = 0 or node is a terminal node
//03         return the heuristic value of node
//
//04     if maximizingPlayer
//05         bestValue := −∞
//06         for each child of node
//07             v := minimax(child, depth − 1, FALSE)
//08             bestValue := max(bestValue, v)
//09         return bestValue
//
//10     else    (* minimizing player *)
//11         bestValue := +∞
//12         for each child of node
//13             v := minimax(child, depth − 1, TRUE)
//14             bestValue := min(bestValue, v)
//15         return bestValue
// (* Initial call for maximizing player *)
//minimax(origin, depth, TRUE)