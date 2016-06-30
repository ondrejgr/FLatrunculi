namespace Latrunculi.Model

module Move =

    [<StructuralEquality;NoComparison>]
    [<StructuredFormatDisplay("{Source}{Target}/{NewSourceSquare} {NewTargetSquare}")>]
    type T = {
        Source: Coord.T;
        Target: Coord.T;
        NewSourceSquare: Square.T;
        NewTargetSquare: Square.T }

    let checkSourceAndTarget src tar =
        if src = tar then Error SourceAndTargetCoordMayNotBeSame else Success ()

    let tryCreate src tar nsrcsq ntarsq =
        maybe {
            do! checkSourceAndTarget src tar
            return { 
                Source = src; 
                Target = tar;
                NewSourceSquare = nsrcsq;
                NewTargetSquare = ntarsq }}

    let tryCreateFromStrCoord src tar nsrcsq ntarsq =
        maybe {
            let! a = changeError InvalidSourceCoord <| Coord.tryCreateFromString src
            let! b = changeError InvalidTargetCoord <| Coord.tryCreateFromString tar
            return! tryCreate a b nsrcsq ntarsq }

    let createFromStrCoordExn src tar nsrcsq ntarsq =
        match tryCreateFromStrCoord src tar nsrcsq ntarsq with
        | Success m -> m
        | _ -> failwith "Unable to create move from string coordinates."
        
