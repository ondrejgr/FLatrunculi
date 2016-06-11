namespace Latrunculi.Model

module Move =
    type Error =
        | InvalidSourceCoordSpecified
        | InvalidTargetCoordSpecified
        | SourceAndTargetMayNotBeSame

    [<StructuralEquality;NoComparison>]
    type T = {
        Source: Coord.T;
        Target: Coord.T;
        NewSourceSquare: Square.T;
        NewTargetSquare: Square.T;
        RemovedPieces: RemovedPiece.T list }

    let tryCreateWithRemovedPiecesList x y nx ny xys =
        let getCoord (c: Result<Coord.T, Coord.Error>) (e: Error) =
            match c with
            | Success c -> Success c
            | Error _ -> Error e

        let checkSourceAndTarget src tar =
            if src = tar then Error SourceAndTargetMayNotBeSame else Success ()

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
                NewTargetSquare = newTargetSq;
                RemovedPieces = xys }}

    let tryCreate x y nx ny =
        tryCreateWithRemovedPiecesList x y nx ny []

    let tryCreateFromStringCoords x y nx ny =
        tryCreate (Coord.tryCreateFromString x) (Coord.tryCreateFromString y) nx ny
