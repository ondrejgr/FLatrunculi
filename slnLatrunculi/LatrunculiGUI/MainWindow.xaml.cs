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

        public MainWindow(MainWindowViewModel viewModel, GameController controller)
        {
            InitializeComponent();
            ViewModel = viewModel;
            Controller = controller;
        }

        private CancellationTokenSource cts = null;

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

        private GameController _controller;
        public GameController Controller
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
            e.CanExecute = !ViewModel.IsGameRunning;
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = ViewModel.IsGameRunning;
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
                MessageBox.Show(this, "Soubor se nepodařilo uložit." + Environment.NewLine + Common.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(this, "Soubor se nepodařilo načíst." + Environment.NewLine + Common.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Settings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameCreatedOrPaused;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            PlayerSettingsViewModel vm = TryShowSettings();
            if (vm != null)
            {
                Controller.changePlayerSettings(
                    vm.WhitePlayer.GetPlayerForModel(),
                    vm.BlackPlayer.GetPlayerForModel());
            }
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameCreatedOrPaused;
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
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
                MessageBox.Show(this, "Novou hru se nepodařilo vytvořit." + Environment.NewLine + Common.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameCreatedOrPaused;
        }

        private void Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            TryLoadGame(true);
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGameCreatedOrPaused && !ViewModel.IsGameCreated;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
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
            e.CanExecute = ViewModel.IsGameRunning && (cts != null);
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModel.IsGameRunning && (cts != null))
                cts.Cancel();
        }

        private void Resume_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsGamePaused;
        }

        private void Resume_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModel.IsGamePaused)
                RunGame();
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
            PlayerSettingsViewModel newSettings = TryShowSettings();
            if (newSettings == null)
                newSettings = ViewModel.Settings;

            Controller.NewGame(newSettings.WhitePlayer.GetPlayerForModel(),
                    newSettings.BlackPlayer.GetPlayerForModel());

            RunGame();
        }

        private void RunGame()
        {
            if (cts != null)
                cts.Dispose();
            cts = new CancellationTokenSource();
            //Controller.Run(cts.Token);
            CommandManager.InvalidateRequerySuggested();
        }

        private void LoadGame(string fileName)
        {
            throw new NotImplementedException();
        }

        private void SaveGame(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
