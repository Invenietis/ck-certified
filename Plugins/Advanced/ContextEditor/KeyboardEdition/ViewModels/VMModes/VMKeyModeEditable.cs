using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using KeyboardEditor.Resources;
using Microsoft.Win32;
using ProtocolManagerModel;

namespace KeyboardEditor.ViewModels
{
    public class VMKeyModeEditable : VMKeyModeBase, IModeViewModel
    {
        IKeyMode _model;
        string _sectionName;
        string _modeName;

        private IKeyMode Model { get { return _model; } set { _model = value; } }

        public VMKeyModeEditable( VMContextEditable context, IKeyMode model )
            : base( context, model )
        {
            _model = model;
            _commands = new ObservableCollection<string>();

            _modeName = String.IsNullOrWhiteSpace( _model.Mode.ToString() ) ? R.DefaultMode : _model.Mode.ToString();
            _sectionName = R.ContentSection;

            foreach( var cmd in Model.OnKeyDownCommands.Commands )
            {
                _commands.Add( cmd );
            }

            foreach( var cmd in Model.OnKeyUpCommands.Commands )
            {
                _commands.Add( cmd );
            }

            foreach( var cmd in Model.OnKeyPressedCommands.Commands )
            {
                _commands.Add( cmd );
            }

            InitializeCurrentImage();

            RegisterEvents();
        }

        private void InitializeCurrentImage()
        {
            object o = Context.SkinConfiguration[Model.Key.Current]["Image"];
            if( o != null )
            {
                string value = o.ToString();
                if( o is Image )
                {
                    value = ( (Image)o ).Source.ToString();
                }

                if( value.StartsWith( "pack://" ) )
                {
                    if( Context.DefaultImages.Values.Contains( value ) )
                    {
                        _selectedImage = Context.DefaultImages.Single( kvp => kvp.Value == value );
                    }
                    else
                    {
                        Context.AddDefaultImage( value, value );
                        _selectedImage = new KeyValuePair<string, string>( value, value );
                    }
                }
            }
        }

        #region Properties

        string _upLabel;
        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string UpLabel
        {
            get 
            {
                if( ActualParent.ShowIcon && Icon != char.MinValue ) return Icon.ToString();
                else return _upLabel ?? Model.UpLabel;
            }
            set 
            {
                //Model.UpLabel = value;
                if( value != _upLabel )
                {
                    _upLabel = value;
                    OnPropertyChanged( "UpLabel" );
                    OnPropertyChanged( "IsDirty" );
                }
            }
        }

        string _downLabel;
        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string DownLabel
        {
            get 
            {
                //return Model.DownLabel; 
                return _downLabel ?? Model.UpLabel;
            }
            set 
            {
                //Model.DownLabel = value; 
                if( value != _downLabel )
                {
                    _downLabel = value;
                    OnPropertyChanged( "DownLabel" );
                    OnPropertyChanged( "IsDirty" );
                }
            }
        }

        char _icon;
        public char Icon
        {
            get { return ActualParent.ShowIcon && _icon == char.MinValue ? Model.UpLabel[0] : _icon ; }
            set 
            {
                if( value != _icon )
                {
                    _icon = value;
                    OnPropertyChanged( "UpLabel" );
                    OnPropertyChanged( "Icon" );
                    OnPropertyChanged( "IsDirty" );
                }
            }
        }

