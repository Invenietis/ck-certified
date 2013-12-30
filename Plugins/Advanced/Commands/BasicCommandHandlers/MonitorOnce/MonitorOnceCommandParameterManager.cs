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
            //TODO : use resx
            _actions = new Dictionary<string, string>();
            _actions.Add( "Lancement d'une touche", "sendkey" );

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

        //TODO : use resx
        public string ActionsTitle { get { return "Action to listen to : "; } }

        //TODO : use resx -- TODO : CHECK : this step doesn't seem necessary
        public string NameTitle { get { return "Give a name to this action : "; } }
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

        public string Title { get { return R.SendKeyProtocolSubtitle; } }

        public void FillFromString( string parameter )
        {
            Initialize();

            string[] splittedParameter = parameter.Split( ',' );

            SelectedAction = _actions.Where( kvp => kvp.Value == splittedParameter[0] ).Single().Key;
            Name = splittedParameter[1];
            string innerCommand = parameter.Substring( splittedParameter[0].Length + splittedParameter[1].Length + 2 );
            string innerCommandProtocol = innerCommand.Substring( innerCommand.IndexOf( ':' ) + 1 );

            CreateKeyCommand( innerCommand );

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
