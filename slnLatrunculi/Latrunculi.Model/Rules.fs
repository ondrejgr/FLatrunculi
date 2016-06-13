namespace Latrunculi.Model


module Rules =
    type Error =
        | RelativeCoordIsOutOfRange
        | UnableToGetTargetSquare
        | UnableToCreateMove
        | TargetSquareIsNotEmpty

    let getInitialBoardSquares (coord: Coord.T) =
        match coord with
        | { Row = Coord.RowNumber 7 } | { Row = Coord.RowNumber 6 } -> Square.createWithPiece <| Piece.createBlack
        | { Row = Coord.RowNumber 2 } | { Row = Coord.RowNumber 1 } -> Square.createWithPiece <| Piece.createWhite
        | _ -> Square.createEmpty

    let getInitialActiveColor =
        Piece.Colors.White

    let getValidMoves (board: Board.T) (color: Piece.Colors) =
        let getValidMovesForCoord src =
            // src coord is already filtered - it is not empty and contains piece of active player
            let tryGetMoveWithDir dir =
                // we return Success move to relative coord in specified direction if possible or Error
                let tryCreateMove tar tarsq =
                    // we create move if target square is empty
                    match tarsq with
                    | Square.Nothing -> 
                        let nsrcsq = Square.createEmpty
                        let ntarsq = Square.createWithPiece <| Piece.create color
                        tryChangeError UnableToCreateMove <| Move.tryCreate src tar nsrcsq ntarsq
                    | _ -> Error TargetSquareIsNotEmpty
                    
                maybe {
                    // try to get relative coord
                    let! tar = tryChangeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative src dir
                    // get target square
                    let! tarsq = tryChangeError UnableToGetTargetSquare <| Board.tryGetSquare board tar
                    return! tryCreateMove tar tarsq }
            Seq.fold (fun result dir ->
                        match tryGetMoveWithDir dir with
                        | Success move -> move::result
                        | _ -> result) [] Coord.Directions
            
        Seq.fold (fun result coord ->
                    List.append result <| getValidMovesForCoord coord) [] <| board.GetCoordsWithPieceColor color
