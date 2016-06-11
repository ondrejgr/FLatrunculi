namespace Latrunculi.Model

[<StructuralEquality;NoComparison>]
type ActiveColor =
    | Black
    | White


module Rules =

    let getInitialBoardSquares (coord: Coord.T) =
        match coord with
        | { Row = Coord.RowNumber 7 } | { Row = Coord.RowNumber 6 } -> Square.createWithPiece <| Piece.createBlack
        | { Row = Coord.RowNumber 2 } | { Row = Coord.RowNumber 1 } -> Square.createWithPiece <| Piece.createWhite
        | _ -> Square.createEmpty

    let getInitialActiveColor =
        White

    
//    let getValidMovesSeq (board: Board.T) (color: ActiveColor) =
//        seq {
//            for coord in Coord.getCoordsSeq do
//                () }

//    let isMoveValid (move: Result<Move.T, Move.Error>) (color: ActiveColor) =
                        

//
//        protected override void OnGetValidMoves(Moves moves, GameColorsEnum color)
//        {
//            Pieces tarPiece = (color == GameColorsEnum.plrBlack) ? Pieces.pcBlack : Pieces.pcWhite;
//
//            Coord src = new Coord();
//
//            for (char x = 'A'; x <= Board.MaxX; x++)
//            {
//                for (byte y = 1; y <= Board.MaxY; y++)
//                {
//                    src.Set(x, y);
//                    if (((color == GameColorsEnum.plrBlack) &&
//                        (Board[src] == Pieces.pcBlack || Board[src] == Pieces.pcBlackKing)) ||
//                        ((color == GameColorsEnum.plrWhite) &&
//                        (Board[src] == Pieces.pcWhite || Board[src] == Pieces.pcWhiteKing)))
//                    {
//                        // barva figurky na danem miste patri hraci provadejicimu tah,
//                        // muzeme overit mozne tahy v ortogonalich smerech.
//                        // Pokud tam neni figurka a pokud to neni mimo desku, je tah dovoleny.
//                        Action<CoordDirectionEnum> addMoveIfValid = new Action<CoordDirectionEnum>((dir) =>
//                            {
//                                Coord? tar;
//                                tar = Board.GetRelativeCoord(src, dir);
//                                if (tar.HasValue && (Board[tar.Value] == Pieces.pcNone))
//                                    moves.Add(new Move(src, tar.Value, Pieces.pcNone, tarPiece));
//                            }
//                        );
//
//                        addMoveIfValid(CoordDirectionEnum.deForward);
//                        addMoveIfValid(CoordDirectionEnum.deAft);
//                        addMoveIfValid(CoordDirectionEnum.deLeft);
//                        addMoveIfValid(CoordDirectionEnum.deRight);
//                    }
//                }
//            }
//        }