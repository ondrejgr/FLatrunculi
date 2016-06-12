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
                    | Error Coord.InvalidColumnNumber -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'A' 0 with
                    | Error Coord.InvalidRowNumber -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'B' 3 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'B') && (s.Row = Coord.RowNumber 3)
                    | _ -> false)

    Assert.IsTrue(match Coord.tryCreateFromString null with
                    | Error Coord.UnableToParseCoordFromString -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "ZZZ" with
                    | Error Coord.UnableToParseCoordFromString -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "AA" with
                    | Error Coord.UnableToParseCoordFromString -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "Z1" with
                    | Error Coord.InvalidColumnNumber -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreateFromString "B8" with
                    | Error Coord.InvalidRowNumber -> true
                    | _ -> false)
    Assert.IsTrue(match Coord.tryCreate 'C' 4 with
                    | Success s -> (s.Column = Coord.ColumnNumber 'C') && (s.Row = Coord.RowNumber 4)
                    | _ -> false)


    Assert.Throws(fun () -> Coord.tryCreateFromString "ZZZ" |> unwrapResultExn |> ignore) |> ignore
    let srcCoord = unwrapResultExn <| Coord.tryCreate 'A' 1
    let tarCoord = unwrapResultExn <| Coord.tryCreate 'B' 2
    let squareEmpty = Square.createEmpty
    let squareWithWhitePiece = Square.createWithPiece whitePiece
    Assert.IsTrue(match Move.tryCreate srcCoord srcCoord squareEmpty squareWithWhitePiece with
                    | Error Move.SourceAndTargetMayNotBeSame -> true
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreateFromStringCoords "ZZZ" "B2" squareEmpty squareWithWhitePiece with
                    | Error Move.InvalidSourceCoord -> true
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreateFromStringCoords "A1" "ZZZ" squareEmpty squareWithWhitePiece with
                    | Error Move.InvalidTargetCoord -> true
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreate srcCoord tarCoord squareEmpty squareWithWhitePiece with
                    | Success s -> 
                        (s.Source = srcCoord) && 
                        (s.Target = tarCoord) &&
                        (s.NewSourceSquare = squareEmpty) &&
                        (s.NewTargetSquare = squareWithWhitePiece) &&
                        (List.length s.RemovedPieces = 0)
                    | _ -> false)
    let removedPiece = RemovedPiece.create srcCoord whitePiece
    Assert.IsTrue(match Move.tryCreateWithRemovedPiecesList srcCoord tarCoord squareEmpty squareWithWhitePiece [removedPiece] with
                    | Success s -> 
                        (s.Source = srcCoord) && 
                        (s.Target = tarCoord) &&
                        (s.NewSourceSquare = squareEmpty) &&
                        (s.NewTargetSquare = squareWithWhitePiece) &&
                        (List.head s.RemovedPieces = removedPiece)
                    | _ -> false)


    let board = Board.create
    let boardCoord1 = unwrapResultExn <| Coord.tryCreate 'B' 1
    let boardCoord2 = unwrapResultExn <| Coord.tryCreate 'B' 3
    let boardMove = unwrapResultExn <| Move.tryCreate boardCoord1 boardCoord2 squareEmpty squareWithWhitePiece
    Assert.Throws(fun () -> (Move.tryCreate boardCoord1 boardCoord1 squareEmpty squareWithWhitePiece) |> unwrapResultExn |> ignore) |> ignore
    Board.move board boardMove
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord1)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord2)
    
    let whitePlayer = PlayerSettings.createHumanPlayer "White" Levels.Easy :> Player
    let blackPlayer = PlayerSettings.createComputerPlayer "Black" Levels.Hard :> Player
    Assert.IsTrue(whitePlayer :? HumanPlayer)
    Assert.IsTrue(blackPlayer :? ComputerPlayer)

    let model = new GameModel()
    let controller = GameController(model)
    let playerSettings = model.changePlayerSettings(whitePlayer, blackPlayer)
    Assert.AreEqual(playerSettings, model.PlayerSettings)
    Assert.AreEqual(whitePlayer.Name, playerSettings.WhitePlayer.Name)
    Assert.AreEqual(whitePlayer.Level, playerSettings.WhitePlayer.Level)
    Assert.AreEqual(blackPlayer.Name, playerSettings.BlackPlayer.Name)
    Assert.AreEqual(blackPlayer.Level, playerSettings.BlackPlayer.Level)
    Assert.IsTrue(playerSettings.WhitePlayer :? HumanPlayer)
    Assert.IsTrue(playerSettings.BlackPlayer :? ComputerPlayer)


    ()
 