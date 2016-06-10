namespace Latrunculi.Model

type Levels =
    | Easy = 0
    | Medium = 1
    | Hard = 2

type Player(name: string, level: Levels) =
    member val Name = name with get, set
    member val Level = level with get, set


type HumanPlayer(name, level) =
    inherit Player(name, level)

type ComputerPlayer(name, level) =
    inherit Player(name, level)
