﻿namespace Latrunculi
open Latrunculi.Controller
open Latrunculi.Model

module ErrorMessages =
   
    let toString error =
        match error with
        // Coord
        | InvalidColumnNumber -> error.ToString()
        | InvalidRowNumber -> error.ToString()
        | ColumnOutOfRange -> error.ToString()
        | RowOutOfRange -> error.ToString()
        | UnableToParseCoordFromString -> error.ToString()    
        // Move
        | InvalidSourceCoord -> error.ToString()
        | InvalidTargetCoord -> error.ToString()
        | SourceAndTargetCoordMayNotBeSame -> error.ToString()
        // Board
        | UnableToInitializeBoard -> "Nepodařilo se zinicializovat herní desku."
        // Rules
        | RelativeCoordIsOutOfRange -> error.ToString()
        | UnableToGetTargetSquare -> error.ToString()
        | UnableToCreateMove -> error.ToString()
        | TargetSquareIsNotEmpty -> error.ToString()
        | MoveIsNotValid -> "Tah není platný."
        | UnableToRemovePiece -> error.ToString()
        // Brain
        | NoValidMoveExists -> "Žádný tah není možný."
        // Player
        | UnableToDeterminePlayerMove -> "Nepodařilo se zjistit tah hráče."
        | NoBoardInstanceSpecified -> "Nebyla předána platná instance hrací desky."
        | NoHistoryInstanceSpecified -> "Nebyla předána platná instance historie."
        | NoGetMoveFromUICallbackSpecified -> "Nebyl předán odkaz na funkci UI pro získání tahu."
        // PlayerSettings
        | TwoPlayersMayNotBeSameColor -> error.ToString()
        // GameModel
        | NoPlayerOnMove -> "Není známo, který hráč je na tahu."
        | NoActiveColor -> "Není známa barva hráče na tahu."
        | UnableToCreateInitialPlayerSettings -> "Nepodařilo se zinicializovat nastavení hráčů."
        // GameController
        | UnableToSwapActiveColor -> "Změna aktivního hráče selhala."
        | CancellationTokenDoesNotExist -> "Požadavek na zrušení akce neexistuje."
        | GameIsAlreadyRunning -> "Hra je již spuštěna."
        | GameIsNotRunning -> "Hra nebyla spuštěna."
        | UnableToGetPlayerMove -> "Nepodařilo se získat tah hráče."
        | UnableToGetActivePlayer -> "Nepodařilo se zjistit hráče na tahu."
        | MoveSuggestionAlreadyComputing -> "Výpočet nejlepšího tahu již probíhá."
        | HumanSelectedMoveRequestDoesNotExists -> "Neexistuje požadavek na výběr tahu lidského hráče."
        | RequestedHistoryMoveNotFound -> "Požadovaný tah nebyl v historii desky nalezen."
