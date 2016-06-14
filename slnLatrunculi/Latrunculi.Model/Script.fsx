#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "Move.fs"
#load "RemovedPiece.fs"
#load "BoardMove.fs"
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
let board = model.Board
let controller = GameController(model)
let playerSettings = PlayerSettings.createDefault
controller.NewGame(playerSettings.WhitePlayer, playerSettings.BlackPlayer)

let empty = Square.createEmpty
let white = Square.createWithPiece <| Piece.createWhite
let black = Square.createWithPiece <| Piece.createBlack

// Coord.iter (fun c -> printfn "%A: %A " c <| Board.getSquare board c)