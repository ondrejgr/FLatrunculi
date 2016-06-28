namespace Latrunculi.Model
open System

module ReplayModel =

    type T(board: Board.T, playerSettings: PlayerSettings.T) = 

        let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
        let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()

        member val Board = board
        member val ActiveColor = None with get, set

        member val PlayerSettings = playerSettings

        [<CLIEvent>]
        member this.BoardChanged = boardChangedEvent.Publish
        [<CLIEvent>]
        member this.ActivePlayerChanged = activePlayerChangedEvent.Publish

        member private this.OnActivePlayerChanged() =
            activePlayerChangedEvent.Trigger(this, EventArgs.Empty)
        member this.RaiseBoardChanged() =
            boardChangedEvent.Trigger(this, EventArgs.Empty)

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


    let tryCreate(board: Board.T, playerSettings: PlayerSettings.T): Result<T, Error> =
        maybe {
            let! replayBoard = Board.tryClone board
            return T(replayBoard, playerSettings) }

