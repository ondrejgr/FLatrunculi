namespace Latrunculi.Model

module Piece =
    
    type Colors = 
        | White = 0
        | Black = 1

    [<StructuralEquality;NoComparison>]
    type T = {
        Color: Colors }
    
    let create x =
        { Color = x }

    let createWhite =
        create Colors.White

    let createBlack =
        create Colors.Black
