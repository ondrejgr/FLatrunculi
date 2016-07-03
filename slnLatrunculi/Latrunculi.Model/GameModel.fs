﻿namespace Latrunculi.Model
open System

[<StructuralEquality;NoComparison>]
type GameStatus =
    | Created
    | Running
    | Paused
    | WaitingForHumanPlayerMove
    | Finished

module MoveRequest =
    [<StructuralEquality;NoComparison>]
    type T =
        | NoRequest
        | UndoRequest of BoardMove.T
        | RedoRequest of BoardMove.T

    let create =
        NoRequest
    let createUndoRequest (move: BoardMove.T) =
        UndoRequest move
    let createRedoRequest (move: BoardMove.T) =
        RedoRequest move

type MoveEventArgs(move: Result<Move.T, Error>) =
    inherit EventArgs()
    member val Move = move

type HistoryItemAddedEventArgs (index: int, item: BoardMove.T) =
    inherit EventArgs()
    member val Index = index
    member val Item = item

type HistoryItemRemovedEventArgs (index: int) =
    inherit EventArgs()
    member val Index = index

type GameErrorEventArgs(error: ErrorDefinitions.Error) =
    inherit EventArgs()
    member val Error = error with get

type ModelChangeEventHandler = delegate of obj * EventArgs -> unit
type MoveSuggestionComputedEventHandler = delegate of obj * MoveEventArgs -> unit
type HistoryItemAddedEventHandler = delegate of obj * HistoryItemAddedEventArgs -> unit
type HistoryItemRemovedEventHandler = delegate of obj * HistoryItemRemovedEventArgs -> unit
type GameErrorEventHandler = delegate of obj * GameErrorEventArgs -> unit

module GameModel =

    type T(board: Board.T, playerSettings: PlayerSettings.T) = 
        let status = Created

        let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let playerSettingsChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let isMoveSuggestionComputingChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let moveSuggestionComputedEvent = new Event<MoveSuggestionComputedEventHandler, MoveEventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let historyItemAddedEvent = new Event<HistoryItemAddedEventHandler, HistoryItemAddedEventArgs>()
        let historyItemRemovedEvent = new Event<HistoryItemRemovedEventHandler, HistoryItemRemovedEventArgs>()
        let historyClearedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let gameErrorEvent = new Event<GameErrorEventHandler, GameErrorEventArgs>()
        let computerPlayerThinkingEvent = new Event<ModelChangeEventHandler, EventArgs>()
              
        member val Board = board
        member val PlayerSettings = playerSettings with get, set
        member val Status = status with get, set
        member val ActiveColor = None with get, set
        member val IsMoveSuggestionComputing = false with get, set
        member val Result = Rules.NoResult with get, set

        member val MoveRequest = MoveRequest.create with get, set
        
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
        member this.HistoryItemRemoved = historyItemRemovedEvent.Publish
        [<CLIEvent>]
        member this.HistoryCleared = historyClearedEvent.Publish
        [<CLIEvent>]
        member this.GameError = gameErrorEvent.Publish
        [<CLIEvent>]
        member this.ComputerPlayerThinking = computerPlayerThinkingEvent.Publish

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

        member private this.RaiseHistoryItemAdded(index, item) =
            historyItemAddedEvent.Trigger(this, HistoryItemAddedEventArgs(index, item))

        member private this.RaiseHistoryItemRemoved(index) =
            historyItemRemovedEvent.Trigger(this, HistoryItemRemovedEventArgs(index))

        member private this.RaiseHistoryCleared() =
            historyClearedEvent.Trigger(this, EventArgs.Empty)

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

        member this.pushMoveToHistory (move: BoardMove.T) =
            let index = this.Board.History.PushMove move
            this.RaiseHistoryItemAdded(index, move)


    let tryCreate() =
        maybe {
            let! board = Board.create() |> Board.tryInit <| Rules.getEmptyBoardSquares
            let! playerSettings = PlayerSettings.tryCreateDefault()
            return T(board, playerSettings) }
