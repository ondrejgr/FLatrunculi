namespace Latrunculi.Model

module Move =
    type Error =
        | InvalidSourceCoord
        | InvalidTargetCoord
        | SourceAndTargetMayNotBeSame

    [<StructuralEquality;NoComparison>]
    type T = {
        Source: Coord.T;
        Target: Coord.T;
        NewSourceSquare: Square.T;
        NewTargetSquare: Square.T;
        RemovedPieces: RemovedPiece.T list }

    let checkSourceAndTarget src tar =
        if src = tar then Error SourceAndTargetMayNotBeSame else Success ()

    let tryCreateWithRemovedPiecesList src tar nsrcsq ntarsq rmpieces =
        maybe {
            do! checkSourceAndTarget src tar
            return { 
                Source = src; 
                Target = tar;
                NewSourceSquare = nsrcsq;
                NewTargetSquare = ntarsq;
                RemovedPieces = rmpieces }}

    let tryCreate src tar nsrcsq ntarsq =
        tryCreateWithRemovedPiecesList src tar nsrcsq ntarsq []

    let tryCreateFromStringCoords src tar nsrcsq ntarsq =
        maybe {
            let! a = tryChangeError InvalidSourceCoord <| Coord.tryCreateFromString src
            let! b = tryChangeError InvalidTargetCoord <| Coord.tryCreateFromString tar
            return! tryCreateWithRemovedPiecesList a b nsrcsq ntarsq [] }
