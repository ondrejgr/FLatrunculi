using Latrunculi.Controller;
using Latrunculi.Model;
using Latrunculi.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Common;

namespace Latrunculi.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel, GameController.T controller)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Model.MoveSuggestionComputed += Model_MoveSuggestionComputed;
            ViewModel.Model.HistoryItemAdded += Model_HistoryItemAdded;
            ViewModel.Model.HistoryItemRemoved += Model_HistoryItemRemoved;
            ViewModel.Model.HistoryCleared += Model_HistoryCleared;
            ViewModel.Model.GameError += Model_GameError;
            Controller = controller;
        }

        private void Model_HistoryItemRemoved(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (ViewModel.Board.History.Count > 0)
                        ViewModel.Board.History.RemoveFirstItem();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this,
                        string.Format("Nepodařilo se odstranit tah z historie: {0}", ViewModelCommon.ConvertExceptionToShortString(exc)),
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Model_HistoryCleared(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    ViewModel.Board.History.Clear();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this,
                        string.Format("Nepodařilo se vymazat historii tahů: {0}", ViewModelCommon.ConvertExceptionToShortString(exc)),
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Model_HistoryItemAdded(object sender, HistoryItemAddedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    HistoryItem.T item = e.Item;
                    ViewModel.Board.History.InsertItem(item);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this,
                        string.Format("Nepodařilo se uložit tah do historie: {0}", ViewModelCommon.ConvertExceptionToShortString(exc)),
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Model_MoveSuggestionComputed(object sender, Model.MoveEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (e.Move.IsError)
                {
                    string msg = ErrorMessages.toString(((Result<Move.T, ErrorDefinitions.Error>.Error)e.Move).Item);
                    MessageBox.Show(this,
                        string.Format("Nepodařilo se vypočítat nejlepší tah: {0}", msg),
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);

                    
                }
            }));
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.PropertyName == "Status" || e.PropertyName == "IsMoveSuggestionComputing")
                    Mouse.OverrideCursor = (ViewModel.Status == Model.GameStatus.Running || ViewModel.IsMoveSuggestionComputing) ? Cursors.AppStarting : null;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Model_GameError(object sender, Model.GameErrorEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show(this,
                    string.Format("Při běhu hry došlo k chybě: {0}", ErrorMessages.toString(e.Error)),
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        public MainWindowViewModel ViewModel
        {
            get
            {
                return (MainWindowViewModel)DataContext;
            }
            private set
            {
                DataContext = value;
            }
        }

        private GameController.T _controller;
        public GameController.T Controller
        {
            get
            {
                return _controller;
            }
            private set
            {
                _controller = value;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            e.Handled = true;
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                int width = ((int)e.NewSize.Width - 300) / ViewModel.NumberOfCols;
                if (width < 18)
                    width = 18;
                if (width > 100)
                    width = 100;

                if (board.SquareSize != width)
                    board.SquareSize = width;
                if (historyBoard.SquareSize != width)
                    historyBoard.SquareSize = width;
            }
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void Help_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (HelpWindow.Current == null)
                HelpWindow.Current = new HelpWindow() { Owner = this };
            HelpWindow.Current.Show();
        }

        private void CancelWorkflows()
        {
            if (ViewModel.IsMoveSuggestionComputing)
            {
                if (MainWindowCommands.CancelSuggestMove.CanExecute(null, this))
                    MainWindowCommands.CancelSuggestMove.Execute(null, this); 
            }
            if (ViewModel.IsGameRunning)
            {
                if (MainWindowCommands.Pause.CanExecute(null, this))
                    MainWindowCommands.Pause.Execute(null, this);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
            CancelWorkflows();
            if (!e.Cancel && !ViewModel.IsGameCreated)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Chcete před ukončením aplikace uložit stávající hru ?");
                switch (MessageBox.Show(this, sb.ToString(), "Uložit hru ?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                    case MessageBoxResult.Yes:
                        e.Cancel = !TrySaveGame(false, false);
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private bool GetFileNameToViewModel(bool isSaveDialog, bool isSaveAsDialog)
        {
            if (isSaveDialog && !isSaveAsDialog && !string.IsNullOrEmpty(ViewModel.FileName))
                return true;

            FileDialog dialog;
            if (isSaveDialog)
                dialog = new SaveFileDialog();
            else
                dialog = new OpenFileDialog();

            dialog.AddExtension = true;
            dialog.CheckPathExists = true;
            if (isSaveDialog)
                ((SaveFileDialog)dialog).OverwritePrompt = true;
            else
                dialog.CheckFileExists = true;
            dialog.DefaultExt = "lat";
            dialog.Title = isSaveDialog ? "Uložit hru" : "Načíst hru";
            dialog.Filter = "Hra|*.lat|Všechny soubory|*.*";
            if (!(dialog.ShowDialog(this) ?? false))
                return false;
            else
            {
                ViewModel.SetFileName(dialog.FileName, dialog.SafeFileName);
                return true;
            }
        }

        private bool TrySaveGame(bool showSuccess, bool isSaveAsDialog)
        {
            try
            {
                if (!GetFileNameToViewModel(true, isSaveAsDialog))
                    return false;

                ModelException.TryThrow<GameController.T>(Controller.TrySaveGame(ViewModel.FileName));

                if (showSuccess)
                    MessageBox.Show(this, "Hra byla uložena.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool TryLoadGame(bool showSuccess)
        {
            string oldFileName = ViewModel.FileName, oldFileTitle = ViewModel.FileTitle;
            try
            {
                if (!GetFileNameToViewModel(false, false))
                    return false;

                ModelException.TryThrow<GameController.T>(Controller.TryLoadGame(ViewModel.FileName));

                if (showSuccess)
                    MessageBox.Show(this, "Hra byla načtena.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception exc)
            {
                ViewModel.SetFileName(oldFileName, oldFileTitle);
                MessageBox.Show(this, ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Settings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsGameRunning;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (!ViewModel.IsGameRunning)
            {
                PlayerSettingsViewModel vm = TryShowSettings();
                if (vm != null)
                {
                    Controller.changePlayerSettings(
                        new Tuple<Model.Player.Types, Model.Player.Types>(PlayerViewModel.PlayerTypeToModel(vm.WhitePlayer), PlayerViewModel.PlayerTypeToModel(vm.BlackPlayer)),
                        new Tuple<string, string>(vm.WhitePlayer.Name, vm.BlackPlayer.Name),
                        new Tuple<Model.Player.Levels, Model.Player.Levels>(PlayerViewModel.PlayerLevelToModel(vm.WhitePlayer), PlayerViewModel.PlayerLevelToModel(vm.BlackPlayer)));
                }
            }
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsGameRunning;
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModel.IsGameRunning)
                return;
            try
            {
                bool cancel = false;
                if (!ViewModel.IsGameCreated)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Chcete před spuštěním nové hry uložit stávající hru ?");
                    switch (MessageBox.Show(this, sb.ToString(), "Uložit hru ?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                    {
                        case MessageBoxResult.Cancel:
                            cancel = true;
                            break;
                        case MessageBoxResult.Yes:
                            cancel = !TrySaveGame(false, false);
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }
                if (!cancel)
                    NewGame();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Novou hru se nepodařilo vytvořit." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsGameRunning;
        }

        private void Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (!ViewModel.IsGameRunning)
                TryLoadGame(true);
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGamePaused || ViewModel.IsGameFinished;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModel.IsGamePaused || ViewModel.IsGameFinished)
                TrySaveGame(true, false);
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            Save_CanExecute(sender, e);
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModel.IsGamePaused || ViewModel.IsGameFinished)
                TrySaveGame(true, true);
        }

        private void Navigate_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void Navigate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HelpWindow.Current == null)
                HelpWindow.Current = new HelpWindow() { Owner = this };

            HelpItem item = HelpWindow.Current.ViewModel.Items.FirstOrDefault(i => i.Key == (string)e.Parameter);
            if (item == null)
                item = HelpWindow.Current.ViewModel.Items[0];
            HelpWindow.Current.ViewModel.GoTo(item);
            HelpWindow.Current.Show();
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameRunning;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (ViewModel.IsGameRunning)
                {
                    ModelException.TryThrow<GameController.T>(Controller.TryPause());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze pozastavit hru kvůli chybě." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Resume_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGamePaused;
        }

        private void Resume_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (ViewModel.IsGamePaused)
                {
                    ModelException.TryThrow<GameController.T>(Controller.TryResume());
                    FocusBoard();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze pokračovat ve hře kvůli chybě." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PlayerSettingsViewModel TryShowSettings()
        {
            PlayerSettingsViewModel vm = (PlayerSettingsViewModel)ViewModel.Settings.Clone();
            SettingsWindow win = new SettingsWindow() { Owner = this, ViewModel = vm };
            if (win.ShowDialog() ?? false)
                return vm;
            else
                return null;
        }

        private void NewGame()
        {
            PlayerSettingsViewModel vm = TryShowSettings();
            if (vm != null)
            {
                Controller.changePlayerSettings(
                    new Tuple<Model.Player.Types, Model.Player.Types>(PlayerViewModel.PlayerTypeToModel(vm.WhitePlayer), PlayerViewModel.PlayerTypeToModel(vm.BlackPlayer)),
                    new Tuple<string, string>(vm.WhitePlayer.Name, vm.BlackPlayer.Name),
                    new Tuple<Model.Player.Levels, Model.Player.Levels>(PlayerViewModel.PlayerLevelToModel(vm.WhitePlayer), PlayerViewModel.PlayerLevelToModel(vm.BlackPlayer)));

                ViewModel.ShowHistory = false;
                ModelException.TryThrow<GameController.T>(Controller.TryNewGame());
                ModelException.TryThrow<GameController.T>(Controller.TryRun());
                FocusBoard();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void CancelSuggestMove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsMoveSuggestionComputing;
        }

        private void CancelSuggestMove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (ViewModel.IsMoveSuggestionComputing)
                {
                    ModelException.TryThrow<GameController.T>(Controller.TryCancelSuggestMove());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nepodařilo se zrušit nápovědu tahu." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SuggestMove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameWaitingForHumanPlayerMove && !ViewModel.IsMoveSuggestionComputing;
        }

        private void SuggestMove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (ViewModel.IsGameWaitingForHumanPlayerMove && !ViewModel.IsMoveSuggestionComputing)
                {
                    ViewModel.ClearBoardIndications();
                    ModelException.TryThrow<GameController.T>(Controller.TrySuggestMove());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nepodařilo se napovědět tah." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void board_BoardSquareClicked(object sender, Controls.BoardSquareClickedEventArgs e)
        {
            try
            {
                if (ViewModel.IsGameWaitingForHumanPlayerMove)
                {
                    if (ViewModel.Source != null)
                    {
                        if (e.Square.Coord.Equals(ViewModel.Source) && e.Square.IsSelected)
                        {
                            // clear selection if same source was clicked
                            ViewModel.ClearSelection();
                            return;
                        }
                        // source was selected, try create move from target
                        Result<Move.T, ErrorDefinitions.Error> result = 
                                Controller.TryGetValidMove(ViewModel.Source, e.Square.Coord);

                        if (result.IsSuccess)
                        {
                            // user selected valid move
                            if (MainWindowCommands.CancelSuggestMove.CanExecute(null, this))
                                MainWindowCommands.CancelSuggestMove.Execute(null, this);

                            Move.T move = ((Result<Move.T, ErrorDefinitions.Error>.Success)result).Item;
                            ModelException.TryThrow<Move.T>(Controller.TrySetSelectedMove(move));
                        }
                        else
                        {
                            // unable to create move with specified target
                            ErrorDefinitions.Error err = ((Result<Move.T, ErrorDefinitions.Error>.Error)result).Item;
                            if (!err.IsNoValidMoveExists)
                                throw new ModelException(err);
                            else
                            {
                                // try to use it as new source
                                bool isValidMoveSource = ModelException.TryThrow<bool>(Controller.TryMoveExistsForCoord(e.Square.Coord));
                                if (!isValidMoveSource)
                                {
                                    e.BlinkRed = true;
                                    return;
                                }
                                else
                                    ViewModel.ClearSelection();
                            }
                        }
                    }

                    if (ViewModel.Source == null)
                    {
                        // no source exists, select this coord as new source
                        bool isValidMoveSource = ModelException.TryThrow<bool>(Controller.TryMoveExistsForCoord(e.Square.Coord));
                        e.BlinkRed = !isValidMoveSource;

                        if (isValidMoveSource)
                        {
                            ViewModel.SetSource(e.Square.Coord);
                            Controller.GetPossibleTargetCoords(ViewModel.Source).ForEach(ViewModel.SetIsSelected);
                        }
                    }

                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nebyl vybrán platný tah." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FocusBoard()
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!ViewModel.ShowHistory)
                        board.Focus();
                }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void History_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            try
            {
                MoveHistoryItem item = e.AddedItems.OfType<MoveHistoryItem>().FirstOrDefault();

                if (item != null)
                    ModelException.TryThrow<GameController.T>(Controller.TryGoToHistoryMove(item.ID));
                else
                    ModelException.TryThrow<GameController.T>(Controller.TryClearHistoryBoard());

                ViewModel.HistoryBoard.RefreshFromModel(ViewModel.Model.HistoryBoard);
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze zobrazit historii tahů na desce." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsGameCreated && ViewModel.IsUndoStackNotEmpty;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (!ViewModel.IsGameCreated && ViewModel.IsUndoStackNotEmpty)
                {
                    ModelException.TryThrow<GameController.T>(Controller.TryUndo());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Tah nelze vzít zpět." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsGameCreated && ViewModel.IsRedoStackNotEmpty;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (!ViewModel.IsGameCreated && ViewModel.IsRedoStackNotEmpty)
                {
                    ModelException.TryThrow<GameController.T>(Controller.TryRedo());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Tah nelze opakovat." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Replay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameFinished;
        }

        private void Replay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (ViewModel.IsGameFinished)
                {
                    ReplayModel.T replayModel = ModelException.TryThrow<ReplayModel.T>(ReplayModel.tryCreate(ViewModel.Model.Board, ViewModel.Model.PlayerSettings));
                    ReplayController.T controller = ModelException.TryThrow<ReplayController.T>(ReplayController.tryCreate(replayModel));
                    ReplayWindowViewModel vm = new ReplayWindowViewModel(replayModel);

                    ReplayWindow win = new ReplayWindow(controller, vm);
                    win.Owner = this;
                    win.ShowDialog();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Okno přehrávače nelze otevřít." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
