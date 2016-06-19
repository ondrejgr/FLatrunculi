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
using System.Windows.Media.Animation;
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


        private ViewModel.BoardSquareViewModel ViewModel
        {
            get
            {
                return DataContext as ViewModel.BoardSquareViewModel;
            }
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
            if (IsMouseOver && isActive && ViewModel.ValidMoveExists)
                VisualStateManager.GoToState(this, "Active", true);
            else
                VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            UpdateMouseOverState();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            UpdateMouseOverState();
            base.OnMouseLeave(e);
        }

        public void BlinkRed()
        {
            Storyboard sb = FindResource("blinkRedBorderAnimation") as Storyboard;
            DependencyObject x = GetTemplateChild("bgSelected");
            if (sb != null && x != null)
            {
                sb.Stop();
                Storyboard.SetTarget(sb, x);
                sb.Begin();
            }
        }
    }
}
