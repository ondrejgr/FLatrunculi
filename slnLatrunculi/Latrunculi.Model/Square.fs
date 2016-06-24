namespace Latrunculi.Model
open System.Runtime.Serialization
open System.Reflection
open Microsoft.FSharp.Reflection

module Square =

    [<KnownType("GetKnownTypes")>]
    [<StructuralEquality;NoComparison>]
    type T =
        | Piece of Piece.T
        | Nothing
        static member GetKnownTypes() = 
            typedefof<T>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) |> Array.filter FSharpType.IsUnion

    let isEmpty x =
        match x with
        | Nothing -> true
        | _ -> false

    let isNotEmpty x =
        not <| isEmpty x

    let containsColor (color: Piece.Colors) x =
        match x with 
        | Nothing -> false
        | Piece p -> p.Color = color

    let createWithPiece piece =
        Piece piece

    let createEmpty =
        Nothing