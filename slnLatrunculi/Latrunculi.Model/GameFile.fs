namespace Latrunculi.Model
open System.Runtime.Serialization

module GameFile =

    let tryCheckObject x name =
        match obj.ReferenceEquals(x, null) with
        | true -> Error (UnableToDeserializeObject name)
        | false -> Success ()

    [<CLIMutable>]
    [<DataContract>]
    type T = {
        [<DataMember>]
        PlayerSettings: PlayerSettingsDto.T }

    let tryCheck (x: T) =
        maybe {
            do! tryCheckObject x "GameFile"
//            do! tryCheckObject x.PlayerSettings "PlayerSettings"
//            do! tryCheckObject x.GameSettings.WhitePlayer "GameSettings.WhitePlayer"
//            do! tryCheckObject x.GameSettings.BlackPlayer "GameSettings.BlackPlayer"
//            do! GameSettings.Player.tryCheck x.GameSettings.WhitePlayer
//            do! GameSettings.Player.tryCheck x.GameSettings.BlackPlayer
//            do! tryCheckObject x.GameMoves "GameMoves"
            return () }

    let create (model: GameModel.T) =
        let result = 
            {   PlayerSettings = PlayerSettingsDto.fromPlayerSettings model.PlayerSettings }
        result

