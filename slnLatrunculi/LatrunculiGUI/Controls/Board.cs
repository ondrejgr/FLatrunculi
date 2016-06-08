using System;
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
        // Using a DependencyProperty as the backing store for SquareSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareSizeProperty =
            DependencyProperty.Register("SquareSize", typeof(int), typeof(Board), new PropertyMetadata(24));


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
