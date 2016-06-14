namespace Latrunculi.Model

module Brain =
    
    type Error = 
        | UnableToComputeMove

    let tryGetBestMove (board: Board.T) (color: Piece.Colors): Result<Move.T, Error> =
        match List.tryHead <| Rules.getValidMoves board color with
        | Some m -> Success m
        | None -> Error UnableToComputeMove
