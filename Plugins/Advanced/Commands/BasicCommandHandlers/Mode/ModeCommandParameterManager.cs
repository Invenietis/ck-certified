using BasicCommandHandlers.Resources;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicCommandHandlers
{
    /// <summary>
    /// IProtocolParameterManager implementation that handles the Mode actions.
    /// syntax example : mode:set,shift
    /// This example removes all other modes and sets the shift mode as the current mode.
    /// </summary>
    public class ModeCommandParameterManager : IProtocolParameterManager
    {
        public IProtocolEditorRoot Root { get; set; }
        public ModeCommandParameterManager()
        {
            _actions = new Dictionary<string, string>();

            _actions.Add( R.RemoveMode, "remove" );
            _actions.Add( R.ToggleMode, "toggle" );
            _actions.Add( R.AddMode, "add" );
            _actions.Add( R.SetMode, "set" );
        }

        Dictionary<string, string> _actions;
        public IEnumerable<string> AvailableActions { get { return _actions.Keys; } }

        public IEnumerable<string> AvailableModes { get { return Root.EditedKeyMode.Keyboard.AvailableMode.AtomicModes.Select( m => m.ToString() ); } }

        string _selectedAction;
        public string SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _selectedAction = value;
                OnPropertyChanged( "SelectedAction" );
                OnPropertyChanged( "IsValid" );
            }
        }

        string _selectedMode;
        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                _selectedMode = value;
                OnPropertyChanged( "SelectedMode" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string ActionsTitle { get { return R.ModeProtocolActionsSubtitle; } }
        public string ModesTitle { get { return R.ModeProtocolModesSubtitle; } }

        public void FillFromString( string parameter )
        {
            string[] splittedParameter = parameter.Split( ',' );

            SelectedAction = _actions.Where( kvp => kvp.Value == splittedParameter[0] ).FirstOrDefault().Key;
            if( String.IsNullOrWhiteSpace( SelectedAction ) ) SelectedAction = String.Empty;

            SelectedMode = AvailableModes.Contains( splittedParameter[1] ) ? splittedParameter[1] : String.Empty;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return _actions.ContainsKey( SelectedAction ) && !String.IsNullOrWhiteSpace( SelectedMode ); } }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedAction ) || !_actions.ContainsKey( SelectedAction ) ) return String.Empty;
            return String.Format( "{0},{1}", _actions[SelectedAction], SelectedMode );
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
