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
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public MainWindowViewModel()
        {
            if (Model == null)
                Model = ModelException.TryThrow<GameModel.T>(GameModel.tryCreate());
            InitModel();
        }

        public MainWindowViewModel(GameModel.T model)
        {
            Model = model;
            InitModel();
        }

        void IDisposable.Dispose()
        {
            Model.BoardChanged -= Model_BoardChanged;
            Model.StatusChanged -= Model_StatusChanged;
            Model.PlayerSettingsChanged -= Model_PlayerSettingsChanged;
            Model.ActivePlayerChanged -= Model_ActivePlayerChanged;
            Model.IsMoveSuggestionComputingChanged -= Model_IsMoveSuggestionComputingChanged;
            Model.MoveSuggestionComputed -= Model_MoveSuggestionComputed;
            Model.HistoryChanged -= Model_HistoryChanged;
            Model.ComputerPlayerThinking -= Model_ComputerPlayerThinking;
        }

        private void InitModel()
        {
            Model.BoardChanged += Model_BoardChanged;
            Model.StatusChanged += Model_StatusChanged;
            Model.PlayerSettingsChanged += Model_PlayerSettingsChanged;
            Model.ActivePlayerChanged += Model_ActivePlayerChanged;
            Model.IsMoveSuggestionComputingChanged += Model_IsMoveSuggestionComputingChanged;
            Model.MoveSuggestionComputed += Model_MoveSuggestionComputed;
            Model.HistoryChanged += Model_HistoryChanged;
            Model.ComputerPlayerThinking += Model_ComputerPlayerThinking;
            Board.Init(Model.Board);

            OnPlayerSettingsChanged();
            OnStatusChanged();
        }

        private void Model_HistoryChanged(object sender, HistoryChangedEventArgs e)
        {
            OnPropertyChanged("NumberOfMovesRemaining");
            OnPropertyChanged("NumberOfMovesRemainingWarn");
            OnPropertyChanged("IsUndoStackNotEmpty");
            OnPropertyChanged("IsRedoStackNotEmpty");
        }

        private void Model_ComputerPlayerThinking(object sender, EventArgs e)
        {
            StatusBarText = string.Format("Počítačový hráč \"{0}\" přemýšlí…", ActivePlayerName);
            OnPropertyChanged("IsGameWaitingForHumanPlayerMove");
        }

        private void Model_MoveSuggestionComputed(object sender, MoveEventArgs e)
        {
            if (e.Move.IsSuccess)
            {
                Move.T move = ((Common.Result<Move.T, ErrorDefinitions.Error>.Success)e.Move).Item;
                Board.SetIsSuggestedMove(move.Source);
                Board.SetIsSuggestedMove(move.Target);
            }
            else
            {
                Board.ClearIsSuggestedMove();
            }
        }
          
        private void Model_IsMoveSuggestionComputingChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("IsMoveSuggestionComputing");
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public int NumberOfMovesRemaining
        {
            get
            {
                return 30 - Model.NumberOfMovesWithoutRemoval;
            }
        }

        public bool NumberOfMovesRemainingWarn
        {
            get
            {
                return NumberOfMovesRemaining <= 5;
            }
        }

        private void Model_BoardChanged(object sender, EventArgs e)
        {
            ClearBoardIndicationsAndSelection();
            Board.RefreshFromModel(Model.Board);
            OnPropertyChanged("NumberOfMovesRemaining");
            OnPropertyChanged("NumberOfMovesRemainingWarn");
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public bool IsUndoStackNotEmpty
        {
            get
            {
                return !History.isUndoStackEmpty(Model.Board.History);
            }
        }

        public bool IsRedoStackNotEmpty
        {
            get
            {
                return !History.isRedoStackEmpty(Model.Board.History);
            }
        }

        private void Model_StatusChanged(object sender, EventArgs e)
        {
            OnStatusChanged();
        }

        private void Model_PlayerSettingsChanged(object sender, EventArgs e)
        {
            OnPlayerSettingsChanged();
        }

        private void Model_ActivePlayerChanged(object sender, EventArgs e)
        {
            OnActivePlayerChanged();
        }

        public void ClearBoardIndicationsAndSelection()
        {
            ClearBoardIndications();
            ClearSelection();
        }

        public void ClearBoardIndications()
        {
            Board.ClearIndications();
        }

        public void ClearSelection()
        {
            Board.ClearIsSelected();
            Source = null;
        }

        public void SetSource(Coord.T coord)
        {
            if (coord == null)
                ClearSelection();
            else
            {
                Source = coord;
                Board.SetIsSelected(coord);
            }
        }

        public void SetIsSelected(Coord.T coord)
        {
            Board.SetIsSelected(coord);
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

        private void OnPlayerSettingsChanged()
        {
            Settings.RefreshFromModel(Model.PlayerSettings);
            OnActivePlayerChanged();
        }

        private void OnActivePlayerChanged()
        {
            OnPropertyChanged("ActivePlayerName");
            OnPropertyChanged("IsGameWaitingForHumanPlayerMove");
            ClearBoardIndicationsAndSelection();
            WhitePlayer.IsActive = Model.isWhitePlayerActive;
            BlackPlayer.IsActive = Model.isBlackPlayerActive;
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public string ActivePlayerName
        {
            get
            {
                return Model.getActivePlayerName();
            }
        }

        private void OnStatusChanged()
        {
            ClearBoardIndicationsAndSelection();

            OnPropertyChanged("Status");
            OnPropertyChanged("IsGameCreated");
            OnPropertyChanged("IsGameRunning");
            OnPropertyChanged("IsGameFinished");
            OnPropertyChanged("IsGamePaused");
            OnPropertyChanged("IsGameWaitingForHumanPlayerMove");

            if (IsGameCreated)
            {
                Error = string.Empty;
                Info = "Vytvořte novou hru nebo ji načtěte ze souboru";
            }
            else if (IsGameFinished)
            {
                Error = string.Empty;
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
            else if (IsGamePaused)
            {
                Error = string.Empty;
                Info = "Hra je pozastavena";
            }
            else
            {
                Error = string.Empty;
                Info = string.Empty;
            }

            if (IsGameWaitingForHumanPlayerMove)
                StatusBarText = string.Format("Čekám na tah lidského hráče \"{0}\"…", ActivePlayerName);
            else if (IsGameRunning)
                StatusBarText = "Hra běží...";
            else if (IsGamePaused)
                StatusBarText = "Hra byla pozastavena...";
            else if (IsGameFinished)
                StatusBarText = "Hra skončila.";
            else
                StatusBarText = "";

            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
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

        public bool IsMoveSuggestionComputing
        {
            get
            {
                return Model.IsMoveSuggestionComputing;
            }
        }


        public bool IsGameCreated
        {
            get
            {
                return Status == GameStatus.Created;
            }
        }

        public bool IsGameRunning
        {
            get
            {
                return (Status == GameStatus.Running) ||
                       IsGameWaitingForHumanPlayerMove;
            }
        }

        public bool IsGamePaused
        {
            get
            {
                return Status == GameStatus.Paused;
            }
        }        

        public bool IsGameFinished
        {
            get
            {
                return Status == GameStatus.Finished;
            }
        }

        public bool IsGameWaitingForHumanPlayerMove
        {
            get
            {
                return Status == GameStatus.WaitingForHumanPlayerMove;
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

        private GameModel.T _model;
        public GameModel.T Model
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

        private Coord.T _source;
        public Coord.T Source
        {
            get
            {
                return _source;
            }
            private set
            {
                _source = value;
                OnPropertyChanged("Source");
            }
        }
    }
}
