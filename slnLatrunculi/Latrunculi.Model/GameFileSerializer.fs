namespace Latrunculi.Model
open System.Xml
open System.Xml.Serialization

module GameFileSerializer =


    let TrySaveFile (fileName: string) (file: GameFile.T) =
        try
            let sett = XmlWriterSettings()
            sett.CloseOutput <- true
            sett.Indent <- true
            let ser = XmlSerializer(typeof<GameFile.T>)
            use xml = XmlWriter.Create(fileName, sett)
            ser.Serialize(xml, file)
            Success ()
        with
        | e -> Error (UnableToSaveGame e.Message) 

    let TryLoadFile (fileName: string) =
        try
            let tryCheckNotNull x msg =
                 Unchecked.defaultof<_>
            let sett = XmlReaderSettings()
            sett.CloseInput <- true
            let ser = XmlSerializer(typeof<GameFile.T>)
            use xml = XmlReader.Create(fileName, sett)
            let file = unbox<GameFile.T>(ser.Deserialize(xml))
            maybe {
                do! GameFile.tryCheck file
                return file
            }
        with
        | e -> Error (UnableToLoadGame e.Message) 
