using CK.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProtocolManagerModel
{

    public class VMProtocolEditorsProvider : INotifyPropertyChanged
    {
        public IEnumerable<VMProtocolEditorWrapper> AvailableProtocolEditors { get { return _availableProtocolEditors.Values; } }

        private Dictionary<string, VMProtocolEditorWrapper> _availableProtocolEditors { get; set; }
        private Dictionary<string, Type> _availableProtocolHandlers { get; set; }

        public VMProtocolEditorsProvider()
        {
            _availableProtocolEditors = new Dictionary<string, VMProtocolEditorWrapper>();
            _availableProtocolHandlers = new Dictionary<string, Type>();
            ProtocolEditor = new VMProtocolEditor();
        }

        public Type GetHandlingService( string protocol )
        {
            Type handlingService = null;
            _availableProtocolHandlers.TryGetValue( protocol, out handlingService );
            return handlingService;
        }

        public void AddEditor( string protocol, VMProtocolEditorWrapper wrapper, Type handlingService )
        {
            _availableProtocolEditors.Add( protocol, wrapper );
            _availableProtocolHandlers.Add( protocol, handlingService );
            OnPropertyChanged( "AvailableProtocolEditors" );
        }

        public void RemoveEditor( string protocol )
        {
            if( _availableProtocolEditors.ContainsKey( protocol ) )
            {
                _availableProtocolEditors.Remove( protocol );
                _availableProtocolHandlers.Remove( protocol );
                OnPropertyChanged( "AvailableProtocolEditors" );
            }
        }

        public VMProtocolEditorWrapper SelectedProtocolEditorWrapper
        {
            get { return ProtocolEditor.Wrapper; }
            set
            {
                ProtocolEditor.Wrapper = value;

                //Creating the ParameterMaanger on the fly
                if( value != null ) ProtocolEditor.ParameterManager = ProtocolEditor.Wrapper.CreateParameterManager();

                OnPropertyChanged( "ProtocolEditor" );
                OnPropertyChanged( "SelectedProtocolEditorWrapper" );
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        VMProtocolEditor _vmProtocolEditor;
        /// <summary>
        /// The KeyCommand currently displayed and used in the editor.
        /// </summary>
        public VMProtocolEditor ProtocolEditor
        {
            get { return _vmProtocolEditor; }
            set
            {
                _vmProtocolEditor = value;
                OnPropertyChanged( "ProtocolEditor" );
            }
        }

        public void FlushCurrentProtocolEditor()
        {
            SelectedProtocolEditorWrapper = null;
            InitializeProtocolEditor();
        }

        public void InitializeProtocolEditor()
        {
            ProtocolEditor = new VMProtocolEditor();
        }

        public void CreateKeyCommand( string keyCommand )
        {
            ProtocolEditor = new VMProtocolEditor();

            //not using the Split method in order to let a parameter use the ':' char
            int idx = keyCommand.IndexOf( ':' );
            string protocol = keyCommand.Substring( 0, idx );
            string parameter = keyCommand.Substring( idx + 1 );

            ProtocolEditor.Wrapper = GetProtocolEditorWrapper( protocol );
            SelectedProtocolEditorWrapper = ProtocolEditor.Wrapper;
            if( ProtocolEditor.Wrapper.IsValid )
            {
                ProtocolEditor.ParameterManager = SelectedProtocolEditorWrapper.CreateParameterManager();
                if( ProtocolEditor.ParameterManager == null ) throw new ArgumentNullException( String.Format( "Null value retrieved while trying to retrieve the IKeyCommandParameterManager for the KeyCommandTypeViewModel handling the protocol '{0}'", protocol ) );

                ProtocolEditor.ParameterManager.FillFromString( parameter );
            }
            OnPropertyChanged( "ProtocolEditor" );
        }

        /// <summary>
        /// Gets the <see cref="VMProtocolEditorWrapper"/> for the specified protocol.
        /// If the protocol is not handled, returns an empty <see cref="VMProtocolEditorWrapper"/>, with a IsValid property returning false
        /// </summary>
        /// <param name="protocol">The protocol (ex : sendString, sendKey, keyboardswitch...)</param>
        /// <returns>The <see cref="VMProtocolEditorWrapper"/> corresponding to the protocol set as parameter. if the returned object's IsValid property returns false, the protocol is not handled</returns>
        public VMProtocolEditorWrapper GetProtocolEditorWrapper( string protocol )
        {
            //If the protocol is not recognized, we'll add an Invalid KeyCommandType.
            VMProtocolEditorWrapper editorWrapper = new VMProtocolEditorWrapper( protocol, protocol );
            _availableProtocolEditors.TryGetValue( protocol, out editorWrapper );
            return editorWrapper;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
