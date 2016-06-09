#load "Common.fs"
#load "Piece.fs"
#load "Square.fs"
#load "Coord.fs"
#load "RemovedPiece.fs"
#load "Move.fs"
#load "Board.fs"
#load "Rules.fs"
#load "PlayerSettings.fs"
#load "GameModel.fs"
#load "GameController.fs"

open Latrunculi.Model
open Latrunculi.Controller
open System.Threading

let model = GameModel()
let controller = GameController(model)

let cts = new CancellationTokenSource()

controller.Run(cts.Token)

//match result |>  Async.RunSynchronously with
//| Choice1Of2 _ -> printfn "Success"; ()
//| Choice2Of2 exn -> 
//      printfn "Failed: %s" exn.Message

//match result |>  Async.RunSynchronously with
//| Choice1Of2 _ -> printfn "Success"; ()
//| Choice2Of2 exn -> 
//      printfn "Failed: %s" exn.Message
  
    
      
//printfn "Sleeping"
//Async.Sleep(2000)
//printfn "Sleeped"
