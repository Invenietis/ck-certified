#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\Helpers\WPFImageProcessingHelper.cs) is part of CiviKey. 
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CK.WPF.ViewModel
{
    public class WPFImageProcessingHelper
    {
        public static Image CloneImage( Image img )
        {
            using( var stream = new MemoryStream() )
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize( stream, img );
                stream.Position = 0;
                return (Image)formatter.Deserialize( stream );
            }
        }

        public static Image GetImageFromStream( Stream stream )
        {
            var formatter = new BinaryFormatter();
            return (Image)formatter.Deserialize( stream );
        }

        //This method handles the different ways an image can be stored in plugin datas
        public static Image ProcessImage( object imageData )
        {
            Image image = new Image();

            if (imageData is BitmapSource)
            {
                var img = new Image();
                img.Source = imageData as ImageSource;
                return img;
            }

            if( imageData is Image  )
            {
                //If a WPF image was stored in the PluginDatas, we use its source to create a NEW image instance, to enable using it multiple times. 
                var img = new Image();
                var bitmapImage = new BitmapImage( new Uri( ( (Image)imageData ).Source.ToString() ) )
                {
                    CacheOption = BitmapCacheOption.OnLoad
                };

                img.Source = bitmapImage;
                return img;
            }

            string imageString = imageData.ToString();

            if( imageString.Length <= 260 && Uri.IsWellFormedUriString( imageString, UriKind.RelativeOrAbsolute ) && !imageString.StartsWith( "pack://" ) && File.Exists( imageString ) ) //Handles URis
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri( imageString );
                bitmapImage.EndInit();

                image.Source = bitmapImage;

                return image;
            }

            if( imageString.StartsWith( "pack://" ) ) //Handles the WPF "pack://" protocol
            {
                Image img = new Image();

                ImageSourceConverter imsc = new ImageSourceConverter();
                img.Source = (ImageSource)imsc.ConvertFromString( imageString );
                return img;
            }

            byte[] imageBytes = Convert.FromBase64String( imageData.ToString() ); //Handles base 64 encoded images
            using( MemoryStream ms = new MemoryStream( imageBytes ) )
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                image.Source = bitmapImage;

            }
            return image;
        }
    }
}
