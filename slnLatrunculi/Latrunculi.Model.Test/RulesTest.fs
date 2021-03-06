﻿module RulesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let RulesTest() =
    let model = unwrapResultExn <| GameModel.tryCreate()
    let board = model.Board
    let controller = GameController.create model
    ignore <| (unwrapResultExn <| controller.TryNewGame())

    let empty = Square.createEmpty
    let white = Square.createWithPiece <| Piece.createWhite
    let black = Square.createWithPiece <| Piece.createBlack
    
    // non-empty target
    Assert.IsFalse(Rules.isMoveValid board Piece.Colors.White <| Move.createFromStrCoordExn "A1" "A2" empty white)
    Assert.IsFalse(Rules.isMoveValid board Piece.Colors.Black <| Move.createFromStrCoordExn "A1" "A2" empty white)

    // valid moves
    Assert.IsTrue(Rules.isMoveValid board Piece.Colors.White <| Move.createFromStrCoordExn "A2" "A3" empty white)
    Assert.IsTrue(Rules.isMoveValid board Piece.Colors.Black <| Move.createFromStrCoordExn "B6" "B5" empty black)
    // only own piece allowed
    Assert.IsFalse(Rules.isMoveValid board Piece.Colors.Black <| Move.createFromStrCoordExn "A2" "A3" empty white)

    // no diagonal
    Assert.IsFalse(Rules.isMoveValid board Piece.Colors.White <| Move.createFromStrCoordExn "B2" "C3" empty white)
    Assert.IsFalse(Rules.isMoveValid board Piece.Colors.White <| Move.createFromStrCoordExn "B2" "A3" empty white)

    // capture in line
    Board.move board <| BoardMove.create Piece.Colors.White (unwrapResultExn <| Move.tryCreateFromStrCoord "B2" "B5" empty white)
    Board.move board <| BoardMove.create Piece.Colors.White (unwrapResultExn <| Move.tryCreateFromStrCoord "A6" "A5" empty white)

    // voluntary passing between enemies - do not capture
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 6 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 4 } -> black

                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white

                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> empty
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 3 } -> white

                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> empty
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'B' 4
    let c2 = unwrapResultExn <| Coord.tryCreate 'B' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty black
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.Black m
    Board.move board move
    Assert.AreEqual(black, Board.getSquare board c2)

    // white captures black
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 6 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> black
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 3 } -> empty

                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> white
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'C' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'C' 3
    let c = unwrapResultExn <| Coord.tryCreate 'B' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board c)
    
    // capture in corner
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 6 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> black

                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> empty
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'B' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'A' 2
    let c = unwrapResultExn <| Coord.tryCreate 'A' 1
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board c)

    // white captures 2 black pieces
    //        o
    //        x
    //      ox <-o
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 3 } -> empty



                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 4 } -> black
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> black

                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 5 } -> white
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> white
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'C' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'C' 3
    let cx = unwrapResultExn <| Coord.tryCreate 'C' 4
    let cy = unwrapResultExn <| Coord.tryCreate 'B' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board cx)    
    Assert.AreEqual(empty, Board.getSquare board cy)    
    
    // white should not capture
    //        o
    //        x
    //      ox xo
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 3 } -> empty

                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'E'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'E'; Row = Coord.RowNumber 4 } -> empty
                            
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 4 } -> black
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> black
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 3 } -> black

                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 5 } -> white
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> white
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'C' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'C' 3
    let cx = unwrapResultExn <| Coord.tryCreate 'C' 4
    let cy = unwrapResultExn <| Coord.tryCreate 'B' 3
    let cz = unwrapResultExn <| Coord.tryCreate 'D' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board cx)    
    Assert.AreEqual(empty, Board.getSquare board cy)    
    Assert.AreEqual(black, Board.getSquare board cz)    

    // white takes 3 black pieces
    //        o
    //        xS
    //      ox xo
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 3 } -> empty

                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'E'; Row = Coord.RowNumber 5 } -> empty
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 4 } -> empty
                            | { Column = Coord.ColumnNumber 'E'; Row = Coord.RowNumber 4 } -> empty
                            
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 4 } -> black
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> black
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 3 } -> black

                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 5 } -> white
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> white
                            | { Column = Coord.ColumnNumber 'E'; Row = Coord.RowNumber 3 } -> white
                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'C' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'C' 3
    let cx = unwrapResultExn <| Coord.tryCreate 'C' 4
    let cy = unwrapResultExn <| Coord.tryCreate 'B' 3
    let cz = unwrapResultExn <| Coord.tryCreate 'D' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board cx)    
    Assert.AreEqual(empty, Board.getSquare board cy)    
    Assert.AreEqual(empty, Board.getSquare board cz)    

    // 3 pieces + piece combined
    //    |  
    //    |oo  
    //    |x xo      
    //    ------------
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 2 } -> empty
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 2 } -> empty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 1 } -> empty

                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> black
                            | { Column = Coord.ColumnNumber 'C'; Row = Coord.RowNumber 1 } -> black
                                                        
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> white
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 2 } -> white
                            | { Column = Coord.ColumnNumber 'D'; Row = Coord.RowNumber 1 } -> white

                            | _ -> Rules.getInitialBoardSquares c)
    let c1 = unwrapResultExn <| Coord.tryCreate 'B' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'B' 1
    let cx = unwrapResultExn <| Coord.tryCreate 'C' 1
    let cy = unwrapResultExn <| Coord.tryCreate 'A' 1
    let cz = unwrapResultExn <| Coord.tryCreate 'B' 1
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    Board.move board move
    Assert.AreEqual(empty, Board.getSquare board cx)    
    Assert.AreEqual(empty, Board.getSquare board cy)    
    Assert.AreEqual(white, Board.getSquare board cz)   

    let board = Board.create()
    // no winner -game continues
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> black
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> black
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | _ -> Square.createEmpty )
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.NoResult -> true
                    | _ -> false)
    // black winner
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> black
                            | _ -> Square.createEmpty )
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.GameOverResult v when v = Rules.Victory Rules.BlackWinner -> true
                    | _ -> false)
    // white winner
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> white
                            | _ -> Square.createEmpty )
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.GameOverResult v when v = Rules.Victory Rules.WhiteWinner -> true
                    | _ -> false)

    // create some fake history
    ignore <| Board.init board Rules.getInitialBoardSquares
    let c1 = unwrapResultExn <| Coord.tryCreate 'A' 2
    let c2 = unwrapResultExn <| Coord.tryCreate 'A' 3
    let m = unwrapResultExn <| Move.tryCreate c1 c2 empty white
    let move = unwrapResultExn <| Rules.tryValidateAndGetBoardMove board Piece.Colors.White m
    let lst: History.T = History.create()
    for i = 1 to 30 do
        lst.PushMoveToUndoStack move
    // black winner -30 moves
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> black
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> black
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | _ -> Square.createEmpty )
    board.History <- lst
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.GameOverResult v when v = Rules.Victory Rules.BlackWinner -> true
                    | _ -> false)
    // white winner -30 moves
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> white
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> black
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 3 } -> white
                            | _ -> Square.createEmpty )
    board.History <- lst
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.GameOverResult v when v = Rules.Victory Rules.WhiteWinner -> true
                    | _ -> false)
    // draw
    ignore <| Board.init board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 1 } -> white
                            | { Column = Coord.ColumnNumber 'A'; Row = Coord.RowNumber 2 } -> black
                            | _ -> Square.createEmpty )
    board.History <- lst
    Assert.IsTrue(match Rules.checkVictory board with
                    | Rules.GameOverResult v when v = Rules.Draw -> true
                    | _ -> false)

    ()


