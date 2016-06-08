namespace Latrunculi.Model

[<StructuralEquality;NoComparison>]
type GameStatus =
    | Created

type StatusChangedEventArgs(newStatus: GameStatus) =
    inherit System.EventArgs()
    member this.NewStatus = newStatus
type StatusChangedEventHandler = delegate of obj * StatusChangedEventArgs -> unit

type GameModel() = 
            
    let board = match Board.tryInit Board.create Rules.GetInitialBoardSquares with
                | Error e -> failwith (sprintf "Desku se nepodařilo zinicializovat: %A" e)
                | Success s -> s
       
    let playerSettings = PlayerSettings.createDefault

    let status = Created

    let statusChangedEvent = new Event<StatusChangedEventHandler, StatusChangedEventArgs>()

    member val Board = board
    member val PlayerSettings = playerSettings with get, set
    member val Status = status with get, set

    [<CLIEvent>]
    member this.StatusChanged = statusChangedEvent.Publish

    member private this.OnStatusChanged() =
        statusChangedEvent.Trigger(this, StatusChangedEventArgs(this.Status))

    member this.changePlayerSettings (white, black) =
        this.PlayerSettings <- PlayerSettings.create white black
        this.PlayerSettings