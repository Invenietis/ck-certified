using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CK.Windows.Config;

namespace Host.VM
{
    public class ComboBoxItem : ConfigItem, INotifyPropertyChanged
    {
        public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;
        public event EventHandler<SelectedItemChangingEventArgs> SelectedItemChanging;

        string _selectedItem;

        public ComboBoxItem( ConfigManager configManager, string label, IEnumerable<string> items )
            : base( configManager )
        {
            Items = items;
            Label = label;
        }

        public IEnumerable<string> Items { get; private set; }
        public string Label { get; set; }

        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if( value != _selectedItem )
                {
                    var e = OnSelectedItemChanging( _selectedItem, value );
                    if( !e.Cancel )
                    {
                        string previousItem = _selectedItem;
                        _selectedItem = value;
                        OnSelectedItemChanged( previousItem, _selectedItem );
                        OnPropertyChanged();
                    }
                }
            }
        }

        public string SelectedIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if( handler != null )
            {
                var e = new PropertyChangedEventArgs( propertyName );
                handler( this, e );
            }
        }

        private SelectedItemChangingEventArgs OnSelectedItemChanging( string currentItem, string newItem )
        {
            EventHandler<SelectedItemChangingEventArgs> handler = this.SelectedItemChanging;
            var e = new SelectedItemChangingEventArgs( currentItem, newItem );
            if( handler != null )
            {
                handler( this, e );
            }
            return e;
        }

        private void OnSelectedItemChanged( string previousItem, string newItem )
        {
            EventHandler<SelectedItemChangedEventArgs> handler = this.SelectedItemChanged;
            if( handler != null )
            {
                var e = new SelectedItemChangedEventArgs( previousItem, newItem );
                handler( this, e );
            }
        }
    }

    public class SelectedItemChangingEventArgs : CancelEventArgs
    {
        public string CurrentItem { get; private set; }
        public string NewItem { get; private set; }

        public SelectedItemChangingEventArgs( string currentItem, string newItem )
        {
            CurrentItem = currentItem;
            NewItem = newItem;
        }
    }

    public class SelectedItemChangedEventArgs : EventArgs
    {
        public string PreviousItem { get; private set; }
        public string Item { get; private set; }

        public SelectedItemChangedEventArgs( string previousItem, string item )
        {
            PreviousItem = previousItem;
            Item = item;
        }
    }
}
