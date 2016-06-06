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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Latrunculi.GUI.Controls
{
    public class PieceDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is BoardSquareViewModel)
            {
                BoardSquareViewModel sq = (BoardSquareViewModel)item;
                string dtName = sq.PieceType.ToString();

                return (element.FindResource(typeof(Piece)) as Style).Resources[dtName] as DataTemplate;
            }

            return null;
        }
    }

    public class Piece : ContentPresenter
    {
        static Piece()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Piece), new FrameworkPropertyMetadata(typeof(Piece)));
        }
    }
}
