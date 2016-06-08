using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi.ViewModel
{
    public enum PlayerTypes { ptHuman, ptComputer };
    public enum PlayerColors { pcWhite, pcBlack };
    public enum PlayerLevels { plEasy, plMedium, plHard };

    public class PlayerViewModel : INotifyPropertyChanged
    {
        public PlayerViewModel(PlayerColors color)
        {
            Color = color;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private PlayerColors _color;
        public PlayerColors Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        private PlayerTypes _playerType = PlayerTypes.ptHuman;
        public PlayerTypes PlayerType
        {
            get
            {
                return _playerType;
            }
            set
            {
                _playerType = value;
                OnPropertyChanged("PlayerType");
            }
        }

        private PlayerLevels _level;
        public PlayerLevels Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                OnPropertyChanged("Level");
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        
        public void RefreshFromModel(Model.PlayerSettings.Player player)
        {
            Model.PlayerSettings.PlayerInfo info;

            if (player.IsComputerPlayer)
            {
                PlayerType = PlayerTypes.ptComputer;
                info = (player as Model.PlayerSettings.Player.ComputerPlayer).Item;
            }
            else if (player.IsHumanPlayer)
            {
                PlayerType = PlayerTypes.ptHuman;
                info = (player as Model.PlayerSettings.Player.HumanPlayer).Item;
            }
            else
                throw new NotImplementedException();

            Name = info.Name;
            Level = (PlayerLevels)(int)info.Level;
        }
    }
}
