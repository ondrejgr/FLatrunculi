[<AutoOpen>]
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
    | UnableToInitializeBoard
    // Rules
    | RelativeCoordIsOutOfRange
    | UnableToGetTargetSquare
    | UnableToCreateMove
    | TargetSquareIsNotEmpty
    | MoveIsNotValid
    | UnableToRemovePiece
    // Brain
    | NoValidMoveExists
    | BrainException of string
    // Player
    | UnableToDeterminePlayerMove
    | NoBoardInstanceSpecified
    | NoHistoryInstanceSpecified
    | NoGetMoveFromUICallbackSpecified
    // PlayerSettings
    | TwoPlayersMayNotBeSameColor
    // GameModel
    | NoPlayerOnMove
    | NoActiveColor
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
    | RequestedHistoryMoveNotFound
    | UnableToSaveGame of string
    | UnableToLoadGame of string
    | UnableToDeserializeObject of string
