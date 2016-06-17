using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Latrunculi.GUI.Controls
{

    public delegate void BoardSquareClickedEvent(object sender, BoardSquareClickedEventArgs e);

    [ComVisible(true)]
    public class BoardSquareClickedEventArgs: EventArgs
    {
        public ViewModel.BoardSquareViewModel vm
        {
            get;
            private set;
        }

        public BoardSquareClickedEventArgs(ViewModel.BoardSquareViewModel viewModel)
        {
            vm = viewModel;
        }
    }

    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Active", GroupName = "CommonStates")]
    public class Board : ItemsControl
    {
        static Board()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Board), new FrameworkPropertyMetadata(typeof(Board)));
        }

        public Board()
        {
            CommandBindings.Add(new CommandBinding(ControlCommands.BoardSquareClick, new ExecutedRoutedEventHandler(BoardSquareClick_Executed), new CanExecuteRoutedEventHandler(BoardSquareClick_CanExecute)));
        }

        public event BoardSquareClickedEvent BoardSquareClicked;

        public int SquareSize
        {
            get { return (int)GetValue(SquareSizeProperty); }
            set { SetValue(SquareSizeProperty, value); }
        }
        public static readonly DependencyProperty SquareSizeProperty =
            DependencyProperty.Register("SquareSize", typeof(int), typeof(Board), new PropertyMetadata(24));


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(Board), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));

        static private void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Board)d).OnIsActiveChanged((bool)e.NewValue);
        }

        private void OnIsActiveChanged(bool isActive)
        {
            VisualStateManager.GoToState(this, isActive ? "Active" : "Normal", true);
        }

        private void BoardSquareClick_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void BoardSquareClick_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if ((e.OriginalSource is Controls.Square) && (e.Parameter is ViewModel.BoardSquareViewModel) &&
                (BoardSquareClicked != null))
                BoardSquareClicked(this, new BoardSquareClickedEventArgs((ViewModel.BoardSquareViewModel)e.Parameter));
        }
    }
}
