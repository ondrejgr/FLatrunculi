using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private PlayerSettingsViewModel _blackPlayer = new PlayerSettingsViewModel(PlayerColors.pcBlack);
        public PlayerSettingsViewModel BlackPlayer
        {
            get
            {
                return _blackPlayer;
            }
        }

        private PlayerSettingsViewModel _whitePlayer = new PlayerSettingsViewModel(PlayerColors.pcWhite);
        public PlayerSettingsViewModel WhitePlayer
        {
            get
            {
                return _whitePlayer;
            }
        }
    }
}
