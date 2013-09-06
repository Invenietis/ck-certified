using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using KeyboardEditor.Resources;
using KeyboardEditor.ViewModels;
using Microsoft.Win32;
using KeyboardEditor.KeyboardEdition;
using CK.Windows.App;

namespace KeyboardEditor.ViewModels
{
    public class VMKeyModeEditable : VMContextElementEditable, IModeViewModel
    {
        IKeyMode _model;

        public VMKeyModeEditable( VMContextEditable context, IKeyMode model )
            : base( context )
        {
            _model = model;
            _commands = new ObservableCollection<string>();

            foreach( var cmd in _model.OnKeyDownCommands.Commands )
            {
                _commands.Add( cmd );
            }

            foreach( var cmd in _model.OnKeyUpCommands.Commands )
            {
                _commands.Add( cmd );
            }

            foreach( var cmd in _model.OnKeyPressedCommands.Commands )
            {
                _commands.Add( cmd );
            }

            //TODO : implement a registering behavior
            _keyCommandTypeProvider = new KeyCommandProviderViewModel();

            RegisterEvents();

        }

        #region Properties

        //COMMON
        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// This override checks the mode of the actual parent keyboard, instead of getting the current keyboard's mode
        /// </summary>
        public bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardVM.CurrentMode;
                return !keyboardMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( keyboardMode );
            }
        }

        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string UpLabel
        {
            get { return _model.UpLabel; }
            set { _model.UpLabel = value; }
        }

        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string DownLabel
        {
            get { return _model.DownLabel; }
            set { _model.DownLabel = value; }
        }

        ///Gets the Description of the underling <see cref="IKeyMode"/>
        public string Description
        {
            get { return _model.Description; }
            set { _model.Description = value; }
        }

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return _model.Enabled; } }

        //COMMON
        public bool IsCurrent { get { return _model.IsCurrent; } }

        //COMMON
        public bool IsEmpty { get { return _model.Mode.IsEmpty; } }

        //COMMON
        public string Name { get { return String.IsNullOrWhiteSpace( _model.Mode.ToString() ) ? R.DefaultMode : _model.Mode.ToString(); } }

        /// <summary>
        /// Gets whether the element is selected.
        /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMKeyModeEditable
        /// </summary>
        public override bool IsSelected
        {
            get
            {
                return Parent.IsSelected
                    && ActualParent.CurrentKeyModeModeVM.Mode.ContainsAll( _model.Mode )
                    && _model.Mode.ContainsAll( ActualParent.CurrentKeyModeModeVM.Mode )
                    && Context.CurrentlyDisplayedModeType == ModeTypes.Mode;
            }
            set
            {
                VMKeyModeEditable previousKeyMode = null;
                VMLayoutKeyModeEditable previousLayoutKeyMode = null;

                if( value && Context.SelectedElement is VMKeyEditable )
                {
                    previousKeyMode = ( Context.SelectedElement as VMKeyEditable ).KeyModeVM;
                    previousLayoutKeyMode = ( Context.SelectedElement as VMKeyEditable ).LayoutKeyModeVM;
                }

                Context.CurrentlyDisplayedModeType = ModeTypes.Mode;
                Context.KeyboardVM.CurrentMode = _model.Mode;

                if( value ) Parent.IsSelected = value;

                if( previousKeyMode != null && previousLayoutKeyMode != null )
                {
                    previousKeyMode.TriggerPropertyChanged( "IsSelected" );
                    previousLayoutKeyMode.TriggerPropertyChanged( "IsSelected" );
                }

                ( Context.SelectedElement as VMKeyEditable ).KeyModeVM.TriggerPropertyChanged( "IsSelected" );
                ( Context.SelectedElement as VMKeyEditable ).LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );
            }
        }

        //COMMON
        /// <summary>
        /// Returns this VMKeyModeEditable's parent's layout element
        /// </summary>
        public override CK.Keyboard.Model.IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

        //COMMON
        VMContextElementEditable _parent;
        /// <summary>
        /// Returns this VMKeyModeEditable's parent
        /// </summary>
        public override VMContextElementEditable Parent
        {
            get
            {
                if( _parent == null ) _parent = Context.Obtain( _model.Key );
                return _parent;
            }
        }

        //COMMON
        private VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

        //COMMON
        private IEnumerable<VMContextElementEditable> GetParents()
        {
            VMContextElementEditable elem = this;
            while( elem != null )
            {
                elem = elem.Parent;

                if( elem != null )
                    yield return elem;
            }
        }

        #endregion

        #region KeyPrograms

        const string vmCollectionOutOfRangeErrorMessage = "The index of the command that has been {0} is out of range in the corresponding viewmodel's Command collection";
        const string ownCollectionOutOfRangeErrorMessage = "The index of the command that has been {0} is out of range in the viewmodel's own Command collection";

        void OnKeyDownCommands_CommandUpdated( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "updated" ) );
            if( _model.OnKeyDownCommands.Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "updated" ) );

            Commands[e.Index] = e.KeyProgram.Commands[e.Index];
        }

        void OnKeyDownCommands_CommandsCleared( object sender, KeyProgramCommandsEventArgs e )
        {
            Commands.Clear();
        }

        void OnKeyDownCommands_CommandInserted( object sender, KeyProgramCommandsEventArgs e )
        {
            if( _model.OnKeyDownCommands.Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "inserted" ) );
            Commands.Insert( e.Index, e.KeyProgram.Commands[e.Index] );
        }

        void OnKeyDownCommands_CommandDeleted( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "deleted" ) );
            Commands.RemoveAt( e.Index );
        }

        ObservableCollection<string> _commands;
        public ObservableCollection<string> Commands { get { return _commands; } }

        bool _showKeyCommandCreationPanel;
        public bool ShowKeyCommandCreationPanel
        {
            get { return _showKeyCommandCreationPanel; }
            set
            {
                _showKeyCommandCreationPanel = value;
                OnPropertyChanged( "ShowKeyCommandCreationPanel" );
            }
        }

        VMCommand _initializeCommand;
        public VMCommand InitializeCommandCommand
        {
            get
            {
                if( _initializeCommand == null )
                {
                    _initializeCommand = new VMCommand( () =>
                    {
                        KeyCommandTypeProvider.InitializeKeyCommand();
                        ShowKeyCommandCreationPanel = true;
                    } );
                }

                return _initializeCommand;
            }
        }

        VMCommand _saveCommand;
        public VMCommand SaveCommandCommand
        {
            get
            {
                if( _saveCommand == null )
                {
                    _saveCommand = new VMCommand( () =>
                    {
                        DoAddKeyCommand( KeyCommandTypeProvider.KeyCommand.ToString() );
                        ShowKeyCommandCreationPanel = false;

                    } );
                }

                return _saveCommand;
            }
        }

        private void DoAddKeyCommand( string keyCommand )
        {
            _model.OnKeyDownCommands.Commands.Add( keyCommand );
            KeyCommandTypeProvider.FlushCurrentKeyCommand();
        }

        string _commandBeingChanged = String.Empty;
        VMCommand<string> _changeCommand;
        public VMCommand<string> ChangeCommandCommand
        {
            get
            {
                if( _changeCommand == null )
                {
                    _changeCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        _commandBeingChanged = cmdString;
                        DoRemoveKeyCommand( cmdString );
                        KeyCommandTypeProvider.CreateKeyCommand( cmdString );
                        ShowKeyCommandCreationPanel = true;
                    } );
                }

                return _changeCommand;
            }
        }

        VMCommand _cancelCommand;
        public VMCommand CancelChangesCommand
        {
            get
            {
                if( _cancelCommand == null )
                {
                    _cancelCommand = new VMCommand( () =>
                    {
                        if( !String.IsNullOrEmpty( _commandBeingChanged ) )
                        {
                            DoAddKeyCommand( _commandBeingChanged );
                            _commandBeingChanged = String.Empty;
                        }
                        else
                        {
                            KeyCommandTypeProvider.FlushCurrentKeyCommand();
                        }
                        ShowKeyCommandCreationPanel = false;
                    } );
                }

                return _cancelCommand;
            }
        }

        VMCommand<string> _removeCommand;
        public VMCommand<string> RemoveCommandCommand
        {
            get
            {
                if( _removeCommand == null )
                {
                    _removeCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        DoRemoveKeyCommand( cmdString );
                    } );
                }

                return _removeCommand;
            }
        }

        private void DoRemoveKeyCommand( string cmdString )
        {
            Debug.Assert( _model.OnKeyDownCommands.Commands.Contains( cmdString )
                || _model.OnKeyUpCommands.Commands.Contains( cmdString )
                || _model.OnKeyPressedCommands.Commands.Contains( cmdString ) );

            if( _model.OnKeyDownCommands.Commands.Contains( cmdString ) )
                _model.OnKeyDownCommands.Commands.Remove( cmdString );
            else if( _model.OnKeyUpCommands.Commands.Contains( cmdString ) )
                _model.OnKeyDownCommands.Commands.Remove( cmdString );
            else if( _model.OnKeyPressedCommands.Commands.Remove( cmdString ) )
                _model.OnKeyDownCommands.Commands.Remove( cmdString );
            else
                throw new ArgumentException( "Trying to remove a command that cannot be found in the key commands. Key : " + _model.UpLabel + ", command : " + cmdString );
        }

        KeyCommandProviderViewModel _keyCommandTypeProvider;
        public KeyCommandProviderViewModel KeyCommandTypeProvider
        {
            get { return _keyCommandTypeProvider; }
            set
            {
                _keyCommandTypeProvider = value;
                OnPropertyChanged( "KeyCommandTypeProvider" );
            }
        }


        VMCommand _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new VMCommand( () =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        VMKeyEditable parent = ActualParent; //Keeping a ref to the parent, since the model will be detached from its parent when destroyed
                        _model.Destroy();
                        parent.RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }


        #endregion

        #region OnXXX

        internal override void Dispose()
        {
            UnregisterEvents();
            base.Dispose();
        }

        private void RegisterEvents()
        {
            _model.OnKeyDownCommands.CommandInserted += OnKeyDownCommands_CommandInserted;
            _model.OnKeyDownCommands.CommandsCleared += OnKeyDownCommands_CommandsCleared;
            _model.OnKeyDownCommands.CommandDeleted += OnKeyDownCommands_CommandDeleted;
            _model.OnKeyDownCommands.CommandUpdated += OnKeyDownCommands_CommandUpdated;
        }

        private void UnregisterEvents()
        {
            _model.OnKeyDownCommands.CommandInserted -= OnKeyDownCommands_CommandInserted;
            _model.OnKeyDownCommands.CommandsCleared -= OnKeyDownCommands_CommandsCleared;
            _model.OnKeyDownCommands.CommandDeleted -= OnKeyDownCommands_CommandDeleted;
            _model.OnKeyDownCommands.CommandUpdated -= OnKeyDownCommands_CommandUpdated;
        }

        #endregion

        //COMMON
        public override string ToString()
        {
            return Name;
        }

        //COMMON
        VMCommand _applyToCurrentModeCommand;
        /// <summary>
        /// Gets a command that sets the embedded <see cref="IKeyboardMode"/> as the holder's current one.
        /// </summary>
        public VMCommand ApplyToCurrentModeCommand
        {
            get
            {
                if( _applyToCurrentModeCommand == null )
                {
                    _applyToCurrentModeCommand = new VMCommand( () =>
                    {
                        if( !Context.KeyboardVM.CurrentMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( Context.KeyboardVM.CurrentMode ) )
                        {
                            Context.KeyboardVM.CurrentMode = _model.Mode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        public void TriggerPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        #region Key Image management

        ICommand _removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if( _removeImageCommand == null )
                {
                    _removeImageCommand = new VMCommand( () => Context.SkinConfiguration[_model.Key.CurrentLayout.Current].Remove( "Image" ) );
                }
                return _removeImageCommand;
            }
        }

        ICommand _browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                if( _browseCommand == null )
                {
                    _browseCommand = new VMCommand<VMKeyEditable>( ( k ) =>
                    {
                        var fd = new OpenFileDialog();
                        fd.DefaultExt = ".png";
                        if( fd.ShowDialog() == true )
                        {
                            if( !String.IsNullOrWhiteSpace( fd.FileName ) && File.Exists( fd.FileName ) && EnsureIsImage( Path.GetExtension( fd.FileName ) ) )
                            {
                                using( Stream str = fd.OpenFile() )
                                {
                                    byte[] bytes = new byte[str.Length];
                                    str.Read( bytes, 0, Convert.ToInt32( str.Length ) );
                                    string encodedImage = Convert.ToBase64String( bytes, Base64FormattingOptions.None );

                                    Context.SkinConfiguration[_model.Key.CurrentLayout.Current]["Image"] = encodedImage;
                                }
                            }
                        }
                    } );
                }
                return _browseCommand;
            }
        }

        private bool EnsureIsImage( string extension )
        {
            return String.Compare( extension, ".jpeg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".jpg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".png", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".bmp", StringComparison.CurrentCultureIgnoreCase ) == 0;
        }

        #endregion

    }
}
