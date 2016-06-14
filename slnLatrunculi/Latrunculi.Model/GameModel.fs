namespace Latrunculi.Model
open System

[<StructuralEquality;NoComparison>]
type GamePausedStatus =
    | PausedByUser

[<StructuralEquality;NoComparison>]
type GameStatus =
    | Created
    | Running
    | Paused of GamePausedStatus
    | Finished

type ModelChangeEventHandler = delegate of obj * EventArgs -> unit

type GameModel() = 
    let board = match Board.tryInit Board.create (fun _ -> Square.createEmpty) with
                | Error e -> failwith (sprintf "Desku se nepodařilo zinicializovat: %A" e)
                | Success s -> s
       
    let playerSettings = PlayerSettings.createDefault

    let status = Created

    let statusChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
    let playerSettingsChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
    let activePlayerChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
    let boardChangedEvent = new Event<ModelChangeEventHandler, EventArgs>()
      
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

    member private this.OnStatusChanged() =
        statusChangedEvent.Trigger(this, EventArgs.Empty)
    member private this.OnPlayerSettingsChanged() =
        playerSettingsChangedEvent.Trigger(this, EventArgs.Empty)
    member private this.OnActivePlayerChanged() =
        activePlayerChangedEvent.Trigger(this, EventArgs.Empty)
    member private this.OnBoardChanged() =
        boardChangedEvent.Trigger(this, EventArgs.Empty)

    member this.setStatus x =
        this.Status <- x
        this.OnStatusChanged()
        this.Status

    member this.changePlayerSettings playerSettings =
        this.PlayerSettings <- playerSettings
        this.OnPlayerSettingsChanged()
        this.PlayerSettings

    member this.isWhitePlayerActive =
        match this.ActiveColor with
        | Some p -> if p = Piece.Colors.White then true else false
        | _ -> false

    member this.isBlackPlayerActive =
        match this.ActiveColor with
        | Some p -> if p = Piece.Colors.Black then true else false
        | _ -> false

    member this.setActiveColor (x: Piece.Colors option) =
        this.ActiveColor <- x
        this.OnActivePlayerChanged()
        this.ActiveColor

    member this.initBoard() =
        match Board.tryInit this.Board Rules.getInitialBoardSquares with
                | Error e -> failwith (sprintf "Desku se nepodařilo zinicializovat: %A" e)
                | Success s -> 
                    this.OnBoardChanged()
                    s

