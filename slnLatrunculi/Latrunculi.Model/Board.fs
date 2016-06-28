﻿namespace Latrunculi.Model

module Board =

    type T() = 
        let sq = Array.init (Seq.length Coord.RowNumbers) (fun _ -> 
            Array.create (Seq.length Coord.ColumnNumbers) Square.createEmpty)

        member val private Squares = sq
        member val History = History.create with get, set

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
            Seq.toList <| Seq.filter (fun coord ->
                                        Square.containsColor color <| this.GetSquare(coord))
                                    Coord.getCoordsSeq

        member this.GetNumberOfPiecesByColor (color: Piece.Colors) =
            Array.fold (fun count row ->
                            count + Array.fold (fun count sq ->
                                                count + match sq with
                                                        | Square.Piece p -> if p.Color = color then 1 else 0
                                                        | _ -> 0) 0 row)
                        0 this.Squares
            
        member this.WhitePiecesCount =
            this.GetNumberOfPiecesByColor Piece.Colors.White

        member this.BlackPiecesCount =
            this.GetNumberOfPiecesByColor Piece.Colors.Black

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

    let whitePiecesCount (board: T) =
        board.WhitePiecesCount

    let blackPiecesCount (board: T) =
        board.BlackPiecesCount

    let getSquare (board: T) coord =
        board.GetSquare coord

    let tryGetSquare (board: T) coord =
        Success (getSquare board coord)

    let move (board: T) (move: BoardMove.T) =
        let addMoveToHistory =
            let id = 1 + List.length board.History
            let item = HistoryItem.create id move
            board.History <- History.getHistoryWithNewItem board.History item

        let m = move.Move 
        board.ChangeSquare m.Source m.NewSourceSquare
        board.ChangeSquare m.Target m.NewTargetSquare
        List.iter (fun (x: RemovedPiece.T) ->
                    board.ChangeSquare x.Coord Square.createEmpty) move.RemovedPieces
        addMoveToHistory

    let invmove (board: T) (move: BoardMove.T) =
        let removeMoveFromHistory =
            board.History <- match List.length board.History with
                             | l when l > 0 -> List.tail board.History
                             | _ -> board.History
            
        let m = move.Move
        board.ChangeSquare m.Source m.NewTargetSquare
        board.ChangeSquare m.Target m.NewSourceSquare
        List.iter (fun (x: RemovedPiece.T) ->
                    board.ChangeSquare x.Coord (Square.createWithPiece x.Piece)) move.RemovedPieces
        removeMoveFromHistory

    let tryInit (board: T) (getInitalSquare: Coord.T -> Square.T) =
        try
            board.History <- History.create
            Coord.iter (fun c ->
                    board.ChangeSquare c <| getInitalSquare c)
            Success board
        with
        | _ -> Error UnableToInitializeBoard

    let clone (source: T) =
        let result = T()
        result.History <- source.History
        unwrapResultExn <| tryInit result source.GetSquare

    let create() =
        T()
