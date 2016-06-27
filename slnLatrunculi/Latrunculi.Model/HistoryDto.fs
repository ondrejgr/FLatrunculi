namespace Latrunculi.Model
open System.Runtime.Serialization

module MoveDto =
    [<DataContract>]
    type T = string

    let fromMove (source: Move.T) =
        let squareToDto (square: Square.T) =
            match square with
            | Square.Nothing -> "N"
            | Square.Piece p ->
                match p.Color with
                | Piece.Colors.Black -> "B"
                | Piece.Colors.White -> "W"
        let coordToDto (coord: Coord.T) =
            Coord.toString coord

        String.concat ""
                <| seq {
                        yield coordToDto source.Source
                        yield coordToDto source.Target
                        yield squareToDto source.NewSourceSquare
                        yield squareToDto source.NewTargetSquare }

    let tryDtoToSquare (source: char) =
        match source with
        | 'N' -> Success Square.createEmpty
        | 'W' -> Success (Square.createWithPiece <| Piece.createWhite)
        | 'B' -> Success (Square.createWithPiece <| Piece.createBlack)
        | _ -> Error (UnableToDeserializeObject "SquareType")

module MovesDto =
    [<DataContract>]
    type T = MoveDto.T array

    let fromHistory (source: History.T) =
        let moves: T = List.toArray <| (List.map (fun (item: HistoryItem.T) -> MoveDto.fromMove item.BoardMove.Move) 
                                                <| List.rev source)
        moves

    let tryToMoveList (source: T): Result<Move.T list, Error> =
        let empty: Move.T list = List.empty
        Array.foldBack 
            (fun (moveDto: MoveDto.T) result ->
                match result with
                | Success lst ->
                    match moveDto with
                    | s when String.length s = 6 ->
                        match maybe {
                                    let! newSourceSquare = MoveDto.tryDtoToSquare s.[4]
                                    let! newTargetSquare = MoveDto.tryDtoToSquare s.[5]
                                    return! Move.tryCreateFromStrCoord s.[0..1] s.[2..3] newSourceSquare newTargetSquare } with
                        | Success move -> Success (move::lst)
                        | Error e -> Error e
                    | _ -> Error (UnableToDeserializeObject moveDto)
                | Error e -> Error e)
            source (Success empty)


module HistoryDto =
    [<CLIMutable>]
    [<DataContract>]
    type T = {
        [<DataMember>]
        Moves: MovesDto.T }

    let fromHistory (source: History.T) =
        {   Moves = MovesDto.fromHistory source }
