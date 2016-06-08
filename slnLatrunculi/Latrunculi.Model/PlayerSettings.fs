namespace Latrunculi.Model

module PlayerSettings =

    type Levels =
        | Easy = 0
        | Medium = 1
        | Hard = 2

    [<StructuralEquality;NoComparison>]
    type PlayerInfo = {
        Name: string;
        Level: Levels }

    [<StructuralEquality;NoComparison>]
    type Player =
        | HumanPlayer of PlayerInfo
        | ComputerPlayer of PlayerInfo

    [<StructuralEquality;NoComparison>]
    type T = {
        WhitePlayer: Player;
        BlackPlayer: Player }

    let create x y =
        { WhitePlayer = x; BlackPlayer = y}

    let createDefault =
        let whitePlayer = HumanPlayer { Name = "Bílý"; Level = Levels.Medium }
        let blackPlayer = HumanPlayer { Name = "Černý"; Level = Levels.Medium }
        create whitePlayer blackPlayer
