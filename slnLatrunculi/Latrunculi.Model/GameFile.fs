namespace Latrunculi.Model
open System.Runtime.Serialization

module GameFile =

    let tryCheckObject x name =
        match obj.ReferenceEquals(x, null) with
        | true -> Error (UnableToDeserializeObject name)
        | false -> Success ()

    [<CLIMutable>]
    [<DataContract(Namespace="")>]
    type T = {
        [<DataMember>]
        PlayerSettings: PlayerSettingsDto.T
        [<DataMember>]
        History: HistoryDto.T }

    let tryCheck (x: T) =
        maybe {
            do! tryCheckObject x "GameFile"
            do! tryCheckObject x.PlayerSettings "GameFile.PlayerSettings"
            do! tryCheckObject x.History "GameFile.History"
            do! tryCheckObject x.History.UndoStack "GameFile.History.UndoStack"
            do! tryCheckObject x.History.RedoStack "GameFile.History.RedoStack"
            return () }

    let create (model: GameModel.T) =
        let result = 
            {   PlayerSettings = PlayerSettingsDto.fromPlayerSettings model.PlayerSettings
                History = HistoryDto.fromHistory model.Board.History }
        result

