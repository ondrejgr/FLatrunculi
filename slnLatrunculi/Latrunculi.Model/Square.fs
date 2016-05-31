namespace Latrunculi.Model

module Square =

    type T =
        | Piece of Piece.T
        | Nothing

    let CreateWithPiece piece =
        Piece piece

    let CreateEmpty =
        Nothing