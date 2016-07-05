namespace Latrunculi.Model

module MoveStack =

    [<NoEquality;NoComparison>]
    type T = 
        | EmptyStack
        | MoveStack of BoardMove.T list

    let create =
        EmptyStack

    let length (stack: T) =
        match stack with
        | EmptyStack -> 0
        | MoveStack m -> List.length m

    let map (f: BoardMove.T -> 'U) (stack: T) =
        let empty: 'U list = list.Empty
        match stack with
        | EmptyStack -> empty
        | MoveStack m -> List.map f m

    let fold (folder: 'State -> BoardMove.T -> 'State) (state: 'State) (stack: T): 'State =
        match stack with
        | EmptyStack -> state
        | MoveStack m -> List.fold folder state m

    let foldBack (folder: BoardMove.T -> 'State -> 'State) (stack: T) (state: 'State): 'State =
        match stack with
        | EmptyStack -> state
        | MoveStack m -> List.foldBack folder m state

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