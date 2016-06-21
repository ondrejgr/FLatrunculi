namespace Latrunculi.Model


module Rules =
    [<StructuralEquality;NoComparison>]
    type Victory =
        | WhiteWinner
        | BlackWinner

    type GameOverResult =
        | Draw
        | Victory of Victory

    [<StructuralEquality;NoComparison>]
    type GameResult =
        | NoResult
        | GameOverResult of GameOverResult

    let checkVictory board numOfMovesWithoutRemoval =
        let whiteCount = Board.whitePiecesCount board
        let blackCount = Board.blackPiecesCount board
        let getResult =
            if whiteCount = blackCount
                then GameOverResult Draw
                else if whiteCount > blackCount
                    then GameOverResult (Victory WhiteWinner)
                    else GameOverResult (Victory BlackWinner)

        if whiteCount = 0 || blackCount = 0 || numOfMovesWithoutRemoval = 30
            then getResult
            else NoResult

    let getInitialBoardSquares (coord: Coord.T) =
        match coord with
        | { Row = Coord.RowNumber 7 } | { Row = Coord.RowNumber 6 } -> Square.createWithPiece <| Piece.createBlack
        | { Row = Coord.RowNumber 2 } | { Row = Coord.RowNumber 1 } -> Square.createWithPiece <| Piece.createWhite
        | _ -> Square.createEmpty

    let getEmptyBoardSquares (coord: Coord.T) =
        Square.createEmpty

    let getInitialActiveColor =
        Piece.Colors.White

        
    let getValidMovesForCoord board color src =
        // src coord is already filtered - it is not empty and contains piece of active player
        let tryGetMoveWithDir dir =
            // we return Success move to relative coord in specified direction if possible or Error
            let tryCreateMove tar tarsq =
                // we create move if target square is empty
                match tarsq with
                | Square.Nothing -> 
                    let nsrcsq = Square.createEmpty
                    let ntarsq = Square.createWithPiece <| Piece.create color
                    Move.tryCreate src tar nsrcsq ntarsq
                | _ -> Error TargetSquareIsNotEmpty
                    
            maybe {
                // try to get relative coord
                let! tar = changeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative src dir
                // get target square
                let! tarsq = Board.tryGetSquare board tar
                return! tryCreateMove tar tarsq }
           
        match Board.tryGetSquare board src with
        | Success sq when (Square.containsColor color sq) -> 
            seq {
                for dir in Coord.Directions do
                    match tryGetMoveWithDir dir with
                    | Success move -> yield move
                    | _ -> () }
        | _ -> Seq.empty 

    let getValidMoves (board: Board.T) (color: Piece.Colors) =
        let getValidMovesForBoardColorCoord coord =
            Seq.toList <| getValidMovesForCoord board color coord
        List.collect getValidMovesForBoardColorCoord <| board.GetCoordsWithPieceColor color

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
        let target = move.Target

        let tryCaptureEnemyInLine c1 dir =
            // capture enemy
            let tryGetRemovedPiece c2 c3 =
                let sq2 = board.GetSquare c2
                let sq3 = board.GetSquare c3
                if (Square.containsColor ownColor sq3) && (Square.containsColor enemyColor sq2) then
                    Success (RemovedPiece.create c2 enemyPiece)
                else
                    Error UnableToRemovePiece
            maybe {
                let! c2 = changeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative c1 dir
                let! c3 = changeError RelativeCoordIsOutOfRange <| Coord.tryGetRelative c2 dir                
                return! tryGetRemovedPiece c2 c3 }

        let tryCaptureEnemyInCorner (boardCorner: Coord.BoardCorner) =
            // capture enemy in corner
            let corner = (fst boardCorner)
            let cc1 = (fst (snd boardCorner))
            let cc2 = (snd (snd boardCorner))

            let sqc = board.GetSquare corner
            let sqc1 = board.GetSquare cc1
            let sqc2 = board.GetSquare cc2

            let tryIsEnemyInCorner =
                match Square.containsColor enemyColor sqc with
                | true -> Success ()
                | false -> Error UnableToRemovePiece
            let tryGetRemovedPiece =
                if (target = cc1) && (Square.containsColor ownColor sqc2) then 
                    Success (RemovedPiece.create corner enemyPiece) 
                elif (target = cc2) && (Square.containsColor ownColor sqc1) then 
                    Success (RemovedPiece.create corner enemyPiece) 
                else Error UnableToRemovePiece

            maybe {
                do! tryIsEnemyInCorner
                return! tryGetRemovedPiece }

        match isMoveValid board color move with
        | false -> Error MoveIsNotValid
        | true ->
            let result = List.concat <| seq {
                yield Seq.toList <| Seq.fold (fun result dir ->
                        match tryCaptureEnemyInLine c1 dir with
                        | Success rmpiece -> rmpiece::result
                        | _ -> result) [] Coord.Directions 
                yield Seq.toList <| Seq.fold (fun result corner ->
                        match tryCaptureEnemyInCorner corner with
                        | Success rmpiece -> rmpiece::result
                        | _ -> result) [] Coord.getBoardCornersSeq }
            
            Success (BoardMove.createWithRmPieces move result)
