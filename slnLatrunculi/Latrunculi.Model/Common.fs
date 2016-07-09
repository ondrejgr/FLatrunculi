[<AutoOpen>]
module Common

type Result<'TSuccess,'TError> = 
    | Success of 'TSuccess 
    | Error of 'TError

let unwrapResultExn c =
    match c with
    | Success c -> c
    | Error e -> failwith <| sprintf "Unable to extract object instance from function result. Error: %A" e

let changeError e m =
    match m with
    | Success x -> Success x
    | Error _ -> Error e

let returnDefault (def: 'TSuccess) (m: Result<'TSuccess, 'TError>): 'TSuccess =
    match m with
    | Success x -> x
    | Error _ -> def

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
