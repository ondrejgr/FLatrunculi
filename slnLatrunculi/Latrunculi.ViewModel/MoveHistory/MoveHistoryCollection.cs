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
    }
}
