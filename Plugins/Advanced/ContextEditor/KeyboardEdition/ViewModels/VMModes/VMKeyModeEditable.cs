﻿using System;
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
using CK.Windows.App;
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
            object o = Context.SkinConfiguration[Model.Key.CurrentLayout.Current]["Image"];
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
                        _selectedImage = Context.DefaultImages.Where( kvp => kvp.Value == value ).Single();
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

        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string UpLabel
        {
            get { return Model.UpLabel; }
            set { Model.UpLabel = value; }
        }

        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public string DownLabel
        {
            get { return Model.DownLabel; }
            set { Model.DownLabel = value; }
        }

        ///Gets the Description of the underling <see cref="IKeyMode"/>
        public string Description
        {
            get { return Model.Description; }
            set { Model.Description = value; }
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
                VMKeyModeEditable previousKeyMode = null;
                VMLayoutKeyModeEditable previousLayoutKeyMode = null;

                if( value )
                {
                    if( Context.SelectedElement != this ) Context.SelectedElement = this;

                    if( Context.SelectedElement is VMKeyEditable )
                    {
                        previousKeyMode = ( Context.SelectedElement as VMKeyEditable ).KeyModeVM;
                        previousLayoutKeyMode = ( Context.SelectedElement as VMKeyEditable ).LayoutKeyModeVM;
                        if( previousKeyMode != null && previousLayoutKeyMode != null )
                        {
                            previousKeyMode.TriggerPropertyChanged( "IsSelected" );
                            previousLayoutKeyMode.TriggerPropertyChanged( "IsSelected" );
                            previousKeyMode.ActualParent.TriggerOnPropertyChanged( "IsSelected" );
                            previousKeyMode.ActualParent.TriggerOnPropertyChanged( "IsBeingEdited" );
                            previousKeyMode.ActualParent.TriggerOnPropertyChanged( "Opacity" );
                        }
                    }
                    else if( Context.SelectedElement is VMLayoutKeyModeEditable )
                    {
                        ( Context.SelectedElement as VMLayoutKeyModeEditable ).TriggerPropertyChanged( "IsSelected" );
                    }
                    else if( Context.SelectedElement is VMKeyModeEditable )
                    {
                        ( Context.SelectedElement as VMKeyModeEditable ).TriggerPropertyChanged( "IsSelected" );
                    }

                    Context.CurrentlyDisplayedModeType = ModeTypes.Mode;
                    //Context.KeyboardVM.CurrentMode = Model.Mode;
                }

                _isSelected = value;

                if( Context.SelectedElement is VMKeyEditable )
                {
                    ( Context.SelectedElement as VMKeyEditable ).KeyModeVM.TriggerPropertyChanged( "IsSelected" );
                    ( Context.SelectedElement as VMKeyEditable ).LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );
                }
                else if( Context.SelectedElement is VMLayoutKeyModeEditable ) ( Context.SelectedElement as VMLayoutKeyModeEditable ).TriggerPropertyChanged( "IsSelected" );
                else if( Context.SelectedElement is VMKeyModeEditable ) ( Context.SelectedElement as VMKeyModeEditable ).TriggerPropertyChanged( "IsSelected" );

                ActualParent.TriggerOnPropertyChanged( "IsSelected" );
                ActualParent.TriggerOnPropertyChanged( "IsBeingEdited" );
                ActualParent.TriggerOnPropertyChanged( "Opacity" );
            }
        }

        public string SectionName { get { return _sectionName; } }
        public string ModeName { get { return _modeName; } }

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

        VMCommand _initializeCommand;
        /// <summary>
        /// Sent when the user clicks on "Add an action" : creates a new empty ProtocolEditor 
        /// </summary>
        public VMCommand InitializeCommandCommand
        {
            get
            {
                if( _initializeCommand == null )
                {
                    _initializeCommand = new VMCommand( () =>
                    {
                        ProtocolEditorsProvider.InitializeProtocolEditor();
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
                        ProtocolEditorsProvider.CreateKeyCommand( cmdString );
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

            if( Model.OnKeyDownCommands.Commands.Contains( cmdString ) )
                Model.OnKeyDownCommands.Commands.Remove( cmdString );
            else if( Model.OnKeyUpCommands.Commands.Contains( cmdString ) )
                Model.OnKeyDownCommands.Commands.Remove( cmdString );
            else if( Model.OnKeyPressedCommands.Commands.Remove( cmdString ) )
                Model.OnKeyDownCommands.Commands.Remove( cmdString );
            else
                throw new ArgumentException( "Trying to remove a command that cannot be found in the key commands. Key : " + Model.UpLabel + ", command : " + cmdString );
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
            OnPropertyChanged( "UpLabel" );
            OnPropertyChanged( "DownLabel" );
            OnPropertyChanged( "Description" );
            OnPropertyChanged( "Enabled" );
            OnPropertyChanged( "IsSelected" );
            OnPropertyChanged( "ModeName" );
            OnPropertyChanged( "Name" );
            OnPropertyChanged( "SelectedImage" );
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
                    _removeImageCommand = new VMCommand( () => Context.SkinConfiguration[Model.Key.CurrentLayout.Current].Remove( "Image" ) );
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
                                SelectedImage = new KeyValuePair<string, string>( fd.FileName, fd.FileName );
                            }
                        }
                    } );
                }
                return _browseCommand;
            }
        }

        private void ProcessImageStream( Stream str )
        {
            byte[] bytes = new byte[str.Length];
            str.Read( bytes, 0, Convert.ToInt32( str.Length ) );
            string encodedImage = Convert.ToBase64String( bytes, Base64FormattingOptions.None );

            Context.SkinConfiguration[Model.Key.CurrentLayout.Current]["Image"] = encodedImage;
        }

        private bool EnsureIsImage( string extension )
        {
            return String.Compare( extension, ".jpeg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".jpg", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".png", StringComparison.CurrentCultureIgnoreCase ) == 0
                || String.Compare( extension, ".bmp", StringComparison.CurrentCultureIgnoreCase ) == 0;
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
                    if( !Context.DefaultImages.Values.Contains( value.Value ) )
                        Context.AddDefaultImage( value.Key, value.Value );

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
                        //The value is the "pack" path to an internal image
                        Context.SkinConfiguration[Model.Key.CurrentLayout.Current]["Image"] = value.Value;
                    }

                    OnPropertyChanged( "SelectedImage" );
                }
            }
        }

        #endregion

    }
}
