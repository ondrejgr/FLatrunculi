namespace Latrunculi.Model
open System.Runtime.Serialization
open System.Xml

module GameFileSerializer =


    let TrySaveFile (fileName: string) (file: GameFile.T) =
        try
            let sett = XmlWriterSettings()
            sett.CloseOutput <- true
            sett.Indent <- true
            let ser = DataContractSerializer(typeof<GameFile.T>)
            use xml = XmlWriter.Create(fileName, sett)
            ser.WriteObject(xml, file)
            Success ()
        with
        | e -> Error (UnableToSaveGame e.Message) 

    let TryLoadFile (fileName: string) =
        try
            let tryCheckNotNull x msg =
                 Unchecked.defaultof<_>
            let sett = XmlReaderSettings()
            sett.CloseInput <- true
            let ser = DataContractSerializer(typeof<GameFile.T>)
            use xml = XmlReader.Create(fileName, sett)
            let file = unbox<GameFile.T>(ser.ReadObject(xml))
            maybe {
                do! GameFile.tryCheck file
                return file
            }
        with
        | e -> Error (UnableToLoadGame e.Message) 
