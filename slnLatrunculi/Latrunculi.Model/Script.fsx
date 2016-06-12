#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "RemovedPiece.fs"
#load "Move.fs"
#load "Board.fs"
#load "Rules.fs"
#load "Player.fs"
#load "PlayerSettings.fs"
#load "GameModel.fs"
#load "GameController.fs"

open Latrunculi.Model
open Latrunculi.Controller
open System.Threading

let model = GameModel()
let controller = GameController(model)
let playerSettings = PlayerSettings.createDefault
controller.NewGame(playerSettings.WhitePlayer, playerSettings.BlackPlayer)


let test a b =
    let geta =
        maybe {
            return Success 4 }
    let getb =
        maybe {
            return Success 5 }

    let secti a b = 
        maybe {
            return Success (a + b) }
    maybe {
        let! a = geta
        let! b = getb
        let! soucet = secti a b              
        return Success soucet }