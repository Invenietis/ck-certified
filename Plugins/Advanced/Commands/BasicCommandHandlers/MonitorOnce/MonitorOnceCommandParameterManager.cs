using BasicCommandHandlers.Resources;
using CK.Plugins.SendInputDriver;
using CK.WPF.ViewModel;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BasicCommandHandlers
{
    public class MonitorOnceCommandParameterManager : VMProtocolEditorsProvider, IProtocolParameterManager
    {
        public MonitorOnceCommandParameterManager()
        {
            _actions = new Dictionary<string, string>();
            _actions.Add( R.KeySent, "sendkey" );

            _saveCommand = new VMCommand( () =>
            {
                _innerCommand = ProtocolEditor.ToString();
                OnPropertyChanged( "IsValid" );
            } );
            _cancelCommand = new VMCommand( () => { _innerCommand = String.Empty; OnPropertyChanged( "IsValid" ); } );
        }

        IProtocolEditorRoot _root;
        public IProtocolEditorRoot Root
        {
            get { return _root; }
            set
            {
                _root = value;
                Initialize();
            }
        }

        private string _innerCommand;

        ICommand _saveCommand;
        public ICommand SaveCommandCommand { get { return _saveCommand; } }

        ICommand _cancelCommand;
        public ICommand CancelChangesCommand { get { return _cancelCommand; } }

        #region Action-to-listen-to selection

        Dictionary<string, string> _actions;
        public IEnumerable<string> AvailableActions { get { return _actions.Keys; } }

        string _selectedAction;
        public string SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _innerCommand = String.Empty;
                _selectedAction = value;
                OnPropertyChanged( "SelectedAction" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string ActionsTitle { get { return R.ActionToListenTo; } }

        //TODO : CHECK : this step doesn't seem necessary
        public string NameTitle { get { return R.GiveName; } }
        public string Name { get; set; }

        private void Initialize()
        {
            _availableProtocolEditors = new Dictionary<string, VMProtocolEditorWrapper>();

            foreach( var item in Root.AvailableProtocolEditors )
            {
                _availableProtocolEditors.Add( item.Protocol, item );
            }

            OnPropertyChanged( "AvailableProtocolEditors" );
        }

        public string InnerActionTitle { get { return R.MonitorOnceInnerActionTitle; } }

        public void FillFromString( string parameter )
        {
            Initialize();

            string[] splittedParameter = parameter.Split( ',' );

            SelectedAction = _actions.Where( kvp => kvp.Value == splittedParameter[0] ).Single().Key;
            Name = splittedParameter[1];
            string innerCommand = parameter.Substring( splittedParameter[0].Length + splittedParameter[1].Length + 2 );
            string innerCommandProtocol = innerCommand.Substring( innerCommand.IndexOf( ':' ) + 1 );

            CreateKeyCommand( innerCommand, Root.EditedKeyMode );

        }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace( Name )
                    && !String.IsNullOrWhiteSpace( SelectedAction )
                    && ProtocolEditor != null && ProtocolEditor.IsValid
                    && !String.IsNullOrWhiteSpace( _innerCommand );
            }
        }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedAction ) ) return String.Empty;
            return String.Format( "{0},{1},{2}", _actions[SelectedAction], Name, _innerCommand );
        }

        #endregion

        protected override void OnChildPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsValid" )
            {
                _innerCommand = ProtocolEditor != null ? ProtocolEditor.ToString() : String.Empty;
                OnPropertyChanged( "IsValid" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public new event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
