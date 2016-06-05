namespace Latrunculi.Model

module Rules =

    let GetInitialBoardSquares (coord: Coord.T) =
        match coord with
        | { Row = Coord.RowNumber 7 } | { Row = Coord.RowNumber 6 } -> Square.createWithPiece <| Piece.createBlack
        | { Row = Coord.RowNumber 2 } | { Row = Coord.RowNumber 1 } -> Square.createWithPiece <| Piece.createWhite
        | _ -> Square.createEmpty
