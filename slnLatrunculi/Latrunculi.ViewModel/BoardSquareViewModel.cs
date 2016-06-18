using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Latrunculi.ViewModel
{
    public enum PieceTypes { ptNone, ptWhite, ptBlack };
    public enum SquareColors { scWhite, scBlack }

    public class BoardSquareViewModel : BoardSquareBaseViewModel
    {
        private Model.Coord.T _coord;
        public Model.Coord.T Coord
        {
            get
            {
                return _coord;
            }
            private set
            {
                _coord = value;
                OnPropertyChanged("Coord");
            }
        }

        private SquareColors _squareColor = SquareColors.scWhite;
        public SquareColors SquareColor
        {
            get
            {
                return _squareColor;
            }
            private set
            {
                _squareColor = value;
                OnPropertyChanged("SquareColor");
            }
        }

        private PieceTypes _pieceType = PieceTypes.ptNone;
        public PieceTypes PieceType
        {
            get
            {
                return _pieceType;
            }
            set
            {
                _pieceType = value;
                OnPropertyChanged("PieceType");
            }
        }

        private bool _isSuggestedMove = false;
        public bool IsSuggestedMove
        {
            get
            {
                return _isSuggestedMove;
            }
            private set
            {
                _isSuggestedMove = value;
                OnPropertyChanged("IsSuggestedMove");
            }
        }

        public void SetIsSuggestedMove(bool value)
        {
            IsSuggestedMove = value;
        }

        private bool _validMoveExists = false;
        public bool ValidMoveExists
        {
            get
            {
                return _validMoveExists;
            }
            private set
            {
                _validMoveExists = value;
                OnPropertyChanged("ValidMoveExists");
            }
        }

        public void SetValidMoveExists(bool value)
        {
            ValidMoveExists = value;
        }

        public void Init(Model.Coord.T coord, SquareColors sqColor)
        {
            Coord = coord;
            SquareColor = sqColor;
        }

        public void RefreshFromModel(Latrunculi.Model.Square.T model)
        {
            if (model.IsPiece)
            {
                Latrunculi.Model.Square.T.Piece p = (Latrunculi.Model.Square.T.Piece)model;
                switch (p.Item.Color)
                {
                    case Model.Piece.Colors.Black:
                        PieceType = PieceTypes.ptBlack;
                        break;
                    case Model.Piece.Colors.White:
                        PieceType = PieceTypes.ptWhite;
                        break;
                }
            }
            else
                PieceType = PieceTypes.ptNone;
        }
    }
}
