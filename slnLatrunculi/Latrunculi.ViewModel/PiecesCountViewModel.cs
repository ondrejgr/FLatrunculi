using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class PiecesCountViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        private int _whitePiecesCount;
        public int WhitePiecesCount
        {
            get
            {
                return _whitePiecesCount;
            }
            private set
            {
                _whitePiecesCount = value;
                OnUpdate();
            }
        }

        public bool IsWhiteWinning
        {
            get
            {
                return WhitePiecesCount > BlackPiecesCount;
            }
        }

        public bool IsWhiteLosing
        {
            get
            {
                return IsBlackWinning;
            }
        }


        private int _blackPiecesCount;
        public int BlackPiecesCount
        {
            get
            {
                return _blackPiecesCount;
            }
            private set
            {
                _blackPiecesCount = value;
                OnUpdate();
            }
        }

        public bool IsBlackLosing
        {
            get
            {
                return IsWhiteWinning;
            }
        }

        public bool IsBlackWinning
        {
            get
            {
                return BlackPiecesCount > WhitePiecesCount;
            }
        }

        private void OnUpdate()
        {
            OnPropertyChanged("WhitePiecesCount");
            OnPropertyChanged("BlackPiecesCount");
            OnPropertyChanged("IsWhiteWinning");
            OnPropertyChanged("IsWhiteLosing");
            OnPropertyChanged("IsBlackWinning");
            OnPropertyChanged("IsBlackLosing");
        }

        public void Update(int whitePieces, int blackPieces)
        {
            WhitePiecesCount = whitePieces;
            BlackPiecesCount = blackPieces;
        }
    }
}
