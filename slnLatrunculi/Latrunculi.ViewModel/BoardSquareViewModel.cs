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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            private set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public void SetIsSelected(bool value)
        {
            IsSelected = value;
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
                if (p.Item.Color.Equals(Model.Piece.Colors.Black))
                    PieceType = PieceTypes.ptBlack;
                else if (p.Item.Color.Equals(Model.Piece.Colors.White))
                    PieceType = PieceTypes.ptWhite;
                else
                    throw new NotSupportedException();
            }
            else
                PieceType = PieceTypes.ptNone;
        }
    }
}
