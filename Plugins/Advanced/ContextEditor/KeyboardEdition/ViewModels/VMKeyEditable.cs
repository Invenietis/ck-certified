#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace ContextEditor.ViewModels
{
    public class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
        VMContextEditable _ctx;
        
        public VMKeyEditable( VMContextEditable ctx, IKey k )
            : base( ctx, k, false )
        {
            _ctx = ctx;
            KeyDownCommand = new VMCommand( () => _ctx.SelectedElement = this );
            _currentKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.Current.Mode );
            _currentLayoutKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, k.CurrentLayout.Current.Mode );
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
            } );
            this.PropertyChanged += new PropertyChangedEventHandler( OnPropertyChangedTriggered );
        }

        private void OnPropertyChangedTriggered(object sender,PropertyChangedEventArgs e )
        {
            if(e.PropertyName == "IsBeingEdited")
            {
                OnPropertyChanged("Opacity");
            }
        }

        bool _isBeingEdited;
        /// <summary>
        /// Gets whether this object is being edited (or is contained in a parent that is being edited
        /// </summary>
        public override bool IsBeingEdited 
        { 
            get { return _isBeingEdited || Parent.IsBeingEdited; }
            set { _isBeingEdited = value; OnPropertyChanged( "IsBeingEdited" ); OnPropertyChanged( "Opacity" ); } 
        }

        /// <summary>
        /// Gets the uplabel of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public string Name { get { return UpLabel; } }

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> Parent
        {
            get { return Context.Obtain( Model.Zone ); }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }

        internal void TriggerOnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                OnPropertyChanged( "HighlightBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
            }
        }

        protected override void OnTriggerModeChanged()
        {
            RefreshKeyboardModelViewModels();
        }

        private void RefreshKeyboardModelViewModels()
        {
            _currentLayoutKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, Model.CurrentLayout.Current.Mode );
            _currentKeyModeModeVM = new VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>( _ctx, Model.Current.Mode );
            OnPropertyChanged( "CurrentLayoutKeyModeModeVM" );
            OnPropertyChanged( "CurrentKeyModeModeVM" );
        }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            base.OnDispose();
        }

        /// <summary>
        /// This regions contains overrides to the <see cref="VMKey"/> properties.
        /// It enables hidding the fallback if necessary.
        /// </summary>
        #region KeyMode properties overrides

        VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> _currentKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> CurrentKeyModeModeVM { get { return _currentKeyModeModeVM; } }

        VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> _currentLayoutKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> CurrentLayoutKeyModeModeVM { get { return _currentLayoutKeyModeModeVM; } } 

        VMCommand<string> _createKeyModeCommand;
        /// <summary>
        /// Gets a Command that creates the <see cref="IKeyMode"/> corresponding to the current <see cref="IKeyboardMode"/>, for the underlying <see cref="IKey"/>
        /// </summary>
        public VMCommand<string> CreateKeyModeCommand
        {
            get
            {
                if( _createKeyModeCommand == null )
                {
                    _createKeyModeCommand = new VMCommand<string>( (type) =>
                    {
                       
                        if( type == "KeyMode" )
                        {
                            Debug.Assert( IsKeyModeFallback );
                            Model.KeyModes.Create( Model.Keyboard.CurrentMode );

                            OnPropertyChanged( "IsKeyModeFallback" );
                        }
                        else if( type == "LayoutKeyMode" )
                        {
                            Debug.Assert( IsLayoutKeyModeFallback );
                            ILayoutKeyMode previousMode = Model.CurrentLayout.Current;
                            ILayoutKeyMode mode = Model.CurrentLayout.LayoutKeyModes.Create( Model.Keyboard.CurrentMode );

                            //Retrieving the previous layoutkeymode's properties values, to apply them on the new layoutkeymode.
                            mode.X = previousMode.X;
                            mode.Y = previousMode.Y;
                            mode.Height = previousMode.Height;
                            mode.Width = previousMode.Width;
                            mode.Visible = true; 

                            OnPropertyChanged( "IsLayoutKeyModeFallback" );
                        }

                        RefreshKeyboardModelViewModels();
                    } );
                }
                return _createKeyModeCommand;
            }
        }

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
                    _deleteKeyModeCommand = new VMCommand<string>( (type) =>
                    {
                        if( type == "KeyMode" )
                        {
                            Debug.Assert( !IsKeyModeFallback );
                            Model.Current.Destroy();

                            OnPropertyChanged( "IsKeyModeFallback" );
                        }
                        else if( type == "LayoutKeyMode" )
                        {
                            Debug.Assert( !IsLayoutKeyModeFallback );
                            Model.CurrentLayout.Current.Destroy();

                            OnPropertyChanged( "IsLayoutKeyModeFallback" );
                        }
                        RefreshKeyboardModelViewModels();

                    } );
                }
                return _deleteKeyModeCommand;
            }
        }

        ///Gets the UpLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string UpLabel
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.UpLabel : String.Empty; }
            set { base.UpLabel = value; }
        }

        ///Gets the DownLabel of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string DownLabel
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.DownLabel : String.Empty; }
            set { base.DownLabel = value; }
        }

        ///Gets the Description of the underling <see cref="IKey"/> if fallback is enabled or if the <see cref="IKeyMode"/> if not a fallback
        public new string Description
        {
            get { return ( !IsKeyModeFallback || ShowKeyModeFallback ) ? base.Description : String.Empty; }
            set { base.Description = value; }
        }

        #endregion

        #region Key Image management

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        public object Image
        {
            get
            {
                object imageData = _ctx.SkinConfiguration[Model.CurrentLayout.Current]["Image"];
                Image image = new Image();

                if( imageData != null )
                {
                    string imageString = imageData.ToString();

                    if( imageData.GetType() == typeof( Image ) ) return imageData;
                    else if( File.Exists( imageString ) )
                    {

                        BitmapImage bitmapImage = new BitmapImage();

                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri( imageString );
                        bitmapImage.EndInit();

                        image.Source = bitmapImage;

                        return image;
                    }
                    else if( imageString.StartsWith( "pack://" ) )
                    {
                        ImageSourceConverter imsc = new ImageSourceConverter();
                        return imsc.ConvertFromString( imageString );
                    }
                    else
                    {
                        byte[] imageBytes = Convert.FromBase64String( imageData.ToString() );
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

                return null;
            }

            set { _ctx.SkinConfiguration[Model.CurrentLayout.Current]["Image"] = value; }
        }

        ICommand _removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if( _removeImageCommand == null )
                {
                    _removeImageCommand = new VMCommand( () => Context.SkinConfiguration[Model.CurrentLayout.Current].Remove( "Image" ) );
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

                                    Context.SkinConfiguration[Model.CurrentLayout.Current].GetOrSet( "Image", encodedImage );
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

        /// <summary>
        /// Gets the opacity of the key.
        /// When the key has <see cref="IsBeingEdited"/> set to false, the opacity is lowered.
        /// returns 1 otherwise.
        /// </summary>
        public double Opacity
        {
            get { return IsBeingEdited ? 1 : 0.3; }
            //get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
        }
    }
}
