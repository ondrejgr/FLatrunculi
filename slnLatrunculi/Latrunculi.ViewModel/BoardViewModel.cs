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
                
        private void ApplySquareModelToViewModel(Latrunculi.Model.Square.T model, BoardSquareViewModel viewModel, SquareColors? sqColor = null)
        {
            if (sqColor.HasValue)
                viewModel.SquareColor = sqColor.Value;

            if (model.IsPiece)
            {
                Latrunculi.Model.Square.T.Piece p = (Latrunculi.Model.Square.T.Piece)model;
                switch (p.Item.Color)
                {
                    case Model.Piece.Colors.Black:
                        viewModel.PieceType = PieceTypes.ptBlack;
                        break;
                    case Model.Piece.Colors.White:
                        viewModel.PieceType = PieceTypes.ptWhite;
                        break;
                }
            }
            else
                viewModel.PieceType = PieceTypes.ptNone;
        }

        public void Init(Latrunculi.Model.Board.T boardModel)
        {
            try
            {
                Clear();

                Func<SquareColors, SquareColors> swapColor = new Func<SquareColors, SquareColors>(c => {
                    return (c == SquareColors.scBlack) ? SquareColors.scWhite : SquareColors.scBlack; });
                SquareColors color = SquareColors.scBlack;

                foreach (int rowNumber in boardModel.GetRowNumbers)
                {
                    BoardRowViewModel row = new BoardRowViewModel();
                    Rows.Add(row);

                    foreach (Tuple<Model.Coord.T, Model.Square.T> t in boardModel.GetCoordAndSquaresByRowNumber(rowNumber))
                    {
                        BoardSquareViewModel sq = new BoardSquareViewModel();
                        sq.Coord = t.Item1;

                        ApplySquareModelToViewModel(t.Item2, sq, color);

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
