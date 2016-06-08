using Latrunculi.Controller;
using Latrunculi.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
        
        private bool MainWindowCommand_CanExecute
        {
            get
            {
                return !ViewModel.IsBusy;
            }
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            CancelEventArgs cnl = new CancelEventArgs();
            OnClosing(cnl);
            if (!cnl.Cancel)
                Close();
        }

        private void Help_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
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
            e.Cancel = ViewModel.IsBusy;

        }

        private void Settings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            SettingsWindow win = new SettingsWindow() { Owner = this, ViewModel = ViewModel.Settings };
            win.ShowDialog();
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Load_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
        }

        private void Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = MainWindowCommand_CanExecute;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
