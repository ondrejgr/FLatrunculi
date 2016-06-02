namespace Latrunculi.Model

module Board =
    type Error =
        | InvalidMoveSpecified

    type T() = 
        member this.Squares = Array.init (Seq.length Coord.RowNumbers) (fun _ -> 
            Array.create (Seq.length Coord.ColumnNumbers) Square.createEmpty)
        
        member this.ChangeSquare (c: Coord.T) (s: Square.T) =
            let col = Coord.getColumnIndex c.Column
            let row = Coord.getRowIndex c.Row
            Array.set this.Squares.[row] col s
            this.Squares

//    let tryMove move =
//        match move with
//        | Error _ -> Error InvalidMoveSpecified
//        | Success m -> 

    let create =
        T()
