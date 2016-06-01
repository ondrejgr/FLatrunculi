module BasicTypesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let BasicTypesTest() =

    let whitePiece = Piece.Create Piece.Colors.White
    let blackPiece = Piece.Create Piece.Colors.Black
    Assert.AreEqual(whitePiece.Color, Piece.Colors.White)
    Assert.AreEqual(blackPiece.Color, Piece.Colors.Black)

    let emptySquare = Square.CreateEmpty
    Assert.IsTrue(match emptySquare with
                    | Square.Nothing -> true
                    | Square.Piece (_) -> false);

    let squareWithWhitePiece = Square.CreateWithPiece whitePiece
    Assert.IsTrue(match squareWithWhitePiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.White);


    let squareWithBlackPiece = Square.CreateWithPiece blackPiece
    Assert.IsTrue(match squareWithBlackPiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.Black);

    let invalidColumnNumberCoord = Coord.tryCreate 'Z' 1
    Assert.IsTrue(match invalidColumnNumberCoord with
                    | Error e -> e = Coord.InvalidColumnNumber
                    | _ -> false);

    let invalidRowNumberCoord = Coord.tryCreate 'A' 0
    Assert.IsTrue(match invalidRowNumberCoord with
                    | Error e -> e = Coord.InvalidRowNumber
                    | _ -> false);

    let coord = Coord.tryCreate 'A' 1
    Assert.IsTrue(match coord with
                    | Success s -> true
                    | _ -> false);


    ()
 