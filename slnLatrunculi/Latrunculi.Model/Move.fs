namespace Latrunculi.Model

module Move =
    type Error =
        | InvalidSourceCoordSpecified
        | InvalidTargetCoordSpecified
        | SourceAndTargetMayNotBeSame

    type T = {
        Source: Coord.T;
        Target: Coord.T;
        NewSourceSquare: Square.T;
        NewTargetSquare: Square.T }

    let tryCreate x y nx ny =
        let getCoord (c: Result<Coord.T, Coord.Error>) (e: Error) =
            match c with
            | Success c -> Success c
            | Error _ -> Error e

        let checkSourceAndTarget src tar =
            if src = tar then Error SourceAndTargetMayNotBeSame else Success ()

        let maybe = new MaybeBuilder()
        maybe {
            let! source = getCoord x InvalidSourceCoordSpecified
            let! target = getCoord y InvalidTargetCoordSpecified
            let! newSourceSq = Success nx
            let! newTargetSq = Success ny
            do! checkSourceAndTarget source target
            return Success { 
                Source = source; 
                Target = target;
                NewSourceSquare = newSourceSq;
                NewTargetSquare = newTargetSq }}
