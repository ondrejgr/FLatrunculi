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
    public class MoveHistoryCollection : ObservableCollection<MoveHistoryItem>
    {
        private void OnPropertyChanged(string propName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propName));
        }

        public void InsertItem(HistoryItem.T item)
        {
            PieceTypes pt = PieceTypes.ptNone;
            if (item.PlayerColor == Piece.Colors.Black)
                pt = PieceTypes.ptBlack;
            else if (item.PlayerColor == Piece.Colors.White)
                pt = PieceTypes.ptWhite;
            
            Insert(0, new MoveHistoryItem(this.Count + 1, pt,
                     Coord.toString(item.Move.Source), Coord.toString(item.Move.Target),
                     item.RemovedPiecesCount));
        }
    }
}
