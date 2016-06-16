namespace Latrunculi.Model

module PlayerSettings =


    [<StructuralEquality;NoComparison>]
    type T = {
        WhitePlayer: Player.T;
        BlackPlayer: Player.T }

    let tryCreate (x: Player.T) (y: Player.T) =
        if (x.Color = y.Color) then Error TwoPlayersMayNotBeSameColor else Success { WhitePlayer = x; BlackPlayer = y}

    let tryCreateDefault =
        let whitePlayer = Player.createHumanPlayer "Bílý" Player.Levels.Medium Piece.Colors.White
        let blackPlayer = Player.createHumanPlayer "Černý" Player.Levels.Medium Piece.Colors.Black
        tryCreate whitePlayer blackPlayer
