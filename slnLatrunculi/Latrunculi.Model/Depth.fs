namespace Latrunculi.Model

module Depth =

    [<StructuralEquality;StructuralComparison>]
    type T = T of int
    let create (x: int) =
        if x < 0 then invalidArg "x" "Hloubka výpočtu nesmí být menší než nula."
            else T x
    let isZero (depth: T) =
        match depth with
        | T d when d = 0 -> true
        | _ -> false
    let dec (depth: T) =
        let (T value) = depth
        T (value - 1)
