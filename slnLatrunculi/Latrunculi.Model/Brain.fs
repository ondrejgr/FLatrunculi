namespace Latrunculi.Model

module Brain =
    
    type Error = 
        | UnableToComputeMove

    let tryGetBestMove (board: Board.T) (color: Piece.Colors): Async<Result<Move.T, Error>> =
        async {
            do! Async.Sleep(5000)
            match List.tryHead <| Rules.getValidMoves board color with
            | Some m -> return Success m
            | None -> return Error UnableToComputeMove }
