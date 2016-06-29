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
            Model.StatusChanged -= Model_StatusChanged;
            Model.GameError -= Model_GameError;
            Model.PositionChanged -= Model_PositionChanged;
        }

        private void InitModel()
        {
            Model.BoardChanged += Model_BoardChanged;
            Model.ActivePlayerChanged += Model_ActivePlayerChanged;
            Model.StatusChanged += Model_StatusChanged;
            Model.GameError += Model_GameError;
            Model.PositionChanged += Model_PositionChanged;

            Settings.RefreshFromModel(Model.PlayerSettings);
            Board.Init(Model.Board);
            OnStatusChanged();
        }

        private void Model_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            Position = e.ID;
        }

        private void Model_GameError(object sender, GameErrorEventArgs e)
        {
            Info = string.Format("Chyba: {0}", ErrorMessages.toString(e.Error));
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

        private void Model_StatusChanged(object sender, EventArgs e)
        {
            OnStatusChanged();
        }

        private void OnActivePlayerChanged()
        {
            OnPropertyChanged("ActivePlayerName");
            WhitePlayer.IsActive = Model.isWhitePlayerActive;
            BlackPlayer.IsActive = Model.isBlackPlayerActive;
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void OnStatusChanged()
        {
            OnPropertyChanged("Status");
            OnPropertyChanged("IsCreated");
            OnPropertyChanged("IsPaused");
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("IsFinished");

            if (IsCreated)
                Info = string.Empty;
            else if (IsPaused)
                Info = "Pozastaveno";
            else if (IsRunning)
                Info = string.Empty;
            else if (IsFinished)
            {
                if (Model.Result.IsGameOverResult)
                {
                    Rules.GameOverResult result = ((Rules.GameResult.GameOverResult)Model.Result).Item;
                    if (result.IsDraw)
                        Info = "Konec hry - remíza";
                    else if (result.IsVictory)
                    {
                        Rules.Victory vict = ((Rules.GameOverResult.Victory)result).Item;
                        if (vict.IsBlackWinner)
                            Info = string.Format("Konec hry - zvítězil {0} (černý)", this.BlackPlayer.Name);
                        else if (vict.IsWhiteWinner)
                            Info = string.Format("Konec hry - zvítězil {0} (bílý)", this.WhitePlayer.Name);
                        else
                            Info = "Hra skončila bez vítěze - chyba aplikace ??";
                    }
                    else
                        Info = "Hra skončila s neznámým vítězem - chyba aplikace ??";
                }
                else
                    Info = "Hra skončila bez výsledku - chyba aplikace ??";
            }
        }

        private int _position;
        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value <= NumberOfMoves)
                {
                    _position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        public int NumberOfMoves
        {
            get
            {
                return Model.getNumberOfMovesInHistory();
            }
        }

        public bool IsFinished
        {
            get
            {
                return Status.IsFinished;
            }
        }

        public bool IsCreated
        {
            get
            {
                return Status.IsCreated;
            }
        }

        public bool IsPaused
        {
            get
            {
                return Status.IsPaused;
            }
        }

        public bool IsRunning
        {
            get
            {
                return Status.IsRunning;
            }
        }

        public Model.ReplayStatus Status
        {
            get
            {
                return Model.Status;
            }
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
                    if (Model != null)
                        Model.setInverval(value);
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
