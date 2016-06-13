namespace Latrunculi.Model


module Rules =
    type Error =
        | RelativeCoordIsOutOfRange
        | UnableToGetTargetSquare
        | UnableToCreateMove
        | TargetSquareIsNotEmpty
        | MoveIsNotValid
        | UnableToRemovePiece

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

    let isMoveValid (board: Board.T) (color: Piece.Colors) (move: Move.T) =
        List.exists (fun m -> m = move) <| getValidMoves board color

    let tryIsMoveValid (board: Board.T) (color: Piece.Colors) (move: Move.T) =
        match List.exists (fun m -> m = move) <| getValidMoves board color with
        | true -> Success move
        | false -> Error MoveIsNotValid

    let tryValidateAndGetBoardMove (board: Board.T) (color: Piece.Colors) (move: Move.T) =
        let ownPiece = Piece.create color
        let enemyPiece = if (color = Piece.Colors.Black) then Piece.createWhite else Piece.createBlack
        let ownColor = color
        let enemyColor = if (color = Piece.Colors.Black) then Piece.Colors.White else Piece.Colors.Black
        let c1 = move.Target

        let tryCaptureEnemyInLine c1 dir =
            // sebrani obkliceneho soupere
            let tryGetRemovedPiece c2 c3 =
                let sq2 = board.GetSquare c2
                let sq3 = board.GetSquare c3
                if (Square.containsColor ownColor sq3) && (Square.containsColor enemyColor sq2) then
                    Success (RemovedPiece.create c2 enemyPiece)
                else
                    Error UnableToRemovePiece
            maybe {
                let! c2 = tryChangeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative c1 dir
                let! c3 = tryChangeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative c2 dir                
                return! tryGetRemovedPiece c2 c3 }

        match isMoveValid board color move with
        | false -> Error MoveIsNotValid
        | true ->
            let result = List.concat <| seq {
                yield Seq.toList <| Seq.fold (fun result dir ->
                        match tryCaptureEnemyInLine c1 dir with
                        | Success rmpiece -> rmpiece::result
                        | _ -> result) [] Coord.Directions }
            
            Success (BoardMove.createWithRmPieces move result)
