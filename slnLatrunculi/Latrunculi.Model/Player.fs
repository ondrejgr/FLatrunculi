namespace Latrunculi.Model
open System
open System.Xml.Serialization

module Player =

    [<XmlType("Types")>]
    type Types =
        | Human
        | Computer

    [<XmlType("Levels")>]
    type Levels =
        | Easy
        | Medium
        | Hard

    type Name = string

    type PlayerTypes = Types * Types
    type PlayerNames = Name * Name
    type PlayerLevels = Levels * Levels

    let mutable Board: Board.T option = None
    let mutable History: History.T option = None
    let mutable getHumanPlayerMoveFromUIWorkflow: (unit -> Async<Move.T>) option = None

    let levelToDepth (x: Levels) =
        match x with
        | Levels.Easy -> Depth.create 1
        | Levels.Medium -> Depth.create 2
        | Levels.Hard -> Depth.create 3

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
                    return! Brain.tryGetBestMove board this.Color <| levelToDepth this.Level
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
    