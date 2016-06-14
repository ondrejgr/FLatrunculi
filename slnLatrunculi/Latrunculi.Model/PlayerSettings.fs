namespace Latrunculi.Model

module PlayerSettings =

    [<StructuralEquality;NoComparison>]
    type T = {
        WhitePlayer: Player.T;
        BlackPlayer: Player.T }

    let create x y =
        { WhitePlayer = x; BlackPlayer = y}

    let createDefault =
        let whitePlayer = Player.createHumanPlayer "Bílý" Player.Levels.Medium
        let blackPlayer = Player.createHumanPlayer "Černý" Player.Levels.Medium
        create whitePlayer blackPlayer
