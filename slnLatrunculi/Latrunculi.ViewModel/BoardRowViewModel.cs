using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class BoardRowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BoardSquareBaseViewModel> _squares = new ObservableCollection<BoardSquareBaseViewModel>();
        public ObservableCollection<BoardSquareBaseViewModel> Squares
        {
            get
            {
                return _squares;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
