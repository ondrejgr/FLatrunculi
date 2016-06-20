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

        private PiecesCountViewModel _piecesCount = new PiecesCountViewModel();
        public PiecesCountViewModel PiecesCount
        {
            get
            {
                return _piecesCount;
            }
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

        public void ClearIsSuggestedMove()
        {
            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares.OfType<BoardSquareViewModel>())
                {
                    sqVM.SetIsSuggestedMove(false);
                }
            }
        }

        public void SetIsSuggestedMove(Model.Coord.T coord)
        {
            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares
                        .OfType<BoardSquareViewModel>()
                        .Where(sq => sq.Coord.Equals(coord)))
                {
                    sqVM.SetIsSuggestedMove(true);
                }
            }
        }

        public void ClearIndications()
        {
            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares.OfType<BoardSquareViewModel>())
                {
                    sqVM.SetIsSuggestedMove(false);
                }
            }
        }

        public void ClearIsSelected()
        {
            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares.OfType<BoardSquareViewModel>())
                {
                    sqVM.SetIsSelected(false);
                }
            }
        }

        public void SetIsSelected(Model.Coord.T coord)
        {
            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares
                        .OfType<BoardSquareViewModel>()
                        .Where(sq => sq.Coord.Equals(coord)))
                {
                    sqVM.SetIsSelected(true);
                }
            }
        }


        public void RefreshFromModel(Latrunculi.Model.Board.T boardModel)
        {
            PiecesCount.Update(boardModel.WhitePiecesCount, boardModel.BlackPiecesCount);

            foreach (BoardRowViewModel rowVM in Rows)
            {
                foreach (BoardSquareViewModel sqVM in rowVM.Squares.OfType<BoardSquareViewModel>())
                {
                    Model.Square.T sq = boardModel.GetSquare(sqVM.Coord);
                    sqVM.RefreshFromModel(sq);
                }
            }
        }

        public void Init(Latrunculi.Model.Board.T boardModel)
        {
            try
            {
                Clear();
                PiecesCount.Update(0, 0);

                Func<SquareColors, SquareColors> swapColor = new Func<SquareColors, SquareColors>(c => {
                    return (c == SquareColors.scBlack) ? SquareColors.scWhite : SquareColors.scBlack; });
                SquareColors color = SquareColors.scBlack;

                foreach (int rowNumber in boardModel.GetRowNumbers)
                {
                    BoardRowViewModel row = new BoardRowViewModel();
                    Rows.Add(row);
                    row.Squares.Add(new BoardSquareRowHeaderViewModel() { Content = string.Format("{0}", rowNumber) });

                    foreach (Tuple<Model.Coord.T, Model.Square.T> t in boardModel.GetCoordAndSquaresByRowNumber(rowNumber))
                    {
                        BoardSquareViewModel sq = new BoardSquareViewModel();
                        sq.Init(t.Item1, color);

                        row.Squares.Add(sq);
                        color = swapColor(color);
                    }

                    color = swapColor(color);
                }

                BoardRowViewModel columnHeaders = new BoardRowViewModel();
                columnHeaders.Squares.Add(new BoardSquareBottomLeftHeaderViewModel());
                foreach (char col in Model.Coord.ColumnNumbers)
                {
                    columnHeaders.Squares.Add(new BoardSquareColumnHeaderViewModel() { Content = string.Format("{0}", col) });
                }
                Rows.Add(columnHeaders);
            }
            finally
            {
                OnNumberOfRowsOrColsChanged();
            }
        }
    }
}
