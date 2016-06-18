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
    public class Square : Button
    {
        static Square()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Square), new FrameworkPropertyMetadata(typeof(Square)));
        }



        private Board Board;

        private static Board GetBoard(DependencyObject child)
        {
            Board result = null;
            if (child == null)
                return result;

            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            while (parentObject != null)
            {
                if (parentObject is Board)
                {
                    result = (Board)parentObject;
                    break;
                }
                else
                    parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return result;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            Board = GetBoard(this);
        }

        private void UpdateMouseOverState()
        {
            bool isActive = (Board != null) && Board.IsActive && (DataContext is ViewModel.BoardSquareViewModel);
            if (IsMouseOver && isActive)
                VisualStateManager.GoToState(this, "Active", true);
            else
                VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateMouseOverState();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateMouseOverState();
        }
    }
}
