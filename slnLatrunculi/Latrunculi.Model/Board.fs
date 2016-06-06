namespace Latrunculi.Model

module Board =
    type Error =
        | UnableToIterateBoard
        | InvalidCoordSpecified
        | InvalidMoveSpecified

    type T() = 
        let sq = Array.init (Seq.length Coord.RowNumbers) (fun _ -> 
            Array.create (Seq.length Coord.ColumnNumbers) Square.createEmpty)

        member val Squares = sq

        member this.GetSquare (c: Coord.T) =
            let col = Coord.ColIndex.[c.Column]
            let row = Coord.RowIndex.[c.Row]
            this.Squares.[row].[col]
        
        member this.ChangeSquare (c: Coord.T) (s: Square.T) =
            let col = Coord.ColIndex.[c.Column]
            let row = Coord.RowIndex.[c.Row]
            Array.set this.Squares.[row] col s
            ()


    let tryGetSquare (board: T) (coord: Result<Coord.T, Coord.Error>) =
        match coord with
        | Error _ -> Error InvalidCoordSpecified
        | Success c -> Success (board.GetSquare c)

    let tryMove (board: T) (move: Result<Move.T, Move.Error>) =
        match move with
        | Error _ -> Error InvalidMoveSpecified
        | Success m -> 
            board.ChangeSquare m.Source m.NewSourceSquare
            board.ChangeSquare m.Target m.NewTargetSquare
            Success ()

    let tryInit (board: T) (getInitalSquare: Coord.T -> Square.T) =
        try
            Coord.iter (fun c ->
                    board.ChangeSquare c <| getInitalSquare c)
            Success board
        with
        | _ -> Error UnableToIterateBoard

    let create =
        T()
