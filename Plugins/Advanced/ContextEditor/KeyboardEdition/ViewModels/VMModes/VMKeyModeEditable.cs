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
using ContextEditor.ViewModels;
using Microsoft.Win32;

//TODOJL : When having the time, replace the VMKeyboardMode of a VMKeyEditable by this object and its Layout parallel
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
            //TODO : add the commands

            _model.OnKeyPressedCommands.CommandInserted += OnKeyPressedCommands_CommandInserted;
            _model.OnKeyPressedCommands.CommandsCleared += OnKeyPressedCommands_CommandsCleared;
            _model.OnKeyPressedCommands.CommandDeleted += OnKeyPressedCommands_CommandDeleted;
            _model.OnKeyPressedCommands.CommandUpdated += OnKeyPressedCommands_CommandUpdated;

        }

        #region Properties

        public string Name { get { return _model.Mode.ToString(); } }

        /// <summary>
        /// Gets whether the element is selected.
        /// A VMKeyModeEditable is selected if its parent is selected and that the keyboard's current mode correspond to the Mode associated with this VMKeyModeEditable
        /// </summary>
        public override bool IsSelected
        {
            get { return Parent.IsSelected 
                && ActualParent.CurrentKeyModeModeVM.Mode.ContainsAll( _model.Mode )
                && _model.Mode.ContainsAll( ActualParent.CurrentKeyModeModeVM.Mode ) 
                && Context.CurrentlyDisplayedModeType == ModeTypes.Mode; }
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

        /// <summary>
        /// Returns this VMKeyModeEditable's parent's layout element
        /// </summary>
        public override CK.Keyboard.Model.IKeyboardElement LayoutElement
        {
            get { return Parent.LayoutElement; }
        }

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

        private VMKeyEditable ActualParent { get { return Parent as VMKeyEditable; } }

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

        void OnKeyPressedCommands_CommandUpdated( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] != null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "updated" ) );
            if( _model.OnKeyPressedCommands.Commands[e.Index] != null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "updated" ) );

            Commands[e.Index] = e.KeyProgram.Commands[e.Index];
        }

        void OnKeyPressedCommands_CommandsCleared( object sender, KeyProgramCommandsEventArgs e )
        {
            Commands.Clear();
        }

        void OnKeyPressedCommands_CommandInserted( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] != null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "inserted" ) );
            if( _model.OnKeyPressedCommands.Commands[e.Index] != null ) throw new IndexOutOfRangeException( String.Format( ownCollectionOutOfRangeErrorMessage, "inserted" ) );

            Commands.Insert( e.Index, e.KeyProgram.Commands[e.Index] );
        }

        void OnKeyPressedCommands_CommandDeleted( object sender, KeyProgramCommandsEventArgs e )
        {
            if( Commands[e.Index] != null ) throw new IndexOutOfRangeException( String.Format( vmCollectionOutOfRangeErrorMessage, "deleted" ) );

            Commands.RemoveAt( e.Index );
        }

        ObservableCollection<string> _commands;
        public ObservableCollection<string> Commands { get { return _commands; } }

        VMCommand<string> _addCommand;
        public VMCommand<string> AddCommand
        {
            get
            {
                if( _addCommand == null )
                {
                    _addCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        _model.OnKeyPressedCommands.Commands.Add( cmdString );
                    } );
                }

                return _addCommand;
            }
        }

        VMCommand<string> _removeCommand;
        public VMCommand<string> RemoveCommand
        {
            get
            {
                if( _removeCommand == null )
                {
                    _removeCommand = new VMCommand<string>( ( cmdString ) =>
                    {
                        Debug.Assert( Commands.Contains( cmdString ) );
                        Commands.Remove( cmdString );
                    } );
                }

                return _removeCommand;
            }
        }

        #endregion

        VMCommand<string> _deleteKeyModeCommand;
        /// <summary>
        /// Gets a Command that deletes the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand<string> DeleteKeyModeCommand
        {
            get
            {
                if( _deleteKeyModeCommand == null )
                {
                    _deleteKeyModeCommand = new VMCommand<string>( ( type ) =>
                    {
                        Context.KeyboardVM.CurrentMode = Context.KeyboardContext.EmptyMode;
                        _model.Destroy();
                        ActualParent.RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

        protected override void OnDispose()
        {
            _model.OnKeyPressedCommands.CommandInserted -= OnKeyPressedCommands_CommandInserted;
            _model.OnKeyPressedCommands.CommandsCleared -= OnKeyPressedCommands_CommandsCleared;
            _model.OnKeyPressedCommands.CommandDeleted -= OnKeyPressedCommands_CommandDeleted;
            _model.OnKeyPressedCommands.CommandUpdated -= OnKeyPressedCommands_CommandUpdated;
            base.OnDispose();
        }


        #region Key Image management

        /// <summary>
        /// Gets the image associated with the underlying <see cref="ILayoutKeyMode"/>, for the current <see cref="IKeyboardMode"/>
        /// </summary>
        public object Image
        {
            get
            {
                object imageData = Context.SkinConfiguration[_model]["Image"];
                Image image = new Image();

                if( imageData != null )
                {
                    return ProcessImage( imageData, image );
                }

                return null;
            }

            set { Context.SkinConfiguration[_model]["Image"] = value; }
        }

        //This method handles the different ways an image can be stored in plugin datas
        private object ProcessImage( object imageData, Image image )
        {
            string imageString = imageData.ToString();


            if( imageData.GetType() == typeof( Image ) )
            {
                //If a WPF image was stored in the PluginDatas, we use its source to create a NEW image instance, to enable using it multiple times. 
                Image img = new Image();
                BitmapImage bitmapImage = new BitmapImage( new Uri( ( (Image)imageData ).Source.ToString() ) );
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                img.Source = bitmapImage;
                return img;
            }
            else if( File.Exists( imageString ) ) //Handles URis
            {
                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri( imageString );
                bitmapImage.EndInit();

                image.Source = bitmapImage;

                return image;
            }
            else if( imageString.StartsWith( "pack://" ) ) //Handles the WPF's pack:// protocol
            {
                ImageSourceConverter imsc = new ImageSourceConverter();
                return imsc.ConvertFromString( imageString );
            }
            else
            {
                byte[] imageBytes = Convert.FromBase64String( imageData.ToString() ); //Handles base 64 encoded images
                using( MemoryStream ms = new MemoryStream( imageBytes ) )
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    image.Source = bitmapImage;
                }
                return image;
            }
        }

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

                                    Context.SkinConfiguration[_model].GetOrSet( "Image", encodedImage );
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
