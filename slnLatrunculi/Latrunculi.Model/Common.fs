[<AutoOpen>]
module Common

    type Result<'TSuccess,'TError> = 
        | Success of 'TSuccess 
        | Error of 'TError

    let tryChangeError m e =
        match m with
        | Success x -> Success x
        | Error _ -> Error e

    let getObjExn c =
        match c with
        | Success c -> c
        | _ -> failwith "Unable to extract object instance from function result, because called function has failed."

    type MaybeBuilder() =
        member this.Bind(m, f) = 
            match m with
            | Success s -> f s
            | Error e -> Error e

        member this.Return(x) = 
            Success x

        member this.ReturnFrom(m) =
            m

    let maybe = new MaybeBuilder()
