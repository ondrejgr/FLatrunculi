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

    public class PlayerViewModel : INotifyPropertyChanged, ICloneable
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

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
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
        
        public void RefreshFromModel(Model.Player.T player)
        {
            if (Model.Player.isComputer(player))
                PlayerType = PlayerTypes.ptComputer;
            else if (Model.Player.isHuman(player))
                PlayerType = PlayerTypes.ptHuman;
            else
                throw new NotImplementedException();

            Name = player.Name;
            Level = (PlayerLevels)player.Level;
        }

        public Model.Player.T GetPlayerForModel()
        {
            if (PlayerType == PlayerTypes.ptComputer)
                return Model.Player.createComputerPlayer(Name, (Model.Player.Levels)Level);
            else if (PlayerType == PlayerTypes.ptHuman)
                return Model.Player.createHumanPlayer(Name, (Model.Player.Levels)Level);
            else
                throw new NotImplementedException();
        }

        public object Clone()
        {
            PlayerViewModel result = new PlayerViewModel(Color);
            result.PlayerType = PlayerType;
            result.Level = Level;
            result.Name = Name;
            return result;
        }
    }
}
