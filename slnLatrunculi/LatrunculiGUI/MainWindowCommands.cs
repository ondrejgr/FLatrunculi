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
        static private MainWindowCommand _exit = new MainWindowCommand("_Konec", "Exit", new KeyGesture(Key.F4, ModifierKeys.Alt));
        static public MainWindowCommand Exit
        {
            get
            {
                return _exit;
            }
        }

        static private MainWindowCommand _back = new MainWindowCommand("_Zpět", "Back", null, "Back.png");
        static public MainWindowCommand Back
        {
            get
            {
                return _back;
            }
        }

        static private MainWindowCommand _forward = new MainWindowCommand("_Vpřed", "Forward", null, "Forward.png");
        static public MainWindowCommand Forward
        {
            get
            {
                return _forward;
            }
        }

        static private MainWindowCommand _home = new MainWindowCommand("_Domů", "Home", new KeyGesture(Key.Home, ModifierKeys.Alt), "Home.png");
        static public MainWindowCommand Home
        {
            get
            {
                return _home;
            }
        }

        static private MainWindowCommand _help = new MainWindowCommand("Zobrazit _nápovědu", "Help", new KeyGesture(Key.F1), "Help.png");
        static public MainWindowCommand Help
        {
            get
            {
                return _help;
            }
        }
    }
}
