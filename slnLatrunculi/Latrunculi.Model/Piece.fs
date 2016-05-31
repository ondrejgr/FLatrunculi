namespace Latrunculi.Model

module Piece =

    [<StructuralEquality;NoComparison>]
    type T = {
        Color: PieceColors }
    
    let Create color =
        { Color = color }