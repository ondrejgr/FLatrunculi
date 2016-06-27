namespace Latrunculi.Model

module Square =

    [<StructuralEquality;NoComparison>]
    type T =
        | Piece of Piece.T
        | Nothing
 
    let isEmpty x =
        match x with
        | Nothing -> true
        | _ -> false

    let isNotEmpty x =
        not <| isEmpty x

    let containsColor (color: Piece.Colors) x =
        match x with 
        | Nothing -> false
        | Piece p -> p.Color = color

    let createWithPiece piece =
        Piece piece

    let createEmpty =
        Nothing