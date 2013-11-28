using BasicCommandHandlers.Resources;
using CK.Keyboard.Model;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Implementation of the IKeyCommandParameter interface that defines a mouse action (left clic, right clic, pressed left, release left etc..)
    /// </summary>
    public class ClickCommandParameterManager : IProtocolParameterManager
    {
        public ClickCommandParameterManager()
        {
            _values = new Dictionary<string, string>() 
            { 
                { "simple", R.LeftClick },
                { "right", R.RightClick },
                { "double", R.DoubleLeftClick }, 
                { "leftpush", R.PushLeft }, 
                { "leftrelease", R.ReleaseLeft }
            };
        }

        IDictionary<string, string> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Values; } }

        string _selectedclick;
        public string SelectedClick
        {
            get { return _selectedclick; }
            set
            {
                _selectedclick = value;
                OnPropertyChanged( "SelectedClick" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public void FillFromString( string parameter )
        {
            string selectedClick = null;
            if( _values.TryGetValue( parameter.Trim(), out selectedClick ) )
            {
                SelectedClick = selectedClick;
            }
            else SelectedClick = null;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return !String.IsNullOrWhiteSpace( SelectedClick ); } }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedClick ) ) return String.Empty;
            return _values.Where( x => x.Value == SelectedClick ).Single().Key;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
