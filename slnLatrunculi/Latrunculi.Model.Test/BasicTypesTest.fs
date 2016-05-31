module BasicTypesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let BasicTypesTest() =

    let whitePiece = Piece.Create PieceColors.White
    let blackPiece = Piece.Create PieceColors.Black
    Assert.AreEqual(whitePiece.Color, PieceColors.White)
    Assert.AreEqual(blackPiece.Color, PieceColors.Black)

    let emptySquare = Square.CreateEmpty
    Assert.IsTrue(match emptySquare with
                    | Square.Nothing -> true
                    | Square.Piece (_) -> false);

    let squareWithWhitePiece = Square.CreateWithPiece whitePiece
    Assert.IsTrue(match squareWithWhitePiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = PieceColors.White);


    let squareWithBlackPiece = Square.CreateWithPiece blackPiece
    Assert.IsTrue(match squareWithBlackPiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = PieceColors.Black);




    Assert.AreEqual(true, true)
 