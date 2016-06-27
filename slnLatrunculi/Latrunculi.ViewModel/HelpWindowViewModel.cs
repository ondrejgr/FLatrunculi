using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Latrunculi.ViewModel
{
    public class HelpItem: INotifyPropertyChanged
    {
        public HelpItem(string title, string documentKey)
        {
            Title = title;
            Key = documentKey;
            if (!string.IsNullOrEmpty(documentKey))
                LoadDocument(documentKey);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void LoadDocument(string key)
        {
            FlowDocument doc = Application.Current.TryFindResource(key) as FlowDocument;
            Document = doc;
        }

        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
            private set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        private FlowDocument _document;
        public FlowDocument Document
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                OnPropertyChanged("Document");
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
    }

    public class HelpWindowViewModel : INotifyPropertyChanged
    {
        public HelpWindowViewModel()
        {
            Items.Add(new HelpItem("O hře Latrunculi", "docAbout"));
            Items.Add(new HelpItem("Pravidla hry", "docRules"));
            Items.Add(new HelpItem("Ovládání hry", "docBoardControl"));
            Items.Add(new HelpItem("Nápověda tahu", "docSuggestMove"));
            Items.Add(new HelpItem("Historie tahů", "docHistory"));
            Items.Add(new HelpItem("Vrátit/opakovat tah", "docUndoRedo"));
            Items.Add(new HelpItem("Nastavení hry", "docPlayerSettings"));

            CurrentItem = Items[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public void GoBack()
        {
            if (CurrentItem != null)
                ForwardStack.Push(CurrentItem);
            if (BackStack.Count > 0)
                CurrentItem = BackStack.Pop();
            OnStackChanged();
        }

        public void GoForward()
        {
            if (CurrentItem != null)
                BackStack.Push(CurrentItem);
            if (ForwardStack.Count > 0)
                CurrentItem = ForwardStack.Pop();
            OnStackChanged();
        }

        public void GoTo(HelpItem item)
        {
            if (CurrentItem != null)
                BackStack.Push(CurrentItem);
            CurrentItem = item;
            OnStackChanged();
        }

        private void OnStackChanged()
        {
            OnPropertyChanged("BackEnabled");
            OnPropertyChanged("ForwardEnabled");
            CommandManager.InvalidateRequerySuggested();
        }

        public bool BackEnabled
        {
            get
            {
                return BackStack.Count > 0;
            }
        }

        public bool ForwardEnabled
        {
            get
            {
                return ForwardStack.Count > 0;
            }
        }

        private Stack<HelpItem> _backStack = new Stack<HelpItem>();
        private Stack<HelpItem> BackStack
        {
            get
            {
                return _backStack;
            }
        }

        private Stack<HelpItem> _forwardStack = new Stack<HelpItem>();
        private Stack<HelpItem> ForwardStack
        {
            get
            {
                return _forwardStack;
            }
        }

        private ObservableCollection<HelpItem> _items = new ObservableCollection<HelpItem>();
        public ObservableCollection<HelpItem> Items
        {
            get
            {
                return _items;
            }
        }

        private HelpItem _currentItem;
        public HelpItem CurrentItem
        {
            get
            {
                return _currentItem;
            }
            private set
            {
                _currentItem = value;
                OnPropertyChanged("CurrentItem");
                OnPropertyChanged("CurrentDocument");
            }
        }

        public FlowDocument CurrentDocument
        {
            get
            {
                if (CurrentItem != null)
                    return CurrentItem.Document;
                else
                    return null;
            }
        }
    }
}
