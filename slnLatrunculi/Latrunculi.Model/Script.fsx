#load "Enumerations.fs"
#load "Sequences.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Board.fs"
#load "GameModel.fs"
#load "GameController.fs"

open Latrunculi.Model
open Latrunculi.Controller


let whitePiece = Piece.Create PieceColors.White
let blackPiece = Piece.Create PieceColors.Black

let emptySquare = Square.CreateEmpty
let squareWithWhitePiece = Square.CreateWithPiece whitePiece
let squareWithBlackPiece = Square.CreateWithPiece blackPiece


