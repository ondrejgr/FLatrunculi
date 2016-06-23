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
            private set
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
                OnPropertyChanged("IsHuman");
            }
        }

        public bool IsHuman
        {
            get
            {
                return PlayerType == PlayerTypes.ptHuman;
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
            Model.Player.Types pt = Model.Player.getPlayerType(player);
            if (pt == Model.Player.Types.Computer)
                PlayerType = PlayerTypes.ptComputer;
            else if (pt == Model.Player.Types.Human)
                PlayerType = PlayerTypes.ptHuman;
            else
                throw new NotSupportedException();

            Name = player.Name;

            Model.Player.Levels lev = player.Level;
            if (lev == Model.Player.Levels.Easy)
                Level = PlayerLevels.plEasy;
            else if (lev == Model.Player.Levels.Medium)
                Level = PlayerLevels.plMedium;
            else if (lev == Model.Player.Levels.Hard)
                Level = PlayerLevels.plHard;
            else
                throw new NotSupportedException();

            if (player.Color == Model.Piece.Colors.Black)
                Color = PlayerColors.pcBlack;
            else if (player.Color == Model.Piece.Colors.White)
                Color = PlayerColors.pcWhite;
            else
                throw new NotSupportedException();
        }

        public object Clone()
        {
            PlayerViewModel result = new PlayerViewModel(Color);
            result.PlayerType = PlayerType;
            result.Level = Level;
            result.Name = Name;
            return result;
        }

        static public Model.Player.Types PlayerTypeToModel(PlayerViewModel vm)
        {
            if (vm.PlayerType == PlayerTypes.ptComputer)
                return Model.Player.Types.Computer;
            else if (vm.PlayerType == PlayerTypes.ptHuman)
                return Model.Player.Types.Human;
            else
                throw new NotSupportedException();
        }

        static public Model.Player.Levels PlayerLevelToModel(PlayerViewModel vm)
        {
            switch (vm.Level)
            {
                case PlayerLevels.plEasy:
                    return Model.Player.Levels.Easy;
                case PlayerLevels.plMedium:
                    return Model.Player.Levels.Medium;
                case PlayerLevels.plHard:
                    return Model.Player.Levels.Hard;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    
}
