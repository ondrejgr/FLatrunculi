namespace Latrunculi.Model

module Brain =
    
    let rnd = System.Random()

    let tryGetBestMove (board: Board.T) (color: Piece.Colors): Async<Result<Move.T, Error>> =
        async {
            do! Async.Sleep(10)
            let moves = Rules.getValidMoves board color
            match List.isEmpty moves with
            | false -> 
                let index = rnd.Next(0, List.length moves)
                return Success (List.item index moves)
            | true -> return Error NoValidMoveExists }
