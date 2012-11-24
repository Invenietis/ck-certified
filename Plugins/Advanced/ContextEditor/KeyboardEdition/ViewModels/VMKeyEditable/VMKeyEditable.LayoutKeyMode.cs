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
using System.Windows.Controls.Primitives;
using CommonServices;

namespace ContextEditor.ViewModels
{
    public partial class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }

        #region Key Image management

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        /// <summary>
        /// Gets the image associated with the underlying <see cref="ILayoutKeyMode"/>, for the current <see cref="IKeyboardMode"/>
        /// </summary>
        public object Image
        {
            get
            {
                object imageData = _ctx.SkinConfiguration[Model.CurrentLayout.Current]["Image"];
                Image image = new Image();

                if( imageData != null )
                {
                    return ProcessImage( imageData, image );
                }

                return null;
            }

            set { _ctx.SkinConfiguration[Model.CurrentLayout.Current]["Image"] = value; }
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
