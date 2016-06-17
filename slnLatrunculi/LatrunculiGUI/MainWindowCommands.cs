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
        static private MainWindowCommand _exit = new MainWindowCommand("_Konec", "Exit", new KeyGesture(Key.F4, ModifierKeys.Alt), true);
        static public MainWindowCommand Exit
        {
            get
            {
                return _exit;
            }
        }

        static private MainWindowCommand _back = new MainWindowCommand("_Zpět", "Back");
        static public MainWindowCommand Back
        {
            get
            {
                return _back;
            }
        }

        static private MainWindowCommand _forward = new MainWindowCommand("_Vpřed", "Forward");
        static public MainWindowCommand Forward
        {
            get
            {
                return _forward;
            }
        }

        static private MainWindowCommand _home = new MainWindowCommand("_Domů", "Home", new KeyGesture(Key.Home, ModifierKeys.Alt));
        static public MainWindowCommand Home
        {
            get
            {
                return _home;
            }
        }

        static private MainWindowCommand _help = new MainWindowCommand("Zobrazit _nápovědu", "Help", new KeyGesture(Key.F1));
        static public MainWindowCommand Help
        {
            get
            {
                return _help;
            }
        }

        static private MainWindowCommand _navigate = new MainWindowCommand("Zobrazit _nápovědu", "Navigate", null, true);
        static public MainWindowCommand Navigate
        {
            get
            {
                return _navigate;
            }
        }

        static private MainWindowCommand _settings = new MainWindowCommand("_Nastavení hry", "Settings", new KeyGesture(Key.F4));
        static public MainWindowCommand Settings
        {
            get
            {
                return _settings;
            }
        }

        static private MainWindowCommand _load = new MainWindowCommand("N_ačíst hru", "Load", new KeyGesture(Key.O, ModifierKeys.Control));
        static public MainWindowCommand Load
        {
            get
            {
                return _load;
            }
        }

        static private MainWindowCommand _save = new MainWindowCommand("_Uložit hru", "Save", new KeyGesture(Key.S, ModifierKeys.Control));
        static public MainWindowCommand Save
        {
            get
            {
                return _save;
            }
        }

        static private MainWindowCommand _saveAs = new MainWindowCommand("_Uložit jako...", "SaveAs");
        static public MainWindowCommand SaveAs
        {
            get
            {
                return _saveAs;
            }
        }

        static private MainWindowCommand _new = new MainWindowCommand("_Nová hra", "New", new KeyGesture(Key.N, ModifierKeys.Control));
        static public MainWindowCommand New
        {
            get
            {
                return _new;
            }
        }

        static private MainWindowCommand _resume = new MainWindowCommand("Pok_račovat", "Resume", new KeyGesture(Key.F5));
        static public MainWindowCommand Resume
        {
            get
            {
                return _resume;
            }
        }

        static private MainWindowCommand _pause = new MainWindowCommand("Poza_stavit", "Pause", new KeyGesture(Key.F5, ModifierKeys.Shift));
        static public MainWindowCommand Pause
        {
            get
            {
                return _pause;
            }
        }

        static private MainWindowCommand _suggestMove = new MainWindowCommand("_Napovědět tah", "SuggestMove", new KeyGesture(Key.F1, ModifierKeys.Shift));
        static public MainWindowCommand SuggestMove
        {
            get
            {
                return _suggestMove;
            }
        }

        static private MainWindowCommand _cancelSuggestMove = new MainWindowCommand("_Zrušit nápovědu tahu", "CancelSuggestMove", null, true);
        static public MainWindowCommand CancelSuggestMove
        {
            get
            {
                return _cancelSuggestMove;
            }
        }
    }
}
