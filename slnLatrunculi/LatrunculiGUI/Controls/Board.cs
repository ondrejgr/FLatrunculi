﻿using System;
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
    public class Board : ItemsControl
    {
        static Board()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Board), new FrameworkPropertyMetadata(typeof(Board)));
        }

        public int SquareSize
        {
            get { return (int)GetValue(SquareSizeProperty); }
            set { SetValue(SquareSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SquareSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareSizeProperty =
            DependencyProperty.Register("SquareSize", typeof(int), typeof(Board), new PropertyMetadata(24));
    }
}