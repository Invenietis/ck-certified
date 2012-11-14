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
            _currentKeyModeModeVM = new VMKeyboardMode( _ctx, k.Current.Mode );
            _currentLayoutKeyModeModeVM = new VMKeyboardMode( _ctx, k.CurrentLayout.Current.Mode );
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
            _currentLayoutKeyModeModeVM = new VMKeyboardMode( _ctx, Model.CurrentLayout.Current.Mode );
            _currentKeyModeModeVM = new VMKeyboardMode( _ctx, Model.Current.Mode );
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

        VMKeyboardMode _currentKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode CurrentKeyModeModeVM { get { return _currentKeyModeModeVM; } }

        VMKeyboardMode _currentLayoutKeyModeModeVM;
        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        public VMKeyboardMode CurrentLayoutKeyModeModeVM { get { return _currentLayoutKeyModeModeVM; } } 

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

        #region Layout Edition elements

        VMCommand<string> _clearCmd;
        public VMCommand<string> ClearPropertyCmd { get { return _clearCmd == null ? _clearCmd = new VMCommand<string>( ClearProperty, CanClearProperty ) : _clearCmd; } }

        void ClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            foreach( var pname in names ) _ctx.Config[Model.CurrentLayout.Current].Remove( pname );
        }

        bool CanClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            // We can clear property if the property owns directly a value.
            foreach( var pname in names ) if( _ctx.Config[Model.CurrentLayout.Current][pname] != null ) return true;
            return false;
        }

        public Color Background
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["Background"] = value;
            }
        }

        public Color HoverBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["HoverBackground"] = value;
            }
        }

        public Color HighlightBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["HighlightBackground"] = value;
            }
        }

        public Color PressedBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["PressedBackground"] = value;
            }
        }

        public Color LetterColor
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["LetterColor"] = value;
            }
        }

        public FontStyle FontStyle
        {
            get { return Model.CurrentLayout.Current.GetWrappedPropertyValue( _ctx.Config, "FontStyle", FontStyles.Normal ).Value; }
        }

        public FontWeight FontWeight
        {
            get { return Model.CurrentLayout.Current.GetWrappedPropertyValue( _ctx.Config, "FontWeight", FontWeights.Normal ).Value; }
        }

        public TextDecorationCollection TextDecorations
        {
            get
            {
                return Model.CurrentLayout.Current.GetWrappedPropertyValue<TextDecorationCollection>( _ctx.Config, "TextDecorations" ).Value;
            }
        }

        #region FontPoperties used for edition
        /// <summary>
        /// Gets whether the FontStyle of the Current LayoutKeyMode is <see cref="FontStyles.Italic"/>.
        /// Returns false otherwise.
        /// Used to bind the FontStyle to a boolean control ( for example, a <see cref="ToggleButton"/>)
        /// </summary>
        public bool FontStyleAsBool
        {
            get { return Model.CurrentLayout.Current.GetWrappedPropertyValue( _ctx.Config, "FontStyle", FontStyles.Normal ).Value == FontStyles.Italic; }
            set
            {
                if( value ) _ctx.Config[Model.CurrentLayout.Current]["FontStyle"] = FontStyles.Italic;
                else _ctx.Config[Model.CurrentLayout.Current]["FontStyle"] = FontStyles.Normal;
            }
        }

        /// <summary>
        /// Gets whether the FontWeight of the Current LayoutKeyMode is <see cref="FontWeights.Bold"/>.
        /// Returns false otherwise.
        /// Used to bind the FontWeight to a boolean control ( for example, a <see cref="ToggleButton"/>)
        /// </summary>
        public bool FontWeightAsBool
        {
            get { return Model.CurrentLayout.Current.GetWrappedPropertyValue( _ctx.Config, "FontWeight", FontWeights.Normal ).Value == FontWeights.Bold; }
            set
            {
                if( value ) _ctx.Config[Model.CurrentLayout.Current]["FontWeight"] = FontWeights.Bold;
                else _ctx.Config[Model.CurrentLayout.Current]["FontWeight"] = FontWeights.Normal;
            }
        }

        /// <summary>
        /// Gets whether the TextDecorationCollection of the Current LayoutKeyMode has elements.
        /// Returns false otherwise.
        /// Used to bind the TextDecorationCollection to a boolean control ( for example, a <see cref="ToggleButton"/>)
        /// </summary>
        public bool TextDecorationsAsBool
        {
            get
            {
                var val = Model.CurrentLayout.Current.GetWrappedPropertyValue<TextDecorationCollection>( _ctx.Config, "TextDecorations" );
                return val.Value != null && val.Value.Count > 0;
            }
            set
            {
                if( value ) _ctx.Config[Model.CurrentLayout.Current]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "Underline" );
                else _ctx.Config[Model.CurrentLayout.Current]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "" );
            }
        }

        IEnumerable<double> _sizes;
        IEnumerable<double> GetSizes( int from, int to )
        {
            for( int i = from; i <= to; i++ ) yield return i;
        }
        public IEnumerable<double> FontSizes { get { return _sizes == null ? _sizes = GetSizes( 10, 30 ) : _sizes; } }


        #endregion

        public double FontSize
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ); }
            set
            {
                _ctx.SkinConfiguration[Model.CurrentLayout.Current]["FontSize"] = value;
            }
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

        public double Opacity
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
        }
    }
}
