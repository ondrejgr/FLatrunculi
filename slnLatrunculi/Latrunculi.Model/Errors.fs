﻿[<AutoOpen>]
module ErrorDefinitions

type Error =
    // Coord
    | InvalidColumnNumber
    | InvalidRowNumber
    | ColumnOutOfRange
    | RowOutOfRange
    | UnableToParseCoordFromString    
    // Move
    | InvalidSourceCoord
    | InvalidTargetCoord
    | SourceAndTargetCoordMayNotBeSame
    // Board
    | UnableToIterateBoard
    // Rules
    | RelativeCoordIsOutOfRange
    | UnableToGetTargetSquare
    | UnableToCreateMove
    | TargetSquareIsNotEmpty
    | MoveIsNotValid
    | UnableToRemovePiece
    // Brain
    | NoValidMoveExists
    // Player
    | UnableToDeterminePlayerMove
    | NoBoardInstanceSpecified
    | NoGetMoveFromUICallbackSpecified
    // PlayerSettings
    | TwoPlayersMayNotBeSameColor
    // GameModel
    | NoPlayerOnMove
    | NoActiveColor
    | UnableToInitializeBoard
    | UnableToCreateInitialPlayerSettings
    // GameController
    | UnableToSwapActiveColor
    | CancellationTokenDoesNotExist
    | GameIsAlreadyRunning
    | GameIsNotRunning
    | UnableToGetPlayerMove
    | UnableToGetActivePlayer
    | MoveSuggestionAlreadyComputing
    | HumanSelectedMoveRequestDoesNotExists