[<AutoOpen>]
module Common

    type Result<'TSuccess,'TError> = 
        | Success of 'TSuccess 
        | Error of 'TError

    let getObjExn c =
        match c with
        | Success c -> c
        | _ -> failwith "Unable to extract object instance from function result, because called function has failed."

    type MaybeBuilder() =
        member this.Bind(x, f) = 
            match x with
            | Success s -> f s
            | Error e -> Error e

        member this.Return(x) = 
            x

    let maybe = new MaybeBuilder()
    