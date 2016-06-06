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

    public class BoardSquareViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
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
