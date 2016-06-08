using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public class PlayerSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private PlayerViewModel _blackPlayer = new PlayerViewModel(PlayerColors.pcBlack);
        public PlayerViewModel BlackPlayer
        {
            get
            {
                return _blackPlayer;
            }
        }

        private PlayerViewModel _whitePlayer = new PlayerViewModel(PlayerColors.pcWhite);
        public PlayerViewModel WhitePlayer
        {
            get
            {
                return _whitePlayer;
            }
        }

        public void RefreshFromModel(Model.PlayerSettings.T model)
        {
            BlackPlayer.RefreshFromModel(model.BlackPlayer);
            WhitePlayer.RefreshFromModel(model.WhitePlayer);
        }
    }
}
