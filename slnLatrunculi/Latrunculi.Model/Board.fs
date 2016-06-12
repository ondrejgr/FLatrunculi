﻿namespace Latrunculi.Model

module Board =
    type Error =
        | UnableToIterateBoard

    type T() = 
        let sq = Array.init (Seq.length Coord.RowNumbers) (fun _ -> 
            Array.create (Seq.length Coord.ColumnNumbers) Square.createEmpty)

        member val private Squares = sq

        member this.GetSquare (c: Coord.T) =
            let col = Coord.ColIndex.[c.Column]
            let row = Coord.RowIndex.[c.Row]
            this.Squares.[row].[col]
        
        member this.ChangeSquare (c: Coord.T) (s: Square.T) =
            let col = Coord.ColIndex.[c.Column]
            let row = Coord.RowIndex.[c.Row]
            Array.set this.Squares.[row] col s
            ()

        member this.GetCoordsWithPieceColor (color: Piece.Colors) =
            Seq.filter (fun coord ->
                            match this.GetSquare(coord) with
                            | Square.Nothing -> false
                            | Square.Piece p -> p.Color = color)
                        Coord.getCoordsSeq

        member this.GetNumberOfPiecesByColor (color: Piece.Colors) =
            Array.fold (fun count row ->
                            count + Array.fold (fun count sq ->
                                                count + match sq with
                                                        | Square.Piece p -> if p.Color = color then 1 else 0
                                                        | _ -> 0) 0 row)
                        0 this.Squares
            
        member this.GetRowNumbers =
            Coord.RowNumbers

        member this.GetCoordAndSquaresByRowNumber (row: int) =
            seq {
                let rowIndex = Coord.RowIndex.[Coord.RowNumber row]
                for col in Coord.ColumnNumbers do
                    let colIndex = Coord.ColIndex.[Coord.ColumnNumber col]
                    let square = this.Squares.[rowIndex].[colIndex]
                    let coord = match Coord.tryCreate col row with
                                | Success c -> c
                                | _ -> failwith "Souřadnici se nepodařilo vytvořit."
                    yield (coord, square) }

    let getCoordsWithPieceColor (board: T) (color: Piece.Colors) =
        board.GetCoordsWithPieceColor(color)

    let getSquare (board: T) coord =
        board.GetSquare coord

    let move (board: T) (move: Move.T) =
        board.ChangeSquare move.Source move.NewSourceSquare
        board.ChangeSquare move.Target move.NewTargetSquare

    let tryInit (board: T) (getInitalSquare: Coord.T -> Square.T) =
        try
            Coord.iter (fun c ->
                    board.ChangeSquare c <| getInitalSquare c)
            Success board
        with
        | _ -> Error UnableToIterateBoard

    let create =
        T()
