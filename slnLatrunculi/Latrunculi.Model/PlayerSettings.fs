namespace Latrunculi.Model

module PlayerSettings =

    type Error =
        | TwoPlayersWithSameColor

    [<StructuralEquality;NoComparison>]
    type T = {
        WhitePlayer: Player.T;
        BlackPlayer: Player.T }

    let tryCreate (x: Player.T) (y: Player.T) =
        if (x.Color = y.Color) then Error TwoPlayersWithSameColor else Success { WhitePlayer = x; BlackPlayer = y}

    let createDefault =
        let whitePlayer = Player.createHumanPlayer "Bílý" Player.Levels.Medium Piece.Colors.White
        let blackPlayer = Player.createHumanPlayer "Černý" Player.Levels.Medium Piece.Colors.Black
        unwrapResultExn <| tryCreate whitePlayer blackPlayer
