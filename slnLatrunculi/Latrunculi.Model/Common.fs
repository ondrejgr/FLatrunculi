[<AutoOpen>]
module Common

    type Result<'TSuccess,'TError> = 
        | Success of 'TSuccess 
        | Error of 'TError

    type MaybeBuilder() =
        member this.Bind(x, f) = 
            match x with
            | Success s -> f s
            | Error e -> Error e

        member this.Return(x) = 
            Success x

    