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
            set
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
            set
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
    }
}
