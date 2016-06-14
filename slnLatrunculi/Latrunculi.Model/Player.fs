namespace Latrunculi.Model

module Player =
    type Levels =
        | Easy = 0
        | Medium = 1
        | Hard = 2

    [<AbstractClass>]
    type T(name: string, level: Levels) =
        member val Name = name with get, set
        member val Level = level with get, set

    type HumanPlayer(name, level) =
        inherit T(name, level)

    type ComputerPlayer(name, level) =
        inherit T(name, level)


    let isComputer (player: T) =
        player :? ComputerPlayer

    let isHuman (player: T) =
        player :? HumanPlayer

    let createHumanPlayer name level =
        HumanPlayer(name, level)

    let createComputerPlayer name level =
        ComputerPlayer(name, level)
