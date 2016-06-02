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

    Assert.IsTrue(match Square.CreateEmpty with
                    | Square.Nothing -> true
                    | Square.Piece (_) -> false);
    Assert.IsTrue(match Square.CreateWithPiece whitePiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.White);
    Assert.IsTrue(match Square.CreateWithPiece blackPiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.Black);


    Assert.IsTrue(match Coord.tryCreate 'Z' 1 with
                    | Error e -> e = Coord.InvalidColumnNumber
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreate 'A' 0 with
                    | Error e -> e = Coord.InvalidRowNumber
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreate 'B' 3 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'B') && (s.Row = Coord.RowNumber 3)
                    | _ -> false);

    Assert.IsTrue(match Coord.tryCreateFromString null with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreateFromString "ZZZ" with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreateFromString "AA" with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreateFromString "Z1" with
                    | Error e -> e = Coord.InvalidColumnNumber
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreateFromString "B8" with
                    | Error e -> e = Coord.InvalidRowNumber
                    | _ -> false);
    Assert.IsTrue(match Coord.tryCreate 'C' 4 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'C') && (s.Row = Coord.RowNumber 4)
                    | _ -> false);


    ()
 