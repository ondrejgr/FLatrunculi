namespace Latrunculi.Model
open System.Runtime.Serialization
open System.Reflection
open Microsoft.FSharp.Reflection

module Piece =
    
    [<KnownType("GetKnownTypes")>]
    type Colors = 
        | White
        | Black
        static member GetKnownTypes() = 
            typedefof<Colors>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) |> Array.filter FSharpType.IsUnion

    [<StructuralEquality;NoComparison>]
    [<CLIMutable>]
    [<DataContract>]
    type T = {
        [<DataMember>]
        Color: Colors }
    
    let swapColor (x: Colors) =
        match x with
        | White -> Black
        | Black -> White

    let create x =
        { Color = x }

    let createWhite =
        create White

    let createBlack =
        create Black
