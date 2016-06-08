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
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        static public HelpWindow Current
        {
            get;
            set;
        }

        public HelpWindow()
        {
            InitializeComponent();
        }

        public HelpWindowViewModel ViewModel
        {
            get
            {
                return (HelpWindowViewModel)DataContext;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Current = null;
        }

        private void Home_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void Home_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            ClearSelection();
            ViewModel.GoTo(ViewModel.Items[0]);
        }

        private void Back_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.BackEnabled;
        }

        private void Back_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            ClearSelection();
            ViewModel.GoBack();
        }

        private void Forward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = ViewModel.ForwardEnabled;
        }

        private void Forward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            ClearSelection();
            ViewModel.GoForward();
        }

        private bool skipSelectionHandler = false;
        private void ClearSelection()
        {
            skipSelectionHandler = true;
            try
            {
                lv.SelectedItem = null;
            }
            finally
            {
                skipSelectionHandler = false;
            }
        }

        private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == lv && !skipSelectionHandler)
            {
                e.Handled = true;
                HelpItem item = lv.SelectedItem as HelpItem;
                if (item != null)
                    ViewModel.GoTo(item);
            }
        }
    }
}
