namespace Latrunculi.Model

module MoveStack =

    [<NoEquality;NoComparison>]
    type T = 
        | EmptyStack
        | MoveStack of BoardMove.T list

    let create =
        EmptyStack

    let map (f: BoardMove.T -> 'U) (stack: T) =
        let empty: 'U list = list.Empty
        match stack with
        | EmptyStack -> empty
        | MoveStack m -> List.map f m

    let push (stack: T) (move: BoardMove.T) =
        match stack with
        | EmptyStack -> 
            MoveStack (move::[])
        | MoveStack moves ->
            MoveStack (move::moves)

    let tryPop (stack: T) =
        match stack with
        | EmptyStack ->
            Error StackIsEmpty
        | MoveStack moves ->
            let move = List.head moves
            match List.length moves with
            | len when len = 1 -> 
                Success (create, move)
            | _ ->
                Success (MoveStack <| List.tail moves, move)

    let isEmpty (stack: T) =
        match stack with
        | EmptyStack -> true
        | MoveStack _ -> false