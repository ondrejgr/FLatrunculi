namespace Latrunculi.Model

module MoveValue =

    [<CustomEquality;CustomComparison>]
    type T =
        | Value of int
        | MAX
        | MIN
        interface System.IComparable with
            member x.CompareTo yobj =
                match yobj with
                | :? T as y ->
                    match y with
                    | Value vy ->
                        match x with
                        | Value vx -> compare vx vy
                        | MAX -> 1
                        | MIN -> -1
                    | MAX ->
                        match x with
                        | Value vx -> -1
                        | MAX -> 0
                        | MIN -> - 1
                    | MIN ->
                        match x with
                        | Value vx -> 1
                        | MAX -> 1
                        | MIN -> 0
                | _ -> invalidArg "yobj" "cannot compare values of different types"
        override x.Equals yobj = 
            match yobj with
            | :? T as y ->
                    match y with
                    | Value vy ->
                        match x with
                        | Value vx -> vx = vy
                        | MAX -> false
                        | MIN -> false
                    | MAX ->
                        match x with
                        | Value vx -> false
                        | MAX -> true
                        | MIN -> false
                    | MIN ->
                        match x with
                        | Value vx -> false
                        | MAX -> false
                        | MIN -> true
            | _ -> false
        override x.GetHashCode() = 
            match x with
            | Value vx -> vx.GetHashCode()
            | MAX -> System.Int32.MaxValue.GetHashCode()
            | MIN -> System.Int32.MinValue.GetHashCode()

    let getMax =
        MAX

    let getMin =
        MIN

    let getValue (x: int) =
        Value x

    let getInvValue (x: T) =
        match x with
        | Value v -> Value -v
        | MAX -> MIN
        | MIN -> MAX
