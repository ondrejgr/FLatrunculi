using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LatrunculiGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StringBuilder sbMessages = new StringBuilder();
            Exception exc = e.Exception;
            while (exc != null)
            {
                sbMessages.AppendFormat("{0} ({1}){2}", exc.Message, exc.GetType().Name, Environment.NewLine);
                exc = exc.InnerException;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Při běhu aplikace Latrunculi došlo k neočekávané výjimce: {0}{0}{1}", Environment.NewLine, sbMessages);

            if (MainWindow != null)
                MessageBox.Show(MainWindow, sb.ToString(), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(sb.ToString(), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Shutdown(-1);
        }
    }
}
