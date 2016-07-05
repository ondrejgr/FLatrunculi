using Latrunculi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class MoveHistoryCollection : Collection<MoveHistoryItem>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public void ClearAndAddHistoryItems(IEnumerable<HistoryItem.T> items)
        {
            Clear();
            foreach (HistoryItem.T item in items)
            {
                BoardMove.T move = item.Move;
                PieceTypes pt = PieceTypes.ptNone;
                if (move.Color.Equals(Piece.Colors.Black))
                    pt = PieceTypes.ptBlack;
                else if (move.Color.Equals(Piece.Colors.White))
                    pt = PieceTypes.ptWhite;

                MoveHistoryItem vm = new MoveHistoryItem(item.ID, pt,
                     Coord.toString(BoardMove.getSourceCoord(move)),
                     Coord.toString(BoardMove.getTargetCoord(move)),
                     BoardMove.getRemovedPiecesCount(move));

                Add(vm);
            }

            OnPropertyChanged("Count");
            OnCollectionChanged();
        }

        private void OnCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
