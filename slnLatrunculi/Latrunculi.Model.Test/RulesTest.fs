module RulesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let RulesTest() =
    let model = GameModel()
    let board = model.Board
    let controller = GameController(model)
    let playerSettings = PlayerSettings.createDefault
    controller.NewGame(playerSettings.WhitePlayer, playerSettings.BlackPlayer)

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

    ()