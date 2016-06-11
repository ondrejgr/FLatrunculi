namespace Latrunculi.Model

module Coord =
    type Error =
        | InvalidColumnNumber
        | InvalidRowNumber
        | ColumnOutOfRange
        | RowOutOfRange
        | InvalidSourceCoord
        | UnableToParseCoordFromString

    type Direction =
        | Up
        | Down
        | Left
        | Right

    type ColumnNumber = ColumnNumber of char
    type RowNumber = RowNumber of int

    let ColumnNumbers = seq ['A'..'H']
    let RowNumbers = Seq.init 7 (fun i -> 7 - i) // [7..1]

    let ColIndex =
        Seq.fold (fun (i, xs) x -> 
                    (i + 1, (ColumnNumber x, i)::xs)) 
            (0, []) ColumnNumbers 
        |> snd |> List.rev |> Map.ofList
    let RowIndex =
        Seq.fold (fun (i, xs) x -> 
                    (i + 1, (RowNumber x, i)::xs)) 
            (0, []) RowNumbers 
        |> snd |> List.rev |> Map.ofList

    let getCol (ColumnNumber col) = col
    let getRow (RowNumber row) = row

    let tryGetPrevCol (x: ColumnNumber) =
        let col = getCol x
        let lst = ColumnNumbers
        fst <| Seq.fold (fun (prev, found) a ->
                    match found with
                    | true -> (prev, found)
                    | false -> if a = col then (prev, true) else (Success (ColumnNumber a), false))
                ((Error ColumnOutOfRange), false) lst

    let tryGetNextCol (x: ColumnNumber) =
        let col = getCol x
        let lst = ColumnNumbers |> Seq.toList
        fst <| List.foldBack (fun a (prev, found) ->
                    match found with
                    | true -> (prev, found)
                    | false -> if a = col then (prev, true) else (Success (ColumnNumber a), false))
                lst ((Error ColumnOutOfRange), false)

    let tryGetPrevRow (x: RowNumber) =
        let row = getRow x
        let lst = RowNumbers
        fst <| Seq.fold (fun (prev, found) a ->
                    match found with
                    | true -> (prev, found)
                    | false -> if a = row then (prev, true) else (Success (RowNumber a), false))
                ((Error ColumnOutOfRange), false) lst

    let tryGetNextRow (x: RowNumber) =
        let row = getRow x
        let lst = RowNumbers |> Seq.toList
        fst <| List.foldBack (fun a (prev, found) ->
                    match found with
                    | true -> (prev, found)
                    | false -> if a = row then (prev, true) else (Success (RowNumber a), false))
                lst ((Error ColumnOutOfRange), false)

    [<StructuralEquality;NoComparison>]
    type T = {
        Column: ColumnNumber;
        Row: RowNumber }

    let tryCreate x y =
        let tryCheckColumnNumberRange (col: char) = 
            match Seq.tryFind (fun x -> x = System.Char.ToUpper(col)) ColumnNumbers with
            | Some x -> Success (ColumnNumber x)
            | None -> Error InvalidColumnNumber
        let tryCheckRowNumberRange (row: int) =
            match Seq.tryFind (fun x -> x = row) RowNumbers with
            | Some y -> Success (RowNumber y)
            | None -> Error InvalidRowNumber

        maybe {
            let! col = tryCheckColumnNumberRange x
            let! row = tryCheckRowNumberRange y
            return Success { Column = col; Row = row } } 

    let tryCreateFromString (s: string) =
        let tryParseColumnNumberFromChar (c: char) =
            Success c
        let tryParseRowNumberFromChar (c: char) =
            match System.Int32.TryParse(string c) with
            | (true,i) -> Success i
            | _ -> Error UnableToParseCoordFromString     
                               
        match s with
        | null -> Error UnableToParseCoordFromString
        | _ when s.Length <> 2 -> Error UnableToParseCoordFromString
        | _ -> 
            maybe {
                let! col = tryParseColumnNumberFromChar <| s.Chars 0
                let! row = tryParseRowNumberFromChar <| s.Chars 1
                let! coord = tryCreate col row
                return Success coord }

    let tryGetRelative (coord: Result<T, Error>) (dir: Direction) =
        let getCoord (coord: Result<T, Error>) =
            match coord with
            | Success c -> Success c
            | _ -> Error InvalidSourceCoord
        maybe {
            let! c = getCoord coord
            let! newCol = match dir with
                          | Up | Down -> Success c.Column
                          | Left -> tryGetPrevCol c.Column
                          | Right -> tryGetNextCol c.Column
            let! newRow = match dir with
                          | Left | Right -> Success c.Row
                          | Up -> tryGetPrevRow c.Row
                          | Down -> tryGetNextRow c.Row
                
            let! newCoord = tryCreate (getCol newCol) (getRow newRow)
            return Success newCoord
        }            
        
    let getCoordsSeq =
        seq {
            for row in RowNumbers do
                for col in ColumnNumbers do
                    yield match tryCreate col row with
                                | Success c -> c
                                | _ -> failwith "Souřadnici se nepodařilo vytvořit." } 

    let iter (x: T -> unit) = 
        Seq.iter (fun row -> 
                    Seq.iter (fun col ->
                                let coord = tryCreate col row
                                match coord with
                                | Success c -> x c
                                | _ -> failwith "Souřadnici se nepodařilo vytvořit.")
                        ColumnNumbers) RowNumbers

