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
using ContextEditor.Resources;
using ContextEditor.ViewModels;
using Microsoft.Win32;

namespace ContextEditor.ViewModels
{
    public class VMKeyModeEditable : VMKeyMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>, IModeViewModel
    {
        IKeyMode _model;

        public VMKeyModeEditable( VMContextEditable context, IKeyMode model )
            : base( context, model )
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

            _model.OnKeyDownCommands.CommandInserted += OnKeyDownCommands_CommandInserted;
            _model.OnKeyDownCommands.CommandsCleared += OnKeyDownCommands_CommandsCleared;
            _model.OnKeyDownCommands.CommandDeleted += OnKeyDownCommands_CommandDeleted;
            _model.OnKeyDownCommands.CommandUpdated += OnKeyDownCommands_CommandUpdated;

        }

        #region Properties

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
                //if( value ) Context.SelectedElement = Parent;

                if( value ) Parent.IsSelected = value;
                //Parent.IsSelected = value;

                if( previousKeyMode != null && previousLayoutKeyMode != null )
                {
                    previousKeyMode.TriggerPropertyChanged( "IsSelected" );
                    previousLayoutKeyMode.TriggerPropertyChanged( "IsSelected" );
                }

                ( Context.SelectedElement as VMKeyEditable ).KeyModeVM.TriggerPropertyChanged( "IsSelected" );
                ( Context.SelectedElement as VMKeyEditable ).LayoutKeyModeVM.TriggerPropertyChanged( "IsSelected" );

                //OnPropertyChanged( "IsSelected" );

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
        VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> _parent;
        /// <summary>
        /// Returns this VMKeyModeEditable's parent
        /// </summary>
        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> Parent
        {
            get
            {
                if( _parent == null ) _parent = Context.Obtain( _model.Key );
                return _parent;
            }
        }

        //COMMON
        /// <summary>
        /// Gets whether this LayoutKeyMode is a fallback or not.
        /// see <see cref="IKeyboardMode"/> for more explanations on the fallback concept
        /// This override checks the mode of the actual parent keyboard, instead of getting the current keyboard's mode
        /// </summary>
        public new bool IsFallback
        {
            get
            {
                IKeyboardMode keyboardMode = Context.KeyboardVM.CurrentMode;
                return !keyboardMode.ContainsAll( _model.Mode ) || !_model.Mode.ContainsAll( keyboardMode );
            }
        }

        //COMMON
        private VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

        //COMMON
        private IEnumerable<VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>> GetParents()
        {
            VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> elem = this;
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
        const string ownCollectionOutOfRangeErrorMessage = "The index of the command that has been {0} is out of range in its own Command collection";

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

        VMCommand<string> _addCommand;
        public VMCommand<string> AddCommandCommand
        {
            get
            {
                if( _addCommand == null )
                {
                    _addCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        _model.OnKeyDownCommands.Commands.Add( cmdString );
                    } );
                }

                return _addCommand;
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
                    } );
                }

                return _removeCommand;
            }
        }

        #endregion

        //COMMON
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

        //COMMON
        public override string ToString()
        {
            return Name;
        }

        protected override void OnDispose()
        {
            _model.OnKeyDownCommands.CommandInserted -= OnKeyDownCommands_CommandInserted;
            _model.OnKeyDownCommands.CommandsCleared -= OnKeyDownCommands_CommandsCleared;
            _model.OnKeyDownCommands.CommandDeleted -= OnKeyDownCommands_CommandDeleted;
            _model.OnKeyDownCommands.CommandUpdated -= OnKeyDownCommands_CommandUpdated;
            base.OnDispose();
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
                        if( !Context.KeyboardVM.CurrentMode.ContainsAll(_model.Mode) || !_model.Mode.ContainsAll(Context.KeyboardVM.CurrentMode) )
                        {
                            Context.KeyboardVM.CurrentMode = _model.Mode;
                        }
                    } );
                }
                return _applyToCurrentModeCommand;
            }
        }

        #region Key Image management

        ICommand _removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if( _removeImageCommand == null )
                {
                    _removeImageCommand = new VMCommand( () => Context.SkinConfiguration[_model].Remove( "Image" ) );
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

                                    Context.SkinConfiguration[_model]["Image"] = encodedImage;
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
