using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Latrunculi.GUI
{
    public class MainWindowCommand: RoutedUICommand
    {
        public MainWindowCommand(string text, string name, InputGesture gesture=null, bool noImage=false): base(text, name, typeof(MainWindow))
        {
            if (gesture != null)
                InputGestures.Add(gesture);
            if (!noImage && !string.IsNullOrEmpty(name))
            {
                string packUri = string.Format("pack://application:,,,/CommandIcons/{0}.png", name);
                ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            }
        }

        private ImageSource imageSource;
        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                imageSource = value;
            }
        }
    }
}
