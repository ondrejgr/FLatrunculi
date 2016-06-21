namespace Latrunculi.Model
open System

[<StructuralEquality;NoComparison>]
type GameStatus =
    | Created
    | Running
    | Paused
    | WaitingForHumanPlayerMove
    | Finished

type MoveEventArgs(move: Result<Move.T, Error>) =
    inherit EventArgs()
    member val Move = move

type HistoryItemAddedEventArgs(item: HistoryItem.T) =
    inherit EventArgs()
    member val Item = item

type GameErrorEventArgs(error: ErrorDefinitions.Error) =
    inherit EventArgs()
    member val Error = error with get

type ModelChangeEventHandler = delegate of obj * EventArgs -> unit
type MoveSuggestionComputedEventHandler = delegate of obj * MoveEventArgs -> unit
type HistoryItemAddedEventHandler = delegate of obj * HistoryItemAddedEventArgs -> unit
type GameErrorEventHandler = delegate of obj * GameErrorEventArgs -> unit

module GameModel =

    type T(board: Board.T, historyBoard: Board.T, playerSettings: PlayerSettings.T) = 
        let status = Created

        let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let playerSettingsChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let isMoveSuggestionComputingChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let moveSuggestionComputedEvent = new Event<MoveSuggestionComputedEventHandler, MoveEventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let historyItemAddedEvent = new Event<HistoryItemAddedEventHandler, HistoryItemAddedEventArgs>()
        let historyClearedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let gameErrorEvent = new Event<GameErrorEventHandler, GameErrorEventArgs>()
              
        member val Board = board
        member val HistoryBoard = historyBoard
        member val PlayerSettings = playerSettings with get, set
        member val Status = status with get, set
        member val ActiveColor = None with get, set
        member val IsMoveSuggestionComputing = false with get, set
        member val Result = Rules.NoResult with get, set
        
        [<CLIEvent>]
        member this.StatusChanged = statusChangedEvent.Publish
        [<CLIEvent>]
        member this.PlayerSettingsChanged = playerSettingsChangedEvent.Publish
        [<CLIEvent>]
        member this.ActivePlayerChanged = activePlayerChangedEvent.Publish
        [<CLIEvent>]
        member this.IsMoveSuggestionComputingChanged = isMoveSuggestionComputingChangedEvent.Publish
        [<CLIEvent>]
        member this.MoveSuggestionComputed = moveSuggestionComputedEvent.Publish
        [<CLIEvent>]
        member this.BoardChanged = boardChangedEvent.Publish
        [<CLIEvent>]
        member this.HistoryItemAdded = historyItemAddedEvent.Publish
        [<CLIEvent>]
        member this.HistoryCleared = historyClearedEvent.Publish
        [<CLIEvent>]
        member this.GameError = gameErrorEvent.Publish

        member private this.OnStatusChanged() =
            statusChangedEvent.Trigger(this, EventArgs.Empty)

        member private this.OnPlayerSettingsChanged() =
            playerSettingsChangedEvent.Trigger(this, EventArgs.Empty)

        member private this.OnActivePlayerChanged() =
            activePlayerChangedEvent.Trigger(this, EventArgs.Empty)

        member private this.OnIsMoveSuggestionComputingChanged() =
            isMoveSuggestionComputingChangedEvent.Trigger(this, EventArgs.Empty)

        member this.RaiseMoveSuggestionComputed(move) =
            moveSuggestionComputedEvent.Trigger(this, MoveEventArgs(move))

        member this.RaiseBoardChanged() =
            boardChangedEvent.Trigger(this, EventArgs.Empty)

        member this.RaiseHistoryItemAdded(x) =
            historyItemAddedEvent.Trigger(this, HistoryItemAddedEventArgs(x))

        member this.RaiseHistoryCleared() =
            historyClearedEvent.Trigger(this, EventArgs.Empty)

        member this.RaiseGameErrorEvent(error) =
            gameErrorEvent.Trigger(this, GameErrorEventArgs(error))

        member this.setStatus x =
            this.Status <- x
            this.OnStatusChanged()
            this.Status

        member this.setResult x =
            this.Result <- x
            this.Result

        member this.setIsMoveSuggestionComputing x =
            this.IsMoveSuggestionComputing <- x
            this.OnIsMoveSuggestionComputingChanged()
            this.IsMoveSuggestionComputing            

        member this.changePlayerSettings playerSettings =
            this.PlayerSettings <- playerSettings
            this.OnPlayerSettingsChanged()
            this.PlayerSettings

        member this.isWhitePlayerActive =
            match this.tryGetActivePlayer() with
            | Success p -> p = this.PlayerSettings.WhitePlayer
            | _ -> false

        member this.isBlackPlayerActive =
            match this.tryGetActivePlayer() with
            | Success p -> p = this.PlayerSettings.BlackPlayer
            | _ -> false

        member this.tryGetActivePlayer() =
            match this.ActiveColor with
            | Some p when p = Piece.Colors.Black -> Success this.PlayerSettings.BlackPlayer
            | Some p when p = Piece.Colors.White -> Success this.PlayerSettings.WhitePlayer
            | _ -> Error NoPlayerOnMove

        member this.tryGetActiveColor() = 
            match this.ActiveColor with
            | Some p -> Success p
            | _ -> Error NoActiveColor

        member this.setActiveColor (x: Piece.Colors option) =
            this.ActiveColor <- x
            this.OnActivePlayerChanged()
            this.ActiveColor

        member this.trySwapActiveColor() =
            match this.ActiveColor with
            | Some color ->
                ignore <| this.setActiveColor (match color with
                                                | Piece.Colors.Black -> Some Piece.Colors.White 
                                                | Piece.Colors.White -> Some Piece.Colors.Black
                                                | _ -> None)
                Success ()
            | None -> Error NoPlayerOnMove

        member this.getNumberOfMovesWithoutRemoval() =
            History.getNumberOfMovesWithoutRemoval this.Board.History

    let tryCreate() =
        maybe {
            let! board = Board.create() |> Board.tryInit <| Rules.getEmptyBoardSquares
            let! historyBoard = Board.create() |> Board.tryInit <| Rules.getEmptyBoardSquares
            let! playerSettings = PlayerSettings.tryCreateDefault()
            return T(board, historyBoard, playerSettings) }
