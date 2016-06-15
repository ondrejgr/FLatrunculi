namespace Latrunculi
open Latrunculi.Controller
open Latrunculi.Model

module ErrorMessages =
   
    let toString (result: obj) =
        match result with
        | :? GameModel.Error as error -> 
                    match error with
                    | GameModel.NoPlayerOnMove -> "Žádný hráč není na tahu."
                    | GameModel.UnableToInitializeBoard -> "Nepodařilo se zinicializovat herní desku."
                    | GameModel.UnableToCreateInitialPlayerSettings -> "Nepodařilo se zinicializovat nastavení hráčů."
        | :? GameController.Error as error -> 
                    match error with
                    | GameController.UnableToSwapActiveColor -> "Změna aktivního hráče selhala."
                    | GameController.UnableToInitializeBoard -> "Nepodařilo se zinicializovat herní desku."
                    | GameController.CancellationTokenDoesNotExist -> "Požadavek na zrušení akce neexistuje."
                    | GameController.GameIsAlreadyRunning -> "Hra je již spuštěna."
                    | GameController.GameIsNotRunning -> "Hra nebyla spuštěna."
                    | GameController.UnableToGetPlayerMove -> "Nepodařilo se získat tah hráče."
                    | GameController.UnableToGetActivePlayer -> "Nepodařilo se zjistit hráče na tahu."
        | _ -> result.ToString()
