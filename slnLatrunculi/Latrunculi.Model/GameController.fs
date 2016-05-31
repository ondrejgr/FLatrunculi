namespace Latrunculi.Controller
open Latrunculi.Model

type GameController(gameModel: GameModel) = 
    member private this.Model = gameModel
    