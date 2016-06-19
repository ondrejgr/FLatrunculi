namespace Latrunculi.Model
open System

module Player =

    type Types =
        | Human = 0
        | Computer = 1

    type Levels =
        | Easy = 0
        | Medium = 1
        | Hard = 2

    type Name = string

    type PlayerTypes = Types * Types
    type PlayerNames = Name * Name
    type PlayerLevels = Levels * Levels

    let mutable Board: Board.T option = None
    let mutable getHumanPlayerMoveFromUIWorkflow: (unit -> Async<Move.T>) option = None

    [<AbstractClass>]
    type T(name: Name, level: Levels, color: Piece.Colors) =
        member val Name = name with get, set
        member val Level = level with get, set
        member val Color = color with get

        abstract member TryGetMove: unit -> Async<Result<Move.T, Error>>

    type HumanPlayer(name, level, color) =
        inherit T(name, level, color)

        override this.TryGetMove() =
            async {
                match getHumanPlayerMoveFromUIWorkflow with
                | Some getHumanPlayerMoveFromUI ->
                    let! move = getHumanPlayerMoveFromUI()
                    return Success move
                | None ->
                    return Error NoGetMoveFromUICallbackSpecified }

    type ComputerPlayer(name, level, color) =
        inherit T(name, level, color)

        override this.TryGetMove() =
            let tryGetBoard =
                match Board with
                | Some b -> Success b
                | None -> Error NoBoardInstanceSpecified
                
            async {
                match tryGetBoard with
                | Success board ->
                    return! Brain.tryGetBestMove board this.Color 
                | Error e ->
                    return Error e }
                
    let tryGetMove (player: T) =
        player.TryGetMove()

    let getPlayerType (player: T) =
        match player with
        | :? HumanPlayer -> Types.Human
        | :? ComputerPlayer -> Types.Computer
        | _ -> failwith "Invalid Player Type"
   
    let createHumanPlayer name level color =
        HumanPlayer(name, level, color)

    let createComputerPlayer name level color =
        ComputerPlayer(name, level, color)
        
    let create (ptype: Types) (name: Name) (level: Levels) (color: Piece.Colors) =
        match ptype with
        | Types.Human -> createHumanPlayer name level color :> T   
        | Types.Computer -> createComputerPlayer name level color :> T
        | _ -> failwith "Invalid Player Type"
    