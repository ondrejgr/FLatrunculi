#load "Errors.fs"
#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "Move.fs"
#load "RemovedPiece.fs"
#load "BoardMove.fs"
#load "HistoryItem.fs"
#load "History.fs"
#load "Board.fs"
#load "Rules.fs"
#load "MoveValue.fs"
#load "MoveTree.fs"
#load "MiniMax.fs"
#load "Brain.fs"
#load "HumanSelectedMove.fs"
#load "Player.fs"
#load "PlayerSettings.fs"
#load "GameModel.fs"
#load "GameController.fs"
#load "ErrorMessages.fs"

open Latrunculi.Model
open Latrunculi.Controller
open System.Threading

let model = unwrapResultExn <| GameModel.tryCreate()
let controller = GameController.create model

unwrapResultExn <| controller.TryNewGame()

unwrapResultExn <| controller.TryRun()

let activeColor = unwrapResultExn <| model.tryGetActiveColor()

