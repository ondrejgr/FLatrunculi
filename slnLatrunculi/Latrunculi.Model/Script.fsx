#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "RemovedPiece.fs"
#load "Move.fs"
#load "Board.fs"
#load "GameModel.fs"
#load "GameController.fs"

open Latrunculi.Model
open Latrunculi.Controller


let board = Board.create
let coord = getObjExn (Coord.tryCreate 'B' 1)
let square = Square.createWithPiece Piece.createWhite

board.ChangeSquare coord square