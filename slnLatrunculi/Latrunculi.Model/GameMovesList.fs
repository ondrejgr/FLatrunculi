﻿namespace Latrunculi.Model
open System.Runtime.Serialization

module GameMovesList =
    [<CLIMutable>]
    [<DataContract>]
    type T = Move.T array


    let createFromHistory (history: History.T) =
        let result: T = List.toArray <| List.map (fun (item: HistoryItem.T) -> item.BoardMove.Move) history
        result
