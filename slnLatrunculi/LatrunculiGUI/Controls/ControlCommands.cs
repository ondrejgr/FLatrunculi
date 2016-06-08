using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Latrunculi.GUI.Controls
{
    static public class ControlCommands
    {
        static private RoutedCommand _boardSquareClick = new RoutedCommand("BoardSquareClick", typeof(Square));
        static public RoutedCommand BoardSquareClick
        {
            get
            {
                return _boardSquareClick;
            }
        }
    }
}
