module RulesTest

open Latrunculi.Model
open Latrunculi.Controller
open NUnit.Framework
open NUnit.Framework.Constraints

[<Test>]
let RulesTest() =
    let model = GameModel()
    let controller = GameController(model)
    let playerSettings = PlayerSettings.createDefault
    controller.NewGame(playerSettings.WhitePlayer, playerSettings.BlackPlayer)

    let empty = Square.createEmpty
    let white = Square.createWithPiece <| Piece.createWhite
    let black = Square.createWithPiece <| Piece.createBlack
    
//    let createMove Coord.tryCreateFromString src
//    Coord.tryCreate 'A' 1 |> Move.tryCreate <| Coord.tryCreate 'A' 2

    let move = Move.tryCreateFromStringCoords "A1" "A2" empty white

    ()