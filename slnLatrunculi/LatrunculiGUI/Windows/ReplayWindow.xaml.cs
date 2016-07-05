using Latrunculi.Controller;
using Latrunculi.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Latrunculi.GUI
{
    /// <summary>
    /// Interaction logic for ReplayWindow.xaml
    /// </summary>
    public partial class ReplayWindow : Window
    {
        public ReplayWindow()
        {
            InitializeComponent();
        }

        public ReplayWindow(Latrunculi.Controller.ReplayController.T controller, ReplayWindowViewModel viewModel)
        {
            DataContext = viewModel;
            Controller = controller;

            InitializeComponent();

            ViewModel.Model.PositionChanged += Model_PositionChanged;

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryGoToPosition(0));
                }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private bool ignoreChange = false;
        private void Model_PositionChanged(object sender, Model.PositionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action<int>((id) =>
            {
                ignoreChange = true;
                slider.Value = id;
                ignoreChange = false;
            }), System.Windows.Threading.DispatcherPriority.Input, e.ID);
        }

        private Latrunculi.Controller.ReplayController.T _controller;
        public Latrunculi.Controller.ReplayController.T Controller
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

        public ReplayWindowViewModel ViewModel
        {
            get
            {
                return (ReplayWindowViewModel)DataContext;
            }
        }
                
        private void win_SizeChanged(object sender, SizeChangedEventArgs e)
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
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;
            try
            {
                if (!ignoreChange)
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryGoToPosition((int)e.NewValue));
                }
            }
            catch (Exception exc)
            {
                ViewModel.Info = string.Format("Chyba: {0}", ViewModelCommon.ConvertExceptionToShortString(exc));
            }
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsCreated && (ViewModel.Position > 0);
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.IsCreated && (ViewModel.Position > 0))
            {
                try
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryDecPosition());
                }
                catch (Exception exc)
                {
                    ViewModel.Info = string.Format("Chyba: {0}", ViewModelCommon.ConvertExceptionToShortString(exc));
                }
            }
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsCreated && (ViewModel.Position < ViewModel.NumberOfMoves);
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.IsCreated && (ViewModel.Position < ViewModel.NumberOfMoves))
            {
                try
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryIncPosition());
                }
                catch (Exception exc)
                {
                    ViewModel.Info = string.Format("Chyba: {0}", ViewModelCommon.ConvertExceptionToShortString(exc));
                }
            }
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = !ViewModel.IsCreated && ViewModel.IsRunning;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!ViewModel.IsCreated && ViewModel.IsRunning)
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryPause());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze pozastavit replay kvůli chybě." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Resume_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.IsPaused || ViewModel.IsCreated;
        }

        private void Resume_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (ViewModel.IsPaused || ViewModel.IsCreated)
                {
                    ModelException.TryThrow<ReplayController.T>(Controller.tryResume());
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, "Nelze spustit replay kvůli chybě." + Environment.NewLine + ViewModelCommon.ConvertExceptionToShortString(exc), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainWindowCommands.Pause.CanExecute(null, this))
                MainWindowCommands.Pause.Execute(null, this);
        }

        private void win_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainWindowCommands.Pause.CanExecute(null, this))
                MainWindowCommands.Pause.Execute(null, this);
        }
    }
}
