namespace Latrunculi.Model
open System.Runtime.Serialization

module GameFile =

    let tryCheckObject x name =
        match obj.ReferenceEquals(x, null) with
        | true -> Error (UnableToDeserializeObject name)
        | false -> Success ()

    module PlayerSettingsDto =
        [<CLIMutable>]
        [<DataContract>]
        type T = {
            [<DataMember>]
            Settings: string
            [<DataMember>]
            WhitePlayerName: string
            [<DataMember>]
            BlackPlayerName: string }

        let fromPlayerSettings (source: PlayerSettings.T) =
            let playerToDto (player: Player.T) =
                String.concat ""
                    <| seq {
                            yield match Player.getPlayerType player with
                                    | Player.Types.Computer -> "C"
                                    | Player.Types.Human -> "H"
                            yield match player.Level with
                                    | Player.Easy -> "1"
                                    | Player.Medium -> "2"
                                    | Player.Hard -> "3" }
            {   Settings = (String.concat ""
                                <| seq {
                                        yield playerToDto source.WhitePlayer
                                        yield playerToDto source.BlackPlayer });
                WhitePlayerName = source.WhitePlayer.Name;
                BlackPlayerName = source.BlackPlayer.Name }

        let tryToPlayerSettings (source: T) =
            let tryCheckSettingsLength x =
                match x with
                | s when String.length s = 4 -> Success s
                | _ -> Error (UnableToDeserializeObject "PlayerSettings")
            let tryCheckNameLength x =
                match x with
                | s when String.length s <= 12 -> Success s
                | _ -> Error (UnableToDeserializeObject "PlayerSettings.PlayerName")
            let tryDtoToPlayer (s: string) (name: string) (color: Piece.Colors) =
                let tryGetLevel = match s.[1] with
                                    | '1' -> Success Player.Easy
                                    | '2' -> Success Player.Medium
                                    | '3' -> Success Player.Hard
                                    | _ -> Error (UnableToDeserializeObject "PlayerSettings.Player.Level")
                let tryGetCreatePlayerFn = match s.[0] with
                                            | 'C' -> Success (Player.create Player.Types.Computer)
                                            | 'H' -> Success (Player.create Player.Types.Human)
                                            | _ -> Error (UnableToDeserializeObject "PlayerSettings.Player.Type")
                maybe {
                    let! level = tryGetLevel
                    let! createPlayerFn = tryGetCreatePlayerFn
                    return createPlayerFn name level color }

            maybe {
                let! whiteName = tryCheckNameLength source.WhitePlayerName
                let! blackName = tryCheckNameLength source.BlackPlayerName
                let! s = tryCheckSettingsLength source.Settings
                let! white = tryDtoToPlayer s.[0..1] whiteName Piece.Colors.White
                let! black = tryDtoToPlayer s.[2..3] blackName Piece.Colors.Black
                return! PlayerSettings.tryCreate white black }

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

