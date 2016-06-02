module BasicTypesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let BasicTypesTest() =

    let whitePiece = Piece.create Piece.Colors.White
    let blackPiece = Piece.create Piece.Colors.Black
    Assert.AreEqual(whitePiece.Color, Piece.Colors.White)
    Assert.AreEqual(blackPiece.Color, Piece.Colors.Black)
    Assert.AreEqual(whitePiece, Piece.createWhite)
    Assert.AreEqual(blackPiece, Piece.createBlack)

    Assert.IsTrue(match Square.createEmpty with
                    | Square.Nothing -> true
                    | Square.Piece (_) -> false)
    Assert.IsTrue(match Square.createWithPiece whitePiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.White)
    Assert.IsTrue(match Square.createWithPiece blackPiece with
                    | Square.Nothing -> false
                    | Square.Piece (p) -> p.Color = Piece.Colors.Black)


    Assert.IsTrue(match Coord.tryCreate 'Z' 1 with
                    | Error e -> e = Coord.InvalidColumnNumber
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'A' 0 with
                    | Error e -> e = Coord.InvalidRowNumber
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'B' 3 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'B') && (s.Row = Coord.RowNumber 3)
                    | _ -> false)

    Assert.IsTrue(match Coord.tryCreateFromString null with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "ZZZ" with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "AA" with
                    | Error e -> e = Coord.UnableToParseCoordFromString
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "Z1" with
                    | Error e -> e = Coord.InvalidColumnNumber
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "B8" with
                    | Error e -> e = Coord.InvalidRowNumber
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'C' 4 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'C') && (s.Row = Coord.RowNumber 4)
                    | _ -> false)


    let invalidCoord = Coord.tryCreateFromString "ZZZ"
    let srcCoord = Coord.tryCreate 'A' 1
    let tarCoord = Coord.tryCreate 'B' 2
    let squareEmpty = Square.createEmpty
    let squareWithWhitePiece = Square.createWithPiece whitePiece
    Assert.IsTrue(match Move.tryCreate srcCoord srcCoord squareEmpty squareWithWhitePiece with
                    | Error e -> e = Move.SourceAndTargetMayNotBeSame
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreate invalidCoord tarCoord squareEmpty squareWithWhitePiece with
                    | Error e -> e = Move.InvalidSourceCoordSpecified
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreate srcCoord invalidCoord squareEmpty squareWithWhitePiece with
                    | Error e -> e = Move.InvalidTargetCoordSpecified
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreate srcCoord tarCoord squareEmpty squareWithWhitePiece with
                    | Success s -> 
                        (s.Source = getObjExn srcCoord) && 
                        (s.Target = getObjExn tarCoord) &&
                        (s.NewSourceSquare = squareEmpty) &&
                        (s.NewTargetSquare = squareWithWhitePiece) &&
                        (List.length s.RemovedPieces = 0)
                    | _ -> false)
    let removedPiece = RemovedPiece.create (getObjExn srcCoord) whitePiece
    Assert.IsTrue(match Move.tryCreateWithRemovedPiecesList srcCoord tarCoord squareEmpty squareWithWhitePiece [removedPiece] with
                    | Success s -> 
                        (s.Source = getObjExn srcCoord) && 
                        (s.Target = getObjExn tarCoord) &&
                        (s.NewSourceSquare = squareEmpty) &&
                        (s.NewTargetSquare = squareWithWhitePiece) &&
                        (List.head s.RemovedPieces = removedPiece)
                    | _ -> false)


    ()
 