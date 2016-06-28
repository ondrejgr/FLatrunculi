using Latrunculi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Latrunculi.ViewModel
{
    public class ReplayWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public ReplayWindowViewModel(ReplayModel.T model)
        {
            Model = model;
            InitModel();
        }

        void IDisposable.Dispose()
        {
            Model.BoardChanged -= Model_BoardChanged;
            Model.ActivePlayerChanged -= Model_ActivePlayerChanged;
        }

        private void InitModel()
        {
            Model.BoardChanged += Model_BoardChanged;
            Model.ActivePlayerChanged += Model_ActivePlayerChanged;
            Settings.RefreshFromModel(Model.PlayerSettings);
            Board.Init(Model.Board);
        }

        private void Model_BoardChanged(object sender, EventArgs e)
        {
            Board.RefreshFromModel(Model.Board);
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void Model_ActivePlayerChanged(object sender, EventArgs e)
        {
            OnActivePlayerChanged();
        }

        private void OnActivePlayerChanged()
        {
            OnPropertyChanged("ActivePlayerName");
            WhitePlayer.IsActive = Model.isWhitePlayerActive;
            BlackPlayer.IsActive = Model.isBlackPlayerActive;
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public int _speed = 1000;
        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                if ((value <= 10000) && (value >= 100))
                {
                    _speed = value;
                    OnPropertyChanged("Speed");
                }
            }
        }

        public string ActivePlayerName
        {
            get
            {
                return Model.getActivePlayerName();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private ReplayModel.T _model;
        public ReplayModel.T Model
        {
            get
            {
                return _model;
            }
            private set
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

        public int NumberOfCols
        {
            get
            {
                if (Board != null)
                    return Board.NumberOfCols;
                else
                    return 0;
            }
        }

        private PlayerSettingsViewModel _settings = new PlayerSettingsViewModel();
        public PlayerSettingsViewModel Settings
        {
            get
            {
                return _settings;
            }
        }

        public PlayerViewModel WhitePlayer
        {
            get
            {
                return Settings.WhitePlayer;
            }
        }

        public PlayerViewModel BlackPlayer
        {
            get
            {
                return Settings.BlackPlayer;
            }
        }

        private string _info;
        public string Info
        {
            get
            {
                return _info;
            }
            set
            {
                _info = value;
                OnPropertyChanged("Info");
                OnPropertyChanged("InfoExists");
            }
        }

        public bool InfoExists
        {
            get
            {
                return !string.IsNullOrEmpty(Info);
            }
        }
    }
}
