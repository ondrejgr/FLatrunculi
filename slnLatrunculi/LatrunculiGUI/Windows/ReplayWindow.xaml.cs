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
            InitializeComponent();
            DataContext = viewModel;
            Controller = controller;
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
                
        private void History_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}
