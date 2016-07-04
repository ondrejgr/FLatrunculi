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
        
        public void InsertMove(int moveNumber, BoardMove.T item)
        {
            PieceTypes pt = PieceTypes.ptNone;
            if (item.Color.Equals(Piece.Colors.Black))
                pt = PieceTypes.ptBlack;
            else if (item.Color.Equals(Piece.Colors.White))
                pt = PieceTypes.ptWhite;

            Insert(0, new MoveHistoryItem(moveNumber, pt,
                     Coord.toString(BoardMove.getSourceCoord(item)), 
                     Coord.toString(BoardMove.getTargetCoord(item)),
                     BoardMove.getRemovedPiecesCount(item)));
        }

        public void RemoveMove(int index)
        {
            RemoveAt(index);
        }
    }
}
