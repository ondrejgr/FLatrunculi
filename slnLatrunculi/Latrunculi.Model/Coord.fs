namespace Latrunculi.Model

module Coord =
    type Error =
        | InvalidColumnNumber
        | InvalidRowNumber
        | UnableToParseCoordFromString

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
