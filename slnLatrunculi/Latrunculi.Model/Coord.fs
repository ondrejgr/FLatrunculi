namespace Latrunculi.Model

module Coord =
    type ErrorMessage =
        | InvalidColumnNumber
        | InvalidRowNumber
        | UnableToParseCoordFromString

    type ColumnNumber = ColumnNumber of char
    type RowNumber = RowNumber of int

    let ColumnNumbers = seq {'A'..'H'}
    let RowNumbers = seq {1..7}

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

        let maybe = new MaybeBuilder()
        maybe {
            let! col = tryCheckColumnNumberRange x
            let! row = tryCheckRowNumberRange y
            return { Column = col; Row = row } } 

//    let tryCreateFromString (s: string) =

//        match s with
        //| null -> Error UnableToParseCoordFromString
        //| _ when s.Length <> 2 -> Error UnableToParseCoordFromString
        //| _ -> parseColumnNumber <| s.Chars 0