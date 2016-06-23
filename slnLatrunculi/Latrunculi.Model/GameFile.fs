namespace Latrunculi.Model
open System.Xml.Serialization

module GameFile =

    let tryCheckObject x name =
        match obj.ReferenceEquals(x, null) with
        | true -> Error (UnableToDeserializeObject name)
        | false -> Success ()

    module GameSettings =
        module Player =
            [<CLIMutable>] 
            [<XmlType("Player")>]
            type T = {
                Type: Player.Types;
                Name: Player.Name;
                Level: Player.Levels }

            let tryCheck (x: T) =
                maybe {
                    do! tryCheckObject x "Player"
                    do! tryCheckObject x.Type "Player.Type"
                    do! tryCheckObject x.Name "Player.Name"
                    do! tryCheckObject x.Level "Player.Level"
                    return () }

            let createFromPlayer (player: Latrunculi.Model.Player.T) =
                {   Type = Player.getPlayerType player;
                    Name = player.Name;
                    Level= player.Level }
            

        [<CLIMutable>] 
        [<XmlType("GameSettings")>]
        type T = {
            WhitePlayer: Player.T;
            BlackPlayer: Player.T }

        let createFromPlayerSettings (playerSettings: PlayerSettings.T) =
            let white = Player.createFromPlayer playerSettings.WhitePlayer
            let black = Player.createFromPlayer playerSettings.BlackPlayer
            {   WhitePlayer = white;
                BlackPlayer = black }

    [<CLIMutable>] 
    [<XmlRoot("GameFile")>]
    type T = {
        GameSettings: GameSettings.T }

    let tryCheck (x: T) =
        maybe {
            do! tryCheckObject x "GameFile"
            do! tryCheckObject x.GameSettings "GameSettings"
            do! tryCheckObject x.GameSettings.WhitePlayer "GameSettings.WhitePlayer"
            do! tryCheckObject x.GameSettings.BlackPlayer "GameSettings.BlackPlayer"
            do! GameSettings.Player.tryCheck x.GameSettings.WhitePlayer
            do! GameSettings.Player.tryCheck x.GameSettings.BlackPlayer
            return () }

    let create (playerSettings: PlayerSettings.T) =
        let result = 
            {   GameSettings = GameSettings.createFromPlayerSettings playerSettings }
        result

