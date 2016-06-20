using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class MoveHistoryItem: INotifyPropertyChanged
    {
        public MoveHistoryItem(int id, PieceTypes pieceType, string source, string target, int rpCount)
        {
            ID = id;
            PieceType = pieceType;
            Source = source;
            Target = target;
            RemovedPiecesCount = rpCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private int _id;
        public int ID
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }        

        private PieceTypes _pieceType;
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

        private string _source;
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }

        private string _target;
        public string Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
                OnPropertyChanged("Target");
            }
        }

        private int _removedPiecesCount = 0;
        public int RemovedPiecesCount
        {
            get
            {
                return _removedPiecesCount;
            }
            set
            {
                _removedPiecesCount = value;
                OnPropertyChanged("RemovedPiecesCount");
                OnPropertyChanged("RemovedPiecesCountStr");
            }
        }

        public string RemovedPiecesCountStr
        {
            get
            {
                if (RemovedPiecesCount > 0)
                    return string.Format("({0})", RemovedPiecesCount);
                else
                    return string.Empty;
            }
        }
    }
}
