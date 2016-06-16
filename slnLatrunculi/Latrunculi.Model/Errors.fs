[<AutoOpen>]
module ErrorDefinitions

type Errors =
    | InvalidColumnNumber
    | InvalidRowNumber
    | ColumnOutOfRange
    | RowOutOfRange
    | InvalidSourceCoord
    | UnableToParseCoordFromString    