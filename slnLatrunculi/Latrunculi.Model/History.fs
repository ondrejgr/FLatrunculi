namespace Latrunculi.Model

module History =

    type T = HistoryItem.T list

    let getHistoryWithNewItem (history: T) (item: HistoryItem.T) =
        let result: T = item::history
        result

    let create() =
        let result: T = []
        result
