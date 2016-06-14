namespace Latrunculi.Model
open System

[<StructuralEquality;NoComparison>]
type GameStatus =
    | Created
    | Running
    | Paused
    | Finished

type GameErrorEventArgs(error) =
    inherit EventArgs()
    member val Error = error with get

type ModelChangeEventHandler = delegate of obj * EventArgs -> unit
type GameErrorEventHandler = delegate of obj * GameErrorEventArgs -> unit

module GameModel =
    type Error = 
        | NoPlayerOnMove
        | UnableToInitializeBoard
        | UnableToCreateInitialPlayerSettings

    type T(board: Board.T, playerSettings: PlayerSettings.T) = 
        let status = Created

        let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let playerSettingsChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let gameErrorEvent = new Event<GameErrorEventHandler, GameErrorEventArgs>()
      
        member val Board = board
        member val PlayerSettings = playerSettings with get, set
        member val Status = status with get, set
        member val ActiveColor = None with get, set
        
        [<CLIEvent>]
        member this.StatusChanged = statusChangedEvent.Publish
        [<CLIEvent>]
        member this.PlayerSettingsChanged = playerSettingsChangedEvent.Publish
        [<CLIEvent>]
        member this.ActivePlayerChanged = activePlayerChangedEvent.Publish
        [<CLIEvent>]
        member this.BoardChanged = boardChangedEvent.Publish
        [<CLIEvent>]
        member this.GameError = gameErrorEvent.Publish

        member private this.OnStatusChanged() =
            statusChangedEvent.Trigger(this, EventArgs.Empty)
        member private this.OnPlayerSettingsChanged() =
            playerSettingsChangedEvent.Trigger(this, EventArgs.Empty)
        member private this.OnActivePlayerChanged() =
            activePlayerChangedEvent.Trigger(this, EventArgs.Empty)
        member private this.OnBoardChanged() =
            boardChangedEvent.Trigger(this, EventArgs.Empty)
        member this.ReportGameError(error) =
            ignore <| this.setStatus(Paused)
            gameErrorEvent.Trigger(this, GameErrorEventArgs(error))

        member this.setStatus x =
            this.Status <- x
            this.OnStatusChanged()
            this.Status

        member this.changePlayerSettings playerSettings =
            this.PlayerSettings <- playerSettings
            this.OnPlayerSettingsChanged()
            this.PlayerSettings

        member this.isWhitePlayerActive =
            match this.getActivePlayer with
            | Success p -> p = this.PlayerSettings.WhitePlayer
            | _ -> false

        member this.isBlackPlayerActive =
            match this.getActivePlayer with
            | Success p -> p = this.PlayerSettings.BlackPlayer
            | _ -> false

        member this.getActivePlayer =
            match this.ActiveColor with
            | Some p when p = Piece.Colors.Black -> Success this.PlayerSettings.BlackPlayer
            | Some p when p = Piece.Colors.White -> Success this.PlayerSettings.WhitePlayer
            | _ -> Error NoPlayerOnMove

        member this.setActiveColor (x: Piece.Colors option) =
            this.ActiveColor <- x
            this.OnActivePlayerChanged()
            this.ActiveColor

        member this.trySwapActiveColor =
            match this.ActiveColor with
            | Some color ->
                    Success (this.setActiveColor <| if color = Piece.Colors.Black then 
                                                        Some Piece.Colors.White 
                                                    else Some Piece.Colors.Black)
            | None -> Error NoPlayerOnMove

        member this.tryInitBoard() =
            match Board.tryInit this.Board Rules.getInitialBoardSquares with
                    | Error e -> Error UnableToInitializeBoard
                    | Success s -> 
                        this.OnBoardChanged()
                        Success s

    let tryCreate =
        maybe {
            let! board = tryChangeError UnableToInitializeBoard <| Board.tryInit 
                                                                        Board.create 
                                                                        (fun _ -> Square.createEmpty)
            let! playerSettings = tryChangeError UnableToCreateInitialPlayerSettings <| PlayerSettings.tryCreateDefault
            return T(board, playerSettings) }
