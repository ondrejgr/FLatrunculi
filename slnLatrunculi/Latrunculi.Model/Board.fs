namespace Latrunculi.Model

module Board =

    [<StructuralEquality;NoComparison>]
    type T = {
        Color: PieceColors }
    
    let Create color =
        { Color = color }