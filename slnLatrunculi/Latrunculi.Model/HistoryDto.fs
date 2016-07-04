namespace Latrunculi.Model
open System.Runtime.Serialization

module CoordDto =
    [<DataContract(Namespace="")>]
    type T = string
    
    let coordToDto (coord: Coord.T) =
        Coord.toString coord

    let tryDtoToCoord (source: T) =
        Coord.tryCreateFromString source

module PieceColorDto =
    [<DataContract(Namespace="")>]
    type T = char

    let pieceColorToDto (color: Piece.Colors) =
        match color with
        | Piece.Colors.Black -> 'B'
        | Piece.Colors.White -> 'W'

    let tryDtoToPieceColor (source: char) =
        match source with
        | 'B' -> Success Piece.Colors.Black
        | 'W' -> Success Piece.Colors.White
        | _ -> Error (UnableToDeserializeObject "PieceColor")

module PieceDto =
    [<DataContract(Namespace="")>]
    type T = char

    let pieceToDto (piece: Piece.T) =
        PieceColorDto.pieceColorToDto piece.Color

    let tryDtoToPiece (source: T) =
        maybe {
            let! color = PieceColorDto.tryDtoToPieceColor source
            return Piece.create color }

module SquareDto =
    [<DataContract(Namespace="")>]
    type T = char

    let squareToDto (square: Square.T) =
        match square with
        | Square.Nothing -> 'N'
        | Square.Piece p -> PieceDto.pieceToDto p

    let tryDtoToSquare (source: T) =
        match source with
        | 'N' -> Success Square.createEmpty
        | 'W' -> Success (Square.createWithPiece <| Piece.createWhite)
        | 'B' -> Success (Square.createWithPiece <| Piece.createBlack)
        | _ -> Error (UnableToDeserializeObject "SquareType")

module RemovedPieceDto =
    [<DataContract(Namespace="")>]
    type T = string

    let fromRemovedPiece (source: RemovedPiece.T) =
        String.concat ""
                <| seq {
                        yield CoordDto.coordToDto source.Coord
                        yield sprintf "%O" <| PieceDto.pieceToDto source.Piece }

    let tryDtoToRemovedPiece (source: T) =
        match source with
        | s when String.length s = 3 ->
            maybe {
                let! coord = CoordDto.tryDtoToCoord s.[0..1]
                let! piece = PieceDto.tryDtoToPiece s.[2]
                return RemovedPiece.create coord piece }
        | _ -> Error (UnableToDeserializeObject source)

module RemovedPiecesDto =
    [<DataContract(Namespace="")>]
    type T = RemovedPieceDto.T array

    let fromRemovedPieces (source: RemovedPiece.T list) =
        List.toArray <| List.map (fun item ->
                                RemovedPieceDto.fromRemovedPiece item) source

    let tryDtoToRemovedPieces (source: T) =
        let empty: RemovedPiece.T list = list.Empty
        Array.foldBack (fun (item: RemovedPieceDto.T) result ->
                    match result with 
                    | Success lst ->
                        match RemovedPieceDto.tryDtoToRemovedPiece item with
                        | Success rm -> Success (rm::lst)
                        | Error e -> Error e
                    | Error e -> Error e) source (Success empty)


module MoveDto =
    [<DataContract(Namespace="")>]
    type T = string

    let fromMove (source: Move.T) =
        String.concat ""
                <| seq {
                        yield CoordDto.coordToDto source.Source
                        yield CoordDto.coordToDto source.Target
                        yield sprintf "%O" <| SquareDto.squareToDto source.NewSourceSquare
                        yield sprintf "%O" <| SquareDto.squareToDto source.NewTargetSquare }

    let tryDtoToMove (source: T) =
        match source with
        | s when String.length s = 6 ->
                maybe {
                    let! newSourceSquare = SquareDto.tryDtoToSquare s.[4]
                    let! newTargetSquare = SquareDto.tryDtoToSquare s.[5]
                    return! Move.tryCreateFromStrCoord s.[0..1] s.[2..3] newSourceSquare newTargetSquare }
        | _ -> Error (UnableToDeserializeObject source)

module BoardMoveDto =
    [<CLIMutable>]
    [<DataContract(Namespace="")>]
    type T = {
        [<DataMember>]
        Color: PieceDto.T
        [<DataMember>]
        Move: MoveDto.T
        [<DataMember>]
        RemovedPieces: RemovedPiecesDto.T }

    let fromBoardMove (source: BoardMove.T) =
        {   Color = PieceColorDto.pieceColorToDto source.Color           
            Move = MoveDto.fromMove source.Move
            RemovedPieces = RemovedPiecesDto.fromRemovedPieces source.RemovedPieces }

    let tryToBoardMove (source: T) =
        maybe {
            let! color = PieceColorDto.tryDtoToPieceColor source.Color
            let! move = MoveDto.tryDtoToMove source.Move
            let! rm = RemovedPiecesDto.tryDtoToRemovedPieces source.RemovedPieces
            return BoardMove.createWithRmPieces color move rm }       

module MoveStackDto =
    [<DataContract(Namespace="")>]
    type T = BoardMoveDto.T array

    let fromMoveStack (source: MoveStack.T) =
        List.toArray <| MoveStack.map (fun bmove ->
                                         BoardMoveDto.fromBoardMove bmove) source

    let tryToMoveStack (source: T) =
        Array.foldBack (fun (item: BoardMoveDto.T) result ->
                    match result with
                    | Success stack ->
                        match BoardMoveDto.tryToBoardMove item with
                        | Success bmove -> Success <| MoveStack.push stack bmove
                        | Error e -> Error e
                    | Error e -> Error e) source (Success MoveStack.create)
        

module HistoryDto =
    [<CLIMutable>]
    [<DataContract(Namespace="")>]
    type T = {
        [<DataMember>]
        UndoStack: MoveStackDto.T
        [<DataMember>]
        RedoStack: MoveStackDto.T }

    let fromHistory (source: History.T) =
        {   UndoStack = MoveStackDto.fromMoveStack source.UndoStack
            RedoStack = MoveStackDto.fromMoveStack source.RedoStack }

    let tryToHistory (source: T) (target: History.T) =
        maybe {
            let! undoStack = MoveStackDto.tryToMoveStack source.UndoStack
            let! redoStack = MoveStackDto.tryToMoveStack source.RedoStack
            target.UndoStack <- undoStack
            target.RedoStack <- redoStack
            return () }

