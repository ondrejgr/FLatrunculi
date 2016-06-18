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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Latrunculi.GUI.Controls
{
    public class Piece : Control
    {
        static Piece()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Piece), new FrameworkPropertyMetadata(typeof(Piece)));
        }
    }
}
