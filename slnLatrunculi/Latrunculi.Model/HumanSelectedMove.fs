namespace Latrunculi.Model
open System

type HumanMoveSelectedEventArgs(move: Result<Move.T, Error>) =
    inherit EventArgs()
    member val Move = move with get

type HumanMoveSelectedEventHandler = delegate of obj * HumanMoveSelectedEventArgs -> unit

module HumanSelectedMove =

    type T() =
        let humanMoveSelectedEvent = new Event<HumanMoveSelectedEventHandler, HumanMoveSelectedEventArgs>()

        [<CLIEvent>]
        member this.HumanMoveSelected = humanMoveSelectedEvent.Publish
        
        member private this.OnHumanMoveSelected(move) =
            humanMoveSelectedEvent.Trigger(this, HumanMoveSelectedEventArgs(move))

    let create =
        T()