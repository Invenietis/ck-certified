using BasicCommandHandlers.Resources;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Implementation of the IKeyCommandParameter interface that defines a mouse action (left clic, right clic, pressed left, release left etc..)
    /// </summary>
    public class ClickCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
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
        public string SelectedValue
        {
            get { return _selectedclick; }
            set
            {
                _selectedclick = value;
                OnPropertyChanged( "SelectedValue" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string Title { get { return R.ClickProtocolSubtitle; } }

        public void FillFromString( string parameter )
        {
            string selectedClick = null;
            if( _values.TryGetValue( parameter.Trim(), out selectedClick ) )
            {
                SelectedValue = selectedClick;
            }
            else SelectedValue = null;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return !String.IsNullOrWhiteSpace( SelectedValue ); } }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedValue ) ) return String.Empty;
            return _values.Where( x => x.Value == SelectedValue ).Single().Key;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
