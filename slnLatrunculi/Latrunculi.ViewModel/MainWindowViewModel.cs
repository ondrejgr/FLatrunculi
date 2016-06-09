using Latrunculi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Latrunculi.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            if (Model == null)
                Model = new GameModel();

            Model.BoardChanged += new ModelChangeEventHandler(Model_BoardChanged);
            Model.StatusChanged += new ModelChangeEventHandler(Model_StatusChanged);
            Model.PlayerSettingsChanged += new ModelChangeEventHandler(Model_PlayerSettingsChanged);
            Model.ActivePlayerChanged += new ModelChangeEventHandler(Model_ActivePlayerChanged);

            Board.Init(Model.Board);
            //OnBoardChanged();
            //OnPlayerSettingsChanged();
            //OnActivePlayerChanged();

            OnStatusChanged(Model.Status);
        }

        public MainWindowViewModel(GameModel model): this()
        {
            Model = model;
        }

        private void Model_BoardChanged(object sender, EventArgs e)
        {
            OnBoardChanged();
        }

        private void Model_StatusChanged(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged("IsGameCreated");
        }

        private void Model_PlayerSettingsChanged(object sender, EventArgs e)
        {
            OnPlayerSettingsChanged();
        }

        private void Model_ActivePlayerChanged(object sender, EventArgs e)
        {
            OnActivePlayerChanged();
        }

        private void OnBoardChanged()
        {
            Board.RefreshFromModel(Model.Board);
        }

        private void OnPlayerSettingsChanged()
        {
            Settings.RefreshFromModel(Model.PlayerSettings);
        }

        private void OnActivePlayerChanged()
        {

        }

        private void OnStatusChanged(Model.GameStatus status)
        {
            if (IsGameCreated)
            {
                Error = string.Empty;
                Info = "Vytvořte novou hru nebo ji načtěte ze souboru";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public Model.GameStatus Status
        {
            get
            {
                return Model.Status;
            }
        }

        public bool IsGameCreated
        {
            get
            {
                return Status == GameStatus.Created;
            }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            private set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private string _fileTitle = string.Empty;
        public string FileTitle
        {
            get
            {
                return _fileTitle;
            }
            private set
            {
                _fileTitle = value;
                OnPropertyChanged("FileTitle");
                OnPropertyChanged("Title");
            }
        }

        public void SetFileName(string fileName, string fileTitle)
        {
            FileName = fileName;
            FileTitle = fileTitle;
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
                if (string.IsNullOrEmpty(FileTitle))
                    return "Latrunculi";
                else
                    return string.Format("{0} - [{1}]", "Latrunculi", FileTitle);
            }
        }

        private string _error;
        public string Error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                OnPropertyChanged("Error");
                OnPropertyChanged("ErrorExists");
            }
        }

        public bool ErrorExists
        {
            get
            {
                return !string.IsNullOrEmpty(Error);
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
