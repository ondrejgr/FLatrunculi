namespace Latrunculi
open Latrunculi.Controller
open Latrunculi.Model

module ErrorMessages =

    let msgGameModel error =
        match error with
        | GameModel.NoPlayerOnMove -> "Žádný hráč není na tahu."
        | GameModel.UnableToInitializeBoard -> "Nepodařilo se zinicializovat herní desku."
        | GameModel.UnableToCreateInitialPlayerSettings -> "Nepodařilo se zinicializovat nastavení hráčů."

    let msgGameController error =
        match error with
        | GameController.UnableToSwapActiveColor -> "Změne aktivního hráče selhala."
        | GameController.UnableToInitializeBoard -> "Nepodařilo se zinicializovat herní desku."
        | GameController.CancellationTokenDoesNotExist -> "Požadavek na zrušení akce neexistuje."
        | GameController.GameIsAlreadyRunning -> "Hra je již spuštěna."
        | GameController.GameIsNotRunning -> "Hra nebyla spuštěna."

    let toString (result: obj) =
        match result with
        | :? Result<GameModel.T,GameModel.Error> as r -> 
                match r with 
                | Success _ -> ""
                | Error e -> msgGameModel e
        | :? Result<GameController.T,GameController.Error> as r -> 
                match r with 
                | Success _ -> ""
                | Error e -> msgGameController e
        | _ -> result.ToString()
