using Latrunculi.Controller;
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
            ViewModel.MoveSuggestionComputed += ViewModel_MoveSuggestionComputed;
            ViewModel.GameError += ViewModel_GameError;
            Controller = controller;
        }

        private void ViewModel_MoveSuggestionComputed(object sender, Model.MoveEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Model.Move.T move = ModelException.TryThrow<Model.Move.T>(e.Move);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this,
                        string.Format("Nepodařilo se vypočítat nejlepší tah: {0}", ViewModelCommon.ConvertExceptionToShortString(exc)),
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (e.PropertyName == "Status" || e.PropertyName == "IsMoveSuggestionComputing")
                    Mouse.OverrideCursor = (ViewModel.Status == Model.GameStatus.Running || ViewModel.IsMoveSuggestionComputing) ? Cursors.AppStarting : null;
            }));
        }

        private void ViewModel_GameError(object sender, Model.GameErrorEventArgs e)
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
                int width = ((int)e.NewSize.Width - 300) / ViewModel.Board.NumberOfCols;
                if (width < 18)
                    width = 18;
                if (width > 100)
                    width = 100;

                if (board.SquareSize != width)
                    board.SquareSize = width;
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

                SaveGame(ViewModel.FileName);

                if (showSuccess)
                    MessageBox.Show(this, "Hra byla uložena.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Soubor se nepodařilo uložit." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
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

                LoadGame(ViewModel.FileName);

                if (showSuccess)
                    MessageBox.Show(this, "Hra byla načtena.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception exc)
            {
                ViewModel.SetFileName(oldFileName, oldFileTitle);
                MessageBox.Show(this, "Soubor se nepodařilo načíst." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        new Tuple<Model.Player.Types, Model.Player.Types>((Model.Player.Types)vm.WhitePlayer.PlayerType, (Model.Player.Types)vm.BlackPlayer.PlayerType),
                        new Tuple<string, string>(vm.WhitePlayer.Name, vm.BlackPlayer.Name),
                        new Tuple<Model.Player.Levels, Model.Player.Levels>((Model.Player.Levels)vm.WhitePlayer.Level, (Model.Player.Levels)vm.BlackPlayer.Level));
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
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze pokračovat ve hře kvůli chybě." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void board_BoardSquareClicked(object sender, Controls.BoardSquareClickedEventArgs e)
        {

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
                    new Tuple<Model.Player.Types, Model.Player.Types>((Model.Player.Types)vm.WhitePlayer.PlayerType, (Model.Player.Types)vm.BlackPlayer.PlayerType),
                    new Tuple<string, string>(vm.WhitePlayer.Name, vm.BlackPlayer.Name),
                    new Tuple<Model.Player.Levels, Model.Player.Levels>((Model.Player.Levels)vm.WhitePlayer.Level, (Model.Player.Levels)vm.BlackPlayer.Level));

                ModelException.TryThrow<GameController.T>(Controller.TryNewGame());
                ModelException.TryThrow<GameController.T>(Controller.TryRun());
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void LoadGame(string fileName)
        {
            throw new NotImplementedException();
        }

        private void SaveGame(string fileName)
        {
            throw new NotImplementedException();
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
                    ViewModel.Board.ClearIsSuggestedMove();
                    ModelException.TryThrow<GameController.T>(Controller.TrySuggestMove());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nepodařilo se napovědět tah." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
