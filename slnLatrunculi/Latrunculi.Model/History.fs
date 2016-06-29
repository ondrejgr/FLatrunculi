namespace Latrunculi.Model

module History =

    type T = HistoryItem.T list

    let getNumberOfMoves (history: T) =
        List.fold (fun result i -> result + 1) 0 history

    let getNumberOfMovesWithoutRemoval (history: T) =
        List.foldBack (fun (item: HistoryItem.T) result ->
                        if BoardMove.anyPiecesRemoved item.BoardMove then 0 else result + 1)
                    history 0

    let getHistoryWithNewItem (history: T) (item: HistoryItem.T) =
        let result: T = item::history
        result

    let create =
        let result: T = []
        result
