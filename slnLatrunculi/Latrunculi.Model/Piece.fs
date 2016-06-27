namespace Latrunculi.Model

module Piece =
    
    type Colors = 
        | White
        | Black

    [<StructuralEquality;NoComparison>]
    type T = {
        Color: Colors }
    
    let swapColor (x: Colors) =
        match x with
        | White -> Black
        | Black -> White

    let create x =
        { Color = x }

    let createWhite =
        create White

    let createBlack =
        create Black
