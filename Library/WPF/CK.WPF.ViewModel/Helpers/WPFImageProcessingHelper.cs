using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
            else if( Uri.IsWellFormedUriString( imageString, UriKind.RelativeOrAbsolute ) && File.Exists( imageString ) ) //Handles URis
            {
                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri( imageString );
                bitmapImage.EndInit();

                image.Source = bitmapImage;

                return image;
            }
            else if( imageString.StartsWith( "pack://" ) ) //Handles the WPF "pack://" protocol
            {
                Image img = new Image();
                
                ImageSourceConverter imsc = new ImageSourceConverter();
                img.Source = (ImageSource)imsc.ConvertFromString( imageString );
                return img;
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
    }
}
