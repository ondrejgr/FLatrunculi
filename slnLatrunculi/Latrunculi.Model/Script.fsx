#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "Move.fs"
#load "RemovedPiece.fs"
#load "BoardMove.fs"
#load "Board.fs"
#load "Rules.fs"
#load "Brain.fs"
#load "Player.fs"
#load "PlayerSettings.fs"
#load "GameModel.fs"
#load "GameController.fs"

open Latrunculi.Model
open Latrunculi.Controller
open System.Threading

let model = GameModel()
let board = model.Board
let controller = GameController(model)
controller.NewGame()
