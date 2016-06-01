namespace Latrunculi.Model

module Piece =
    
    type Colors = 
        | White = 0
        | Black = 1

    [<StructuralEquality;NoComparison>]
    type T = {
        Color: Colors }
    
    let Create x =
        { Color = x }