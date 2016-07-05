namespace Latrunculi.Controller
open Latrunculi.Model
open System.Threading

[<StructuralEquality;NoComparison>]
type GameLoopCycleResult =
    | Continue
    | Finished

[<StructuralEquality;NoComparison>]
type GameLoopStatus =
    | WaitingForHumanPlayerCoords

module GameController =

    type T(model: GameModel.T) = 

        member val private humanSelectedMove: HumanSelectedMove.T option = None with get, set
        member val private cts: CancellationTokenSource option = None with get, set
        member val private ctsMoveSuggestion: CancellationTokenSource option = None with get, set
        member private this.Model = model
    
        member this.changePlayerSettingsFromPlayers (white: Player.T) (black: Player.T) =
            let playerSettings = unwrapResultExn <| PlayerSettings.tryCreate white black
            this.Model.changePlayerSettings playerSettings

        member this.changePlayerSettings (types: Player.PlayerTypes) (names: Player.PlayerNames) (levels: Player.PlayerLevels) =
            let white = Player.create (fst types) (fst names) (fst levels) Piece.Colors.White
            let black = Player.create (snd types) (snd names) (snd levels) Piece.Colors.Black
            this.changePlayerSettingsFromPlayers white black

        member private this.GetHumanMoveFromUI() =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused) |> ignore)

                let request = HumanSelectedMove.create()
                this.humanSelectedMove <- Some request

                this.Model.setStatus(GameStatus.WaitingForHumanPlayerMove) |> ignore
                let! move = Async.AwaitEvent(request.HumanMoveSelected)
                this.Model.setStatus(GameStatus.Running) |> ignore
                return move.Move }

        member this.TrySetSelectedMove move =
            match this.humanSelectedMove with
            | Some hsm -> 
                hsm.SetMove move
                Success move
            | _ -> Error HumanSelectedMoveRequestDoesNotExists

        member this.TryNewGame() =
            maybe {
                Player.Board <- Some this.Model.Board
                Player.getHumanPlayerMoveFromUIWorkflow <- Some this.GetHumanMoveFromUI

                // reset game result and set active player
                this.Model.setActiveColor(Some Rules.getInitialActiveColor) |> ignore
                this.Model.setResult Rules.NoResult |> ignore

                // init board with default positions
                let! board = Board.tryInit this.Model.Board Rules.getInitialBoardSquares
                this.Model.RaiseBoardChanged()
                this.Model.RaiseHistoryChanged()
                return this }
            
        member private this.ReportGameError e =
            this.Model.setStatus(GameStatus.Paused) |> ignore
            this.Model.RaiseGameErrorEvent(e)

        member private this.GameLoopCycle(): Async<Result<GameLoopCycleResult, Error>> =
            let getPlayerMoveWorkflow =
                maybe {
                    let! player = this.Model.tryGetActivePlayer()
                    match player with
                    | :? Player.ComputerPlayer ->
                        this.Model.RaiseComputerPlayerThinking()
                        return player.TryGetMove
                    | _ ->
                        return player.TryGetMove }    

            let tryApplyBoardMoveAndChangePlayer (boardMove: BoardMove.T) =
                maybe {
                    // apply PLAYER move
                    Board.move this.Model.Board boardMove
                    this.Model.pushMoveToHistoryAndClearRedoStack boardMove

                    // update board
                    this.Model.RaiseBoardChanged()

                    // check game over
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    do! this.Model.trySwapActiveColor()
                    match this.Model.Result with
                    | Rules.GameOverResult _ ->
                        return Finished
                    | Rules.NoResult ->
                        return Continue }

            let tryApplyMoveAndChangePlayer move =
                maybe {
                    // create move with removed pieces list
                    let! color = this.Model.tryGetActiveColor()
                    let! boardMove = Rules.tryValidateAndGetBoardMove this.Model.Board color move
                    return! tryApplyBoardMoveAndChangePlayer boardMove }

            async {
                match getPlayerMoveWorkflow with
                | Success getPlayerMove ->
                    // get player move
                    let! moveResult = getPlayerMove()
                    match moveResult with
                    | Success move ->
                        // apply move
                        return tryApplyMoveAndChangePlayer move
                    | Error e ->
                        return Error e
                | Error e ->
                    return Error e }

        member private this.GameLoop(): Async<unit> =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setStatus(GameStatus.Paused) |> ignore)

                let! a = this.GameLoopCycle()
                match a with
                | Success result ->
                    match result with
                    | Continue -> 
                        return! this.GameLoop()
                    | Finished -> 
                        this.Model.setStatus(GameStatus.Finished) |> ignore
                        return ()
                | Error e ->
                    this.ReportGameError e
                    return () }

        member private this.TryGetCts() =
            match this.cts with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.cts then 
                this.cts.Value.Dispose()
            this.cts <- Some (new CancellationTokenSource())
            Success (Option.get this.cts)

        member private this.TryGetMoveSuggestionCts() =
            match this.ctsMoveSuggestion with
            | Some cts -> Success cts
            | None -> Error CancellationTokenDoesNotExist

        member private this.TryCreateMoveSuggestionCts(): Result<CancellationTokenSource, Error> =
            if Option.isSome this.ctsMoveSuggestion then
                this.ctsMoveSuggestion.Value.Dispose()
            this.ctsMoveSuggestion <- Some (new CancellationTokenSource())
            Success (Option.get this.ctsMoveSuggestion)

        member this.TryCancelSuggestMove() =
            maybe {
                let! cts = this.TryGetMoveSuggestionCts()
                cts.Cancel()
                return this }

        member private this.SuggestMove color level =
            async {
                use! cnl = Async.OnCancel(fun () -> this.Model.setIsMoveSuggestionComputing false |> ignore)
                let! move = Brain.tryGetBestMove this.Model.Board color <| Player.levelToDepth level
                this.Model.setIsMoveSuggestionComputing false |> ignore
                this.Model.RaiseMoveSuggestionComputed(move)
                () }

        member this.TrySuggestMove() =
            let checkGameStatus =
                if this.Model.IsMoveSuggestionComputing 
                    then Error MoveSuggestionAlreadyComputing 
                    else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateMoveSuggestionCts()
                let! color = this.Model.tryGetActiveColor()
                let! activePlayer = this.Model.tryGetActivePlayer()
                this.Model.setIsMoveSuggestionComputing true |> ignore
                Async.Start(this.SuggestMove color activePlayer.Level, cts.Token)
                return this }

        member this.TryPause() =
            maybe {
                do! (match this.Model.IsMoveSuggestionComputing with
                    | true -> Success (ignore <| this.TryCancelSuggestMove())
                    | false -> Success ())
                match this.Model.Status with
                | GameStatus.Running | GameStatus.WaitingForHumanPlayerMove ->
                    let! cts = this.TryGetCts()
                    cts.Cancel()
                    this.Model.setStatus(GameStatus.Paused) |> ignore
                    return this
                | _ ->
                    this.Model.setStatus(GameStatus.Paused) |> ignore
                    return this }

        member this.TryResume() =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Running then Error GameIsAlreadyRunning else Success ()
            maybe {
                do! checkGameStatus
                return! this.TryRun() }

        member this.TryRun() =
            let checkGameStatus =
                if Seq.contains this.Model.Status 
                    <| seq { 
                            yield GameStatus.Running 
                            yield GameStatus.WaitingForHumanPlayerMove } 
                    then Error GameIsAlreadyRunning 
                    else Success ()
            maybe {
                do! checkGameStatus
                let! cts = this.TryCreateCts()
                this.Model.setStatus(GameStatus.Running) |> ignore
                Async.Start(this.GameLoop(), cts.Token)
                return this }

        member this.TryMoveExistsForCoord(coord): Result<bool, Error> =
            let tryGetValidMovesForCoord color =
                let move = Seq.tryHead <| Rules.getValidMovesForCoord this.Model.Board color coord
                match move with
                | Some _ -> true
                | None _ -> false
            maybe {
                let! color = this.Model.tryGetActiveColor()
                let result = tryGetValidMovesForCoord color
                return result }

        member this.TryGetValidMove (src: Coord.T) (tar: Coord.T): Result<Move.T, Error> =
            maybe {
                let! color = this.Model.tryGetActiveColor()
                let! move = match Seq.tryFind (fun (m: Move.T) -> m.Source = src && m.Target = tar) 
                                        <| Rules.getValidMovesForCoord this.Model.Board color src with
                            | Some m -> Success m
                            | _ -> Error NoValidMoveExists
                return move }

        member this.GetPossibleTargetCoords (src: Coord.T) =
            match this.Model.tryGetActiveColor() with
            | Success color ->
                        System.Collections.Generic.List<Coord.T>(Seq.map (fun (coord: Move.T) -> coord.Target) 
                                <| Rules.getValidMovesForCoord this.Model.Board color src)
            | _ -> System.Collections.Generic.List<Coord.T>(List.empty)

        member this.TrySaveGame (fileName: string) =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Running then Error GameIsRunning else Success ()
            maybe {
                do! checkGameStatus
                let file = GameFile.create model
                do! GameFileSerializer.TrySaveFile fileName file
                return this }

        member this.TryApplyMovesLoadedFromFile (moves: Move.T list) =
            Seq.foldBack
                (fun (move: Move.T) result ->
                    match result with 
                    | Success Rules.GameResult.NoResult ->
                        let newResult =
                            maybe {
                                // validate and create board move
                                let! color = this.Model.tryGetActiveColor()
                                let! boardMove = Rules.tryValidateAndGetBoardMove this.Model.Board color move
                                // apply move and add it to history
                                Board.move this.Model.Board boardMove
                                this.Model.Board.History.PushMoveToUndoStack boardMove
                                // get result
                                this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                                do! this.Model.trySwapActiveColor()
                                return this.Model.Result }              
                        newResult
                    | Success s -> Success s      
                    | Error e -> Error e) 
                moves (Success Rules.GameResult.NoResult)

        member this.TryLoadGame (fileName: string) =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Running then Error GameIsRunning else Success ()
            match maybe {
                        do! checkGameStatus
                        this.Model.setStatus GameStatus.Paused |> ignore
                        let! controller = this.TryNewGame()

                        // load file
                        let! file = GameFileSerializer.TryLoadFile fileName
                        // change player settings
                        let! newPlayerSettings = PlayerSettingsDto.tryToPlayerSettings file.PlayerSettings
                        this.Model.changePlayerSettings newPlayerSettings |> ignore

                        // load game history
                        let! newHistory = HistoryDto.tryToHistory file.History

                        // apply loaded moves
                        let moves = MoveStack.map (fun item -> item.Move) newHistory.UndoStack
                        let! gameResult = this.TryApplyMovesLoadedFromFile moves

                        // set redostack
                        this.Model.Board.History.RedoStack <- newHistory.RedoStack

                        // render board
                        this.Model.RaiseBoardChanged()
                        this.Model.RaiseHistoryChanged()

                        match this.Model.Result with
                        | Rules.GameOverResult _ ->
                            this.Model.setStatus(GameStatus.Finished) |> ignore
                            return this
                        | _ -> 
                            return this } with
            | Success s -> Success s
            | Error e -> Error (ErrorLoadingFile e)


         member this.TryGoToMove (id: int) =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Created 
                    then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! pausedController = this.TryPause()
                match id with
                | 0 ->
                    return this
                | _ ->
                    let boardMoves = this.Model.Board.History.takeBoardMoves id
                    let redoBoardMoves = this.Model.Board.History.skipBoardMoves id
                    let! newGameController = this.TryNewGame()
                    // apply moves (including rule checking)
                    do! List.fold (fun result (item: BoardMove.T) ->
                                    match result with
                                    | Success _ ->
                                        maybe {
                                            let! color = this.Model.tryGetActiveColor()
                                            Board.move this.Model.Board item
                                            this.Model.Board.History.PushMoveToUndoStack item
                                            do! this.Model.trySwapActiveColor()
                                            return ()}   
                                    | Error e -> Error e) (Success ()) boardMoves

                    // recreate redo stack
                    List.iter (fun (item: BoardMove.T) ->
                            this.Model.Board.History.PushMoveToRedoStack item) redoBoardMoves

                    // refresh board
                    this.Model.RaiseBoardChanged()
                    this.Model.RaiseHistoryChanged()
                    this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                    match this.Model.Result with
                    | Rules.GameOverResult _ ->
                        this.Model.setStatus(GameStatus.Finished) |> ignore
                        return this
                    | _ -> 
                        return this }

        member this.TryUndo() =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Created 
                    then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! pausedController = this.TryPause()
                do! this.Model.trySwapActiveColor()

                // pop move from redo stack
                let! originalBoardMove = this.Model.Board.History.tryPopMoveFromUndoStack()
                // push move to undo stack
                this.Model.Board.History.PushMoveToRedoStack originalBoardMove

                // apply move
                Board.invmove this.Model.Board originalBoardMove

                // refresh board
                this.Model.RaiseBoardChanged()
                this.Model.RaiseHistoryChanged()

                // check game over
                this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                match this.Model.Result with
                | Rules.GameOverResult _ ->
                    this.Model.setStatus(GameStatus.Finished) |> ignore
                    return this
                | _ -> 
                    return this }

        member this.TryRedo() =
            let checkGameStatus =
                if this.Model.Status = GameStatus.Created 
                    then Error GameIsNotRunning else Success ()
            maybe {
                do! checkGameStatus
                let! pausedController = this.TryPause()

                // pop move from redo stack
                let! color = this.Model.tryGetActiveColor()
                let! originalBoardMove = this.Model.Board.History.tryPopMoveFromRedoStack()
                // push move to undo stack
                this.Model.Board.History.PushMoveToUndoStack originalBoardMove

                // apply move
                let! boardMove = Rules.tryValidateAndGetBoardMove this.Model.Board color originalBoardMove.Move
                Board.move this.Model.Board boardMove

                // refresh board
                this.Model.RaiseBoardChanged()
                this.Model.RaiseHistoryChanged()

                do! this.Model.trySwapActiveColor()
                // check game over
                this.Model.setResult <| (Rules.checkVictory this.Model.Board) |> ignore
                match this.Model.Result with
                | Rules.GameOverResult _ ->
                    this.Model.setStatus(GameStatus.Finished) |> ignore
                    return this
                | _ -> 
                    return this }
            

        member this.tryPopMoveNumberFromUndoStack() =
            snd <| MoveStack.foldBack (fun item result ->
                                         let id = 1 + fst result
                                         (id, Success id)) 
                                    this.Model.Board.History.UndoStack (0, Error StackIsEmpty)

    let create (model: GameModel.T) =
        T(model)
