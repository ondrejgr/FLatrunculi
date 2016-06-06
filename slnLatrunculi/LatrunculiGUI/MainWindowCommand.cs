using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Latrunculi.GUI
{
    public class MainWindowCommand: RoutedUICommand
    {
        public MainWindowCommand(string text, string name, InputGesture gesture): base(text, name, typeof(MainWindow))
        {
            if (gesture != null)
                InputGestures.Add(gesture);
        }
    }
}
