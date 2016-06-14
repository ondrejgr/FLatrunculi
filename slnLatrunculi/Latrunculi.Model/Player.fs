namespace Latrunculi.Model

module Player =
    type Error =
        | UnableToDeterminePlayerMove

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

    let mutable SelectedMove: Move.T option = None

    [<AbstractClass>]
    type T(name: Name, level: Levels, color: Piece.Colors) =
        member val Name = name with get, set
        member val Level = level with get, set
        member val Color = color with get

        abstract member TryGetMove: unit -> Result<Move.T, Error>

    type HumanPlayer(name, level, color) =
        inherit T(name, level, color)

        override this.TryGetMove() =
            match SelectedMove with
            | Some m -> Success m
            | None -> Error UnableToDeterminePlayerMove            

    type ComputerPlayer(name, level, color, board) =
        inherit T(name, level, color)

        member val private Board = board with get

        override this.TryGetMove() =
            match Brain.tryGetBestMove this.Board this.Color with
            | Success m -> Success m
            | Error _ -> Error UnableToDeterminePlayerMove     
                
    let tryGetMove (player: T) =
            player.TryGetMove()

    let getPlayerType (player: T) =
        match player with
        | :? HumanPlayer -> Types.Human
        | :? ComputerPlayer -> Types.Computer
        | _ -> failwith "Invalid Player Type"
   
    let createHumanPlayer name level color =
        HumanPlayer(name, level, color)

    let createComputerPlayer name level color board =
        ComputerPlayer(name, level, color, board)
        
    let create (ptype: Types) (name: Name) (level: Levels) (color: Piece.Colors) (board: Board.T) =
        match ptype with
        | Types.Human -> createHumanPlayer name level color :> T   
        | Types.Computer -> createComputerPlayer name level color board :> T
        | _ -> failwith "Invalid Player Type"
    