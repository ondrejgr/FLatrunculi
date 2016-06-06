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
            if (Model == null)
                Model = new GameModel();

            Board.Init(Model.Board);
        }

        public MainWindowViewModel(GameModel model): this()
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

        private BoardViewModel _board = new BoardViewModel();
        public BoardViewModel Board
        {
            get
            {
                return _board;
            }
        }        

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _statusBarText;
        public string StatusBarText
        {
            get
            {
                return _statusBarText;
            }
            set
            {
                _statusBarText = value;
                OnPropertyChanged("StatusBarText");
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
