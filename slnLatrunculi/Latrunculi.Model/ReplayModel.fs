namespace Latrunculi.Model
open System

[<StructuralEquality;NoComparison>]
type ReplayStatus =
    | Created
    | Paused
    | Running
    | Finished

type PositionChangedEventArgs(id: int) =
    inherit EventArgs()
    member val ID = id

type PositionChangedEventHandler = delegate of obj * PositionChangedEventArgs -> unit


module ReplayModel =

    type T(board: Board.T, playerSettings: PlayerSettings.T) = 

        let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let gameErrorEvent = new Event<GameErrorEventHandler, GameErrorEventArgs>()
        let positionChangedEvent = new Event<PositionChangedEventHandler, PositionChangedEventArgs>()

        member val Board = board
        member val ActiveColor = None with get, set

        member val PlayerSettings = playerSettings

        member val Status = Created with get, set
        member val Result = Rules.NoResult with get, set

        member val Interval = 1000.0 with get, set

        [<CLIEvent>]
        member this.StatusChanged = statusChangedEvent.Publish
        [<CLIEvent>]
        member this.BoardChanged = boardChangedEvent.Publish
        [<CLIEvent>]
        member this.ActivePlayerChanged = activePlayerChangedEvent.Publish
        [<CLIEvent>]
        member this.GameError = gameErrorEvent.Publish
        [<CLIEvent>]
        member this.PositionChanged = positionChangedEvent.Publish

        member private this.OnStatusChanged() =
            statusChangedEvent.Trigger(this, EventArgs.Empty)
    
        member private this.OnActivePlayerChanged() =
            activePlayerChangedEvent.Trigger(this, EventArgs.Empty)

        member this.RaiseBoardChanged() =
            boardChangedEvent.Trigger(this, EventArgs.Empty)

        member this.RaiseGameErrorEvent(error) =
            gameErrorEvent.Trigger(this, GameErrorEventArgs(error))

        member this.RaisePositionChangedEvent(id) =
            positionChangedEvent.Trigger(this, PositionChangedEventArgs(id))

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
            
        member this.setResult x =
            this.Result <- x
            this.Result

        member this.setInverval x =
            this.Interval <- x
            this.Interval

        member this.setStatus x =
            this.Status <- x
            this.OnStatusChanged()
            this.Status


        member this.getNumberOfMovesInHistory() =
            History.getNumberOfMoves this.Board.History

    let tryCreate(board: Board.T, playerSettings: PlayerSettings.T): Result<T, Error> =
        maybe {
            let! replayBoard = Board.tryClone board
            return T(replayBoard, playerSettings) }

