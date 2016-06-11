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

let col = Coord.ColumnNumber 'B'
Coord.tryGetNextCol col
Coord.tryGetPrevCol col