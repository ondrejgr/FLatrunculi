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

type HistoryChangedEventArgs (items: HistoryItem.T seq) =
    inherit EventArgs()
    member val Items = items

type GameErrorEventArgs(error: ErrorDefinitions.Error) =
    inherit EventArgs()
    member val Error = error with get

type ModelChangeEventHandler = delegate of obj * EventArgs -> unit
type MoveSuggestionComputedEventHandler = delegate of obj * MoveEventArgs -> unit
type HistoryChangedEventHandler = delegate of obj * HistoryChangedEventArgs -> unit
type GameErrorEventHandler = delegate of obj * GameErrorEventArgs -> unit

module GameModel =

    type T(board: Board.T, playerSettings: PlayerSettings.T) = 
        let status = Created

        let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let resultChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let playerSettingsChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let isMoveSuggestionComputingChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let moveSuggestionComputedEvent = new Event<MoveSuggestionComputedEventHandler, MoveEventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let historyChangedEvent = new Event<HistoryChangedEventHandler, HistoryChangedEventArgs>()
        let gameErrorEvent = new Event<GameErrorEventHandler, GameErrorEventArgs>()
        let computerPlayerThinkingEvent = new Event<ModelChangeEventHandler, EventArgs>()
              
        member val Board = board
        member val PlayerSettings = playerSettings with get, set
        member val Status = status with get, set
        member val ActiveColor = None with get, set
        member val IsMoveSuggestionComputing = false with get, set
        member val Result = Rules.NoResult with get, set
       
        [<CLIEvent>]
        member this.StatusChanged = statusChangedEvent.Publish
        [<CLIEvent>]
        member this.ResultChanged = resultChangedEvent.Publish
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
        member this.HistoryChanged = historyChangedEvent.Publish
        [<CLIEvent>]
        member this.GameError = gameErrorEvent.Publish
        [<CLIEvent>]
        member this.ComputerPlayerThinking = computerPlayerThinkingEvent.Publish

        member private this.OnStatusChanged() =
            statusChangedEvent.Trigger(this, EventArgs.Empty)

        member private this.OnResultChanged() =
            resultChangedEvent.Trigger(this, EventArgs.Empty)

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

        member this.RaiseHistoryChanged() =
            historyChangedEvent.Trigger(this, HistoryChangedEventArgs(this.Board.History.Items))

        member this.RaiseGameErrorEvent(error) =
            gameErrorEvent.Trigger(this, GameErrorEventArgs(error))

        member this.RaiseComputerPlayerThinking() =
            computerPlayerThinkingEvent.Trigger(this, EventArgs.Empty)

        member this.setStatus x =
            this.Status <- x
            this.OnStatusChanged()
            this.Status

        member this.setResult x =
            this.Result <- x
            this.OnResultChanged()
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

        member this.getActivePlayerName() =
            match this.tryGetActivePlayer() with 
            | Success p -> p.Name
            | _ -> String.Empty

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
                                                | Piece.Colors.White -> Some Piece.Colors.Black)
                Success ()
            | None -> Error NoPlayerOnMove

        member this.NumberOfMovesWithoutRemoval =
            this.Board.History.NumberOfMovesWithoutRemoval

        member this.pushMoveToHistoryAndClearRedoStack (move: BoardMove.T) =
            this.Board.History.ClearRedoStack()       
            this.Board.History.PushMoveToUndoStack move
            this.RaiseHistoryChanged()

    let tryCreate() =
        maybe {
            let! board = Board.create() |> Board.tryInit <| Rules.getEmptyBoardSquares
            let! playerSettings = PlayerSettings.tryCreateDefault()
            return T(board, playerSettings) }
