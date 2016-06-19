﻿using Latrunculi.Model;
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
                Model = ModelException.TryThrow<GameModel.T>(GameModel.tryCreate);
            InitModel();
        }

        public MainWindowViewModel(GameModel.T model)
        {
            Model = model;
            InitModel();
        }

        void IDisposable.Dispose()
        {
            Model.BoardChanged -= new ModelChangeEventHandler(Model_BoardChanged);
            Model.StatusChanged -= new ModelChangeEventHandler(Model_StatusChanged);
            Model.PlayerSettingsChanged -= new ModelChangeEventHandler(Model_PlayerSettingsChanged);
            Model.ActivePlayerChanged -= new ModelChangeEventHandler(Model_ActivePlayerChanged);
            Model.IsMoveSuggestionComputingChanged -= new ModelChangeEventHandler(Model_IsMoveSuggestionComputingChanged);
            Model.MoveSuggestionComputed -= new MoveSuggestionComputedEventHandler(Model_MoveSuggestionComputed);
            Model.GameError -= new GameErrorEventHandler(Model_GameError);
        }

        private void InitModel()
        {
            Model.BoardChanged += new ModelChangeEventHandler(Model_BoardChanged);
            Model.StatusChanged += new ModelChangeEventHandler(Model_StatusChanged);
            Model.PlayerSettingsChanged += new ModelChangeEventHandler(Model_PlayerSettingsChanged);
            Model.ActivePlayerChanged += new ModelChangeEventHandler(Model_ActivePlayerChanged);
            Model.IsMoveSuggestionComputingChanged += new ModelChangeEventHandler(Model_IsMoveSuggestionComputingChanged);
            Model.MoveSuggestionComputed += new MoveSuggestionComputedEventHandler(Model_MoveSuggestionComputed);
            Model.GameError += new GameErrorEventHandler(Model_GameError);

            Board.Init(Model.Board);
            OnBoardChanged();
            OnPlayerSettingsChanged();
            OnActivePlayerChanged();

            OnStatusChanged();
        }

        public event MoveSuggestionComputedEventHandler MoveSuggestionComputed;
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


            if (MoveSuggestionComputed != null)
                MoveSuggestionComputed(this, e);
        }

        private void Model_IsMoveSuggestionComputingChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("IsMoveSuggestionComputing");
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        public event GameErrorEventHandler GameError;
        private void Model_GameError(object sender, GameErrorEventArgs e)
        {
            if (GameError != null)
                GameError(this, e);
        }

        private void Model_BoardChanged(object sender, EventArgs e)
        {
            ClearBoardIndicationsAndSelection();
            OnBoardChanged();
        }

        private void Model_StatusChanged(object sender, EventArgs e)
        {
            ClearBoardIndicationsAndSelection();
            OnStatusChanged();
        }

        private void Model_PlayerSettingsChanged(object sender, EventArgs e)
        {
            ClearBoardIndicationsAndSelection();
            OnPlayerSettingsChanged();
        }

        private void Model_ActivePlayerChanged(object sender, EventArgs e)
        {
            ClearBoardIndicationsAndSelection();
            OnActivePlayerChanged();
        }

        public void ClearBoardIndicationsAndSelection()
        {
            Board.ClearIndications();
        }

        private void OnBoardChanged()
        {
            Board.RefreshFromModel(Model.Board);
        }

        private void OnPlayerSettingsChanged()
        {
            Settings.RefreshFromModel(Model.PlayerSettings);
            OnActivePlayerChanged();
        }

        private void OnActivePlayerChanged()
        {
            WhitePlayer.IsActive = Model.isWhitePlayerActive;
            BlackPlayer.IsActive = Model.isBlackPlayerActive;
            Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void OnStatusChanged()
        {
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
                Info = "Hra skončila";
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
                StatusBarText = "Čekám na tah lidského hráče...";
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
        private GameModel.T Model
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
    }
}
