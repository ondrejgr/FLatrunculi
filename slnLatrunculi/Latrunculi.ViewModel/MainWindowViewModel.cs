using Latrunculi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {

        }

        public MainWindowViewModel(GameModel model)
        {
            Model = model;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private GameModel _model;
        private GameModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        public string Title
        {
            get
            {
                return "Latrunculi";
            }
        }
    }
}
