namespace Latrunculi.Model
open System.Runtime.Serialization

module GameFile =

    let tryCheckObject x name =
        match obj.ReferenceEquals(x, null) with
        | true -> Error (UnableToDeserializeObject name)
        | false -> Success ()

    module GameSettings =
        module Player =
            [<CLIMutable>]
            [<DataContract>]
            type T = {
                [<DataMember>]
                Type: Player.Types;
                [<DataMember>]
                Name: Player.Name;
                [<DataMember>]
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
        [<DataContract>]
        type T = {
            [<DataMember>]
            WhitePlayer: Player.T;
            [<DataMember>]
            BlackPlayer: Player.T }

        let createFromPlayerSettings (playerSettings: PlayerSettings.T) =
            let white = Player.createFromPlayer playerSettings.WhitePlayer
            let black = Player.createFromPlayer playerSettings.BlackPlayer
            {   WhitePlayer = white;
                BlackPlayer = black }

    module GameMovesArray =
        [<CLIMutable>]
        [<DataContract>]
        type T = Move.T array

        let createFromHistory (history: History.T) =
            let result: T = List.toArray <| (List.map (fun (item: HistoryItem.T) -> item.BoardMove.Move) 
                                                    <| List.rev history)
            result


    [<CLIMutable>]
    [<DataContract>]
    type T = {
        [<DataMember>]
        GameSettings: GameSettings.T
        [<DataMember>]
        GameMoves: GameMovesArray.T }

    let tryCheck (x: T) =
        maybe {
            do! tryCheckObject x "GameFile"
            do! tryCheckObject x.GameSettings "GameSettings"
            do! tryCheckObject x.GameSettings.WhitePlayer "GameSettings.WhitePlayer"
            do! tryCheckObject x.GameSettings.BlackPlayer "GameSettings.BlackPlayer"
            do! GameSettings.Player.tryCheck x.GameSettings.WhitePlayer
            do! GameSettings.Player.tryCheck x.GameSettings.BlackPlayer
            do! tryCheckObject x.GameMoves "GameMoves"
            return () }

    let create (model: GameModel.T) =
        let result = 
            {   GameSettings = GameSettings.createFromPlayerSettings model.PlayerSettings;
                GameMoves = GameMovesArray.createFromHistory model.Board.History }
        result