        string _description;
        ///Gets the Description of the underling <see cref="IKeyMode"/>
        public string Description
        {
            get 
            { 
                //return Model.Description;
                return _description ?? Model.Description;
            }
            set 
            { 
                if( value != _description )
                {
                    _description = value;
                    OnPropertyChanged( "Description" );
                    OnPropertyChanged( "IsDirty" );
                }
            }
        }

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return Model.Enabled; } }

        bool _isSelected;
        /// <summary>
        /// Gets whether the element is selected.
        /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMKeyModeEditable
        /// </summary>
        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if( value )
                {
                    if( Context.SelectedElement != this )
                    {
                        Context.SelectedElement = this;
                        Context.CurrentlyDisplayedModeType = ModeTypes.Mode;
                    }
                }
                else
                {
                    ResetCacheValue();
                }

                _isSelected = value;
                OnPropertyChanged( "IsSelected" );

                ActualParent.TriggerOnPropertyChanged( "IsSelected" );
                ActualParent.TriggerOnPropertyChanged( "IsBeingEdited" );
                ActualParent.TriggerOnPropertyChanged( "Opacity" );
            }
        }

        public string SectionName { get { return _sectionName; } }
        public string ModeName { get { return _modeName; } }

        private void ResetCacheValue()
        {
            _icon = char.MinValue;
            _imageString = null;
            _downLabel = null;
            _description = null;
            _upLabel = null;
            _selectedImage = new KeyValuePair<string,string>();
            ActualParent.Image = null;
            ActualParent.DisplayType = Context.SkinConfiguration[Model.Key.Current].GetOrSet( "DisplayType", "Label" );

            OnPropertyChanged( "Icon" );
            OnPropertyChanged( "Image" );
            OnPropertyChanged( "DownLabel" );
            OnPropertyChanged( "UpLabel" );
            OnPropertyChanged( "Description" );
            OnPropertyChanged( "SelectedImage" );
            ActualParent.TriggerOnPropertyChanged( "ShowIcon" );
            ActualParent.TriggerOnPropertyChanged( "ShowImage" );
            ActualParent.TriggerOnPropertyChanged( "ShowLabel" );

            OnPropertyChanged( "IsDirty" );
        }

        #endregion

        #region KeyPrograms

        const string vmCollectionOutOfRangeErrorMessage = "The index of the command that has been {0} is out of range in the corresponding viewmodel's Command collection";
        const string ownCollectionOutOfRangeErrorMessage = "The index of the command that has been {0} is out of range in the viewmodel's own Command collection";

        void OnKeyDownCommands_CommandUpdated( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "updated" ) );
            if( Model.OnKeyDownCommands.Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "updated" ) );

            Commands[e.Index] = e.KeyProgram.Commands[e.Index];
        }

        void OnKeyDownCommands_CommandsCleared( object sender, KeyProgramCommandsEventArgs e )
        {
            Commands.Clear();
        }

        void OnKeyDownCommands_CommandInserted( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Model.OnKeyDownCommands.Commands[e.Index] == null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "inserted" ) );
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

        ICommand _initializeCommand;
        /// <summary>
        /// Sent when the user clicks on "Add an action" : creates a new empty ProtocolEditor 
        /// </summary>
        public ICommand InitializeCommandCommand
        {
            get
            {
                if( _initializeCommand == null )
                {
                    _initializeCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        ProtocolEditorsProvider.InitializeProtocolEditor( Model );
                        ShowKeyCommandCreationPanel = true;
                    } );
                }

                return _initializeCommand;
            }
        }

        ICommand _saveCommand;
        public ICommand SaveCommandCommand
        {
            get
            {
                if( _saveCommand == null )
                {
                    _saveCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        DoAddKeyCommand( ProtocolEditorsProvider.ProtocolEditor.ToString(), _commandBeingChangedIndex );
                        ShowKeyCommandCreationPanel = false;
                        _commandBeingChanged = String.Empty;
                        _commandBeingChangedIndex = -1;
                    } );
                }

                return _saveCommand;
            }
        }

        private void DoAddKeyCommand( string keyCommand, int index )
        {
            if( index == -1 ) index = Model.OnKeyDownCommands.Commands.Count;
            Model.OnKeyDownCommands.Commands.Insert( index, keyCommand );
            ProtocolEditorsProvider.FlushCurrentProtocolEditor();
        }

        string _commandBeingChanged = String.Empty;
        int _commandBeingChangedIndex = -1;
        VMCommand<string> _changeCommand;
        /// <summary>
        /// Sent when the user clicks on the edit button of a KeyCommand.
        /// </summary>
        public VMCommand<string> ChangeCommandCommand
        {
            get
            {
                if( _changeCommand == null )
                {
                    _changeCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        _commandBeingChanged = cmdString;
                        for( int i = 0; i < Model.OnKeyDownCommands.Commands.Count; i++ )
                        {
                            if( Model.OnKeyDownCommands.Commands[i] == cmdString )
                            {
                                _commandBeingChangedIndex = i;
                                break;
                            }
                        }

                        DoRemoveKeyCommand( cmdString );
                        ProtocolEditorsProvider.CreateKeyCommand( cmdString, Model );
                        ShowKeyCommandCreationPanel = true;
                    } );
                }

                return _changeCommand;
            }
        }


        ICommand _cancelCommand;
        public ICommand CancelChangesCommand
        {
            get
            {
                if( _cancelCommand == null )
                {
                    _cancelCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        if( !String.IsNullOrEmpty( _commandBeingChanged ) )
                        {
                            DoAddKeyCommand( _commandBeingChanged, _commandBeingChangedIndex );
                            _commandBeingChanged = String.Empty;
                            _commandBeingChangedIndex = -1;
                        }
                        else
                        {
                            ProtocolEditorsProvider.FlushCurrentProtocolEditor();
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
            Debug.Assert( Model.OnKeyDownCommands.Commands.Contains( cmdString )
                || Model.OnKeyUpCommands.Commands.Contains( cmdString )
                || Model.OnKeyPressedCommands.Commands.Contains( cmdString ) );

            //In order to keep things a little simpler in the editor, we are only bound to the KeyDownCommand events (add, delete etc..)
            //So if the key that we are modifying is not part of the KeyDownCommand list, we don't know it.
            //As the delete event doesn't give us the actual string of the removed command, we cannot remove it from the observable collection on our level.
            //The editor's simplification must not impact the model, so until we remove this simplification in the editor, we are going to synchronize the collection by hand, which is pretty ugly.

            if( Model.OnKeyDownCommands.Commands.Contains( cmdString ) )
                Model.OnKeyDownCommands.Commands.Remove( cmdString );
            else if( Model.OnKeyUpCommands.Commands.Contains( cmdString ) )
            {
                Model.OnKeyUpCommands.Commands.Remove( cmdString );
                _commands.Remove( cmdString ); //see comment above
            }
            else if( Model.OnKeyPressedCommands.Commands.Contains( cmdString ) )
            {
                Model.OnKeyPressedCommands.Commands.Remove( cmdString );
                _commands.Remove( cmdString ); //see comment above
            }
        }

        VMProtocolEditorsProvider _keyCommandTypeProvider;
        public VMProtocolEditorsProvider ProtocolEditorsProvider
        {
            get { return Context.ProtocolManagerService.Service.ProtocolEditorsProviderViewModel; }
            set
            {
                _keyCommandTypeProvider = value;
                OnPropertyChanged( "ProtocolEditorsProvider" );
            }
        }

        ICommand _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public ICommand DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new CK.Windows.App.VMCommand( () =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        VMKeyEditable parent = ActualParent; //Keeping a ref to the parent, since the model will be detached from its parent when destroyed
                        Model.Destroy();
                        parent.RefreshKeyboardModelViewModels();
                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

        #endregion

        #region OnXXX

        protected override void OnModeChangedTriggered()
        {
            OnPropertyChanged( "SelectedImage" );
            OnPropertyChanged( "Description" );
            OnPropertyChanged( "IsSelected" );
            OnPropertyChanged( "DownLabel" );
            OnPropertyChanged( "ModeName" );
            OnPropertyChanged( "UpLabel" );
            OnPropertyChanged( "Enabled" );
            OnPropertyChanged( "Name" );
        }

        internal override void Dispose()
        {
            UnregisterEvents();
            base.Dispose();
        }

        private void RegisterEvents()
        {
            Model.OnKeyDownCommands.CommandInserted += OnKeyDownCommands_CommandInserted;
            Model.OnKeyDownCommands.CommandsCleared += OnKeyDownCommands_CommandsCleared;
            Model.OnKeyDownCommands.CommandDeleted += OnKeyDownCommands_CommandDeleted;
            Model.OnKeyDownCommands.CommandUpdated += OnKeyDownCommands_CommandUpdated;
        }

        private void UnregisterEvents()
        {
            Model.OnKeyDownCommands.CommandInserted -= OnKeyDownCommands_CommandInserted;
            Model.OnKeyDownCommands.CommandsCleared -= OnKeyDownCommands_CommandsCleared;
            Model.OnKeyDownCommands.CommandDeleted -= OnKeyDownCommands_CommandDeleted;
            Model.OnKeyDownCommands.CommandUpdated -= OnKeyDownCommands_CommandUpdated;
        }

        #endregion

        #region Key Image management

        ICommand _removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if( _removeImageCommand == null )
                {
                    _removeImageCommand = new CK.Windows.App.VMCommand( () => Context.SkinConfiguration[Model.Key.Current].Remove( "Image" ) );
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
                        fd.Filter = "Image (*.png, *.jpg, *.bmp)|*.png;*.jpg;*.bmp|Tous les fichiers (*.*)|*.*";
                        if( fd.ShowDialog() == true )
                        {
                            if( !String.IsNullOrWhiteSpace( fd.FileName ) && File.Exists( fd.FileName ) && EnsureIsImage( Path.GetExtension( fd.FileName ) ) )
                            {
                                SelectedImage = new KeyValuePair<string, string>( fd.FileName, fd.FileName );
                            }
                        }
                    } );
                }
                return _browseCommand;
            }
        }

        ICommand _saveContentCommand;
        public ICommand SaveContentCommand
        {
            get
            {
                if( _saveContentCommand == null )
                {
                    _saveContentCommand = new VMCommand<VMKeyEditable>( ( k ) =>
                    {
                        if( ActualParent.ShowLabel )
                        {
                            if( Model.UpLabel != UpLabel ) Model.UpLabel = UpLabel;
                            if( Model.DownLabel != DownLabel ) Model.DownLabel = DownLabel;
                        }
                        else if( ActualParent.ShowIcon )
                        {
                            if( Model.UpLabel != Icon.ToString() ) Model.UpLabel = Model.DownLabel = Icon.ToString();
                        }
                        else if( ActualParent.ShowImage )
                        {
                            Context.SkinConfiguration[Model.Key.Current]["Image"] = _imageString;
                        }
                        if( Model.Description != Description ) Model.Description = Description;

                        Context.SkinConfiguration[Model.Key.Current]["DisplayType"] = ActualParent.DisplayType;

                        ResetCacheValue();
                    }, k => IsDirty );
                }
                return _saveContentCommand;
            }
        }
        public bool IsDirty
        {
            get
            {
                return _upLabel != null
                    || _downLabel != null
                    || _imageString != null
                    || _icon != char.MinValue
                    || _description != null;
            }
        }

        private void ProcessImageStream( Stream str )
        {
            byte[] bytes = new byte[str.Length];
            str.Read( bytes, 0, Convert.ToInt32( str.Length ) );
            string encodedImage = Convert.ToBase64String( bytes, Base64FormattingOptions.None );

            Image image = new Image();
            image.Source = WPFImageProcessingHelper.ProcessImage( encodedImage ).Source;
            ActualParent.Image = image;

            _imageString = encodedImage;
            _selectedImage = new KeyValuePair<string, string>();
            _selectedText = "";
            OnPropertyChanged( "SelectedText" );
        }

        private bool EnsureIsImage( string extension )
        {
            return String.Compare( extension, ".jpeg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".jpg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".png", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".bmp", StringComparison.CurrentCultureIgnoreCase ) == 0;
        }

        string _imageString;
        public Image Image
        {
            get { return ActualParent.Image; }
        }

        KeyValuePair<string, string> _selectedImage;
        public KeyValuePair<string, string> SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                if( _selectedImage.Value != value.Value )
                {
                    _selectedImage = value;
                    //if( !Context.DefaultImages.Values.Contains( value.Value ) )
                    //    Context.AddDefaultImage( value.Key, value.Value );

                    if( File.Exists( _selectedImage.Value ) )
                    {
                        //The value is the path to a file
                        using( FileStream fs = File.OpenRead( _selectedImage.Value ) )
                        {
                            ProcessImageStream( fs );
                        }
                    }
                    else
                    {
                        Image image = new Image();
                        image.Source = WPFImageProcessingHelper.ProcessImage( value.Value ).Source;
                        ActualParent.Image = image;
                        _imageString = value.Value;
                    }

                    OnPropertyChanged( "SelectedImage" );
                    OnPropertyChanged( "Image" );
                    OnPropertyChanged( "IsDirty" );
                }
            }
        }

        string _selectedText;
        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                if( value != _selectedText )
                {
                    _selectedText = value;
                }
            }
        }

        #endregion

    }
}
