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
    }
}
