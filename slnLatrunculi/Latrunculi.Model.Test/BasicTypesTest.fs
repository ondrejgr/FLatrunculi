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
    Assert.IsTrue(match Move.tryCreateFromStrCoord "ZZZ" "B2" squareEmpty squareWithWhitePiece with
                    | Error Move.InvalidSourceCoord -> true
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreateFromStrCoord "A1" "ZZZ" squareEmpty squareWithWhitePiece with
                    | Error Move.InvalidTargetCoord -> true
                    | _ -> false)
    Assert.IsTrue(match Move.tryCreate srcCoord tarCoord squareEmpty squareWithWhitePiece with
                    | Success s -> 
                        (s.Source = srcCoord) && 
                        (s.Target = tarCoord) &&
                        (s.NewSourceSquare = squareEmpty) &&
                        (s.NewTargetSquare = squareWithWhitePiece)
                    | _ -> false)

    // physical moves (without rules checking)
    let board = Board.create
    let boardCoord1 = unwrapResultExn <| Coord.tryCreate 'B' 1
    let boardCoord2 = unwrapResultExn <| Coord.tryCreate 'B' 3
    let boardCoord3 = unwrapResultExn <| Coord.tryCreate 'B' 4
    let boardCoord4 = unwrapResultExn <| Coord.tryCreate 'B' 5
    ignore <| (unwrapResultExn <| Board.tryInit board (fun c ->
                            match c with
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 1 } -> squareWithWhitePiece
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 3 } -> squareEmpty
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 4 } -> squareWithWhitePiece
                            | { Column = Coord.ColumnNumber 'B'; Row = Coord.RowNumber 5 } -> squareWithWhitePiece
                            | _ -> squareEmpty))
    // create invalid move
    Assert.Throws(fun () -> (Move.tryCreate boardCoord1 boardCoord1 squareEmpty squareWithWhitePiece) |> unwrapResultExn |> ignore) |> ignore
    // move with pieces removal
    let move = unwrapResultExn <| Move.tryCreate boardCoord1 boardCoord2 squareEmpty squareWithWhitePiece
    let rmPieces = (RemovedPiece.create boardCoord3 whitePiece)::(RemovedPiece.create boardCoord4 whitePiece)::[]
    let boardMove = BoardMove.createWithRmPieces move rmPieces
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord1)
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord2)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord3)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord4)
    Board.move board boardMove
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord1)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord2)
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord3)
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord4)
    // inv move
    Board.invmove board boardMove
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord1)
    Assert.AreEqual(squareEmpty, Board.getSquare board boardCoord2)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord3)
    Assert.AreEqual(squareWithWhitePiece, Board.getSquare board boardCoord4)

    let whitePlayer = Player.createHumanPlayer "White" Player.Levels.Easy Piece.Colors.White :> Player.T
    let blackPlayer = Player.createComputerPlayer "Black" Player.Levels.Hard Piece.Colors.Black :> Player.T
    Assert.IsTrue(whitePlayer :? Player.HumanPlayer)
    Assert.IsTrue(blackPlayer :? Player.ComputerPlayer)

    // player settings change
    let model = unwrapResultExn <| GameModel.tryCreate
    let controller = GameController.create model
    let playerSettings = controller.changePlayerSettingsFromPlayers whitePlayer  blackPlayer
    Assert.AreEqual(playerSettings, model.PlayerSettings)
    Assert.AreEqual(whitePlayer.Name, playerSettings.WhitePlayer.Name)
    Assert.AreEqual(whitePlayer.Level, playerSettings.WhitePlayer.Level)
    Assert.AreEqual(blackPlayer.Name, playerSettings.BlackPlayer.Name)
    Assert.AreEqual(blackPlayer.Level, playerSettings.BlackPlayer.Level)
    Assert.IsTrue(playerSettings.WhitePlayer :? Player.HumanPlayer)
    Assert.IsTrue(playerSettings.BlackPlayer :? Player.ComputerPlayer)

    
    ()
 