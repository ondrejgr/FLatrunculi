using Latrunculi.ViewModel;
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
        public ViewModel.BoardSquareViewModel Square
        {
            get;
            private set;
        }

        public bool BlinkRed
        {
            get;
            set;
        }

        public BoardSquareClickedEventArgs(ViewModel.BoardSquareViewModel viewModel)
        {
            BlinkRed = false;
            Square = viewModel;
        }
    }

    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Active", GroupName = "CommonStates")]
    public class Board : Control
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

        public Color WhiteSquareColor
        {
            get { return (Color)GetValue(WhiteSquareColorProperty); }
            set { SetValue(WhiteSquareColorProperty, value); }
        }
        public static readonly DependencyProperty WhiteSquareColorProperty =
            DependencyProperty.Register("WhiteSquareColor", typeof(Color), typeof(Board), new PropertyMetadata(Colors.WhiteSmoke));


        public Color BlackSquareColor
        {
            get { return (Color)GetValue(BlackSquareColorProperty); }
            set { SetValue(BlackSquareColorProperty, value); }
        }
        public static readonly DependencyProperty BlackSquareColorProperty =
            DependencyProperty.Register("BlackSquareColor", typeof(Color), typeof(Board), new PropertyMetadata(Colors.DarkGray));

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
            e.CanExecute = IsActive && (e.OriginalSource is Controls.Square) && (e.Parameter is ViewModel.BoardSquareViewModel);
        }

        private void BoardSquareClick_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (IsActive && (e.OriginalSource is Controls.Square) && (e.Parameter is ViewModel.BoardSquareViewModel))
            {
                Controls.Square sq = (Controls.Square)e.OriginalSource;
                ViewModel.BoardSquareViewModel vm = ((ViewModel.BoardSquareViewModel)e.Parameter);
                if (BoardSquareClicked != null)
                {
                    BoardSquareClickedEventArgs arg = new BoardSquareClickedEventArgs(vm);
                    BoardSquareClicked(this, arg);
                    if (arg.BlinkRed)
                        sq.BlinkRed();
                }
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateIcItemsSource();
        }

        private void UpdateIcItemsSource()
        {
            IEnumerable<BoardRowViewModel> rows = null;
            if (BoardViewModel != null)
                rows = BoardViewModel.Rows;            

            ItemsControl ic = GetTemplateChild("ic") as ItemsControl;
            if (ic != null)
                ic.ItemsSource = rows;
        }

        public BoardViewModel BoardViewModel
        {
            get { return GetValue(BoardViewModelProperty) as BoardViewModel; }
            set { SetValue(BoardViewModelProperty, value); }
        }
        public static readonly DependencyProperty BoardViewModelProperty
                = DependencyProperty.Register("BoardViewModel", typeof(BoardViewModel), typeof(Board),
                                  new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBoardViewModelChanged)));
        private static void OnBoardViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Board b = (Board)d;
            b.UpdateIcItemsSource();
        }
    }
}
