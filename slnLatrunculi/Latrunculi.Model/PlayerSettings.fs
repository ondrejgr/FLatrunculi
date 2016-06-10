namespace Latrunculi.Model

module PlayerSettings =

    [<StructuralEquality;NoComparison>]
    type T = {
        WhitePlayer: Player;
        BlackPlayer: Player }

    let createHumanPlayer name level =
        HumanPlayer(name, level)

    let createComputerPlayer name level =
        ComputerPlayer(name, level)

    let create x y =
        { WhitePlayer = x; BlackPlayer = y}

    let createDefault =
        let whitePlayer = createHumanPlayer "Bílý" Levels.Medium
        let blackPlayer = createHumanPlayer "Černý" Levels.Medium
        create whitePlayer blackPlayer
