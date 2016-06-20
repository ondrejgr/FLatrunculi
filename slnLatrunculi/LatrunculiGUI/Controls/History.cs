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
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Latrunculi.GUI.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Latrunculi.GUI.Controls;assembly=Latrunculi.GUI.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:History/>
    ///
    /// </summary>
    public class History : ListBox
    {
        static History()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(History), new FrameworkPropertyMetadata(typeof(History)));
        }

        /// <summary> Create or identify the element used to display the given item. </summary>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new HistoryItem();
        }

        /// <summary>
        /// Return true if the item is (or is eligible to be) its own ItemContainer
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is HistoryItem);
        }

        public bool ShowHistory
        {
            get { return (bool)GetValue(ShowHistoryProperty); }
            set { SetValue(ShowHistoryProperty, value); }
        }
        public static readonly DependencyProperty ShowHistoryProperty =
            DependencyProperty.Register("ShowHistory", typeof(bool), typeof(History), new PropertyMetadata(false));


    }
}
