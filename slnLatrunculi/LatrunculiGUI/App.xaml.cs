using Latrunculi.Controller;
using Latrunculi.Model;
using Latrunculi.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Latrunculi.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (MainWindow != null)
                MessageBox.Show(MainWindow, Common.ConvertExceptionToString(e.Exception), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(Common.ConvertExceptionToString(e.Exception), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Shutdown(-1);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                GameModel model = new GameModel();
                GameController controller = new GameController(model);
                MainWindowViewModel viewModel = new MainWindowViewModel(model);

                MainWindow win = new GUI.MainWindow(viewModel, controller);
                MainWindow = win;
                win.Show();
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Aplikaci Latrunculi se nepodařilo spustit:{0}{1}", Environment.NewLine, Common.ConvertExceptionToString(exc)), 
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
