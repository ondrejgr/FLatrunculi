using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Latrunculi.GUI
{
    static class MainWindowCommands
    {
        static private RoutedUICommand _exit = new MainWindowCommand("_Konec", "Exit", new KeyGesture(Key.F4, ModifierKeys.Alt));
        static public RoutedUICommand Exit
        {
            get
            {
                return _exit;
            }
        }
    }
}
