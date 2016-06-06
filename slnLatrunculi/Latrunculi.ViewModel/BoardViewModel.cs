using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Latrunculi.ViewModel
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BoardRowViewModel> _rows = new ObservableCollection<BoardRowViewModel>();
        public ObservableCollection<BoardRowViewModel> Rows
        {
            get
            {
                return _rows;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public int NumberOfRows
        {
            get
            {
                return Rows.Count;
            }
        }
        public int NumberOfCols
        {
            get
            {
                if (Rows.Count == 0)
                    return 0;
                else
                    return Rows[0].Squares.Count;
            }
        }

        private void OnNumberOfRowsOrColsChanged()
        {
            OnPropertyChanged("NumberOfRows");
            OnPropertyChanged("NumberOfCols");
        }

        public void Clear()
        {
            foreach (BoardRowViewModel row in Rows)
            {
                row.Squares.Clear();
            }
            Rows.Clear();
            OnNumberOfRowsOrColsChanged();
        }
                
        public void Init(Latrunculi.Model.Board.T boardModel)
        {
            try
            {
                Func<Color, Color> swapColor = new Func<Color, Color>(c => {
                    return (c == Colors.Black) ? Colors.White : Colors.Black; });

                Clear();

                System.Windows.Media.Color color = System.Windows.Media.Colors.Black;
                foreach (Latrunculi.Model.Square.T[] rows in boardModel.Squares)
                {
                    BoardRowViewModel row = new BoardRowViewModel();
                    Rows.Add(row);

                    foreach (Latrunculi.Model.Square.T m in rows)
                    {
                        BoardSquareViewModel sq = new BoardSquareViewModel();
                        sq.Color = color;

                        if (m.IsPiece)
                        {
                            Latrunculi.Model.Square.T.Piece p = (Latrunculi.Model.Square.T.Piece)m;
                            switch (p.Item.Color)
                            {
                                case Model.Piece.Colors.Black:
                                    sq.PieceType = PieceType.ptBlack;
                                    break;
                                case Model.Piece.Colors.White:
                                    sq.PieceType = PieceType.ptWhite;
                                    break;
                            }
                        }
                        else
                            sq.PieceType = PieceType.ptNone;

                        row.Squares.Add(sq);
                        color = swapColor(color);
                    }
                    color = swapColor(color);
                }
            }
            finally
            {
                OnNumberOfRowsOrColsChanged();
            }
        }
    }
}
