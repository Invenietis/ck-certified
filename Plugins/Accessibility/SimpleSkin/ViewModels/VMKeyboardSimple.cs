using System;
using CK.WPF.ViewModel;
using System.Windows;
using SimpleSkin.Helper;
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleSkin.ViewModels
{
    internal class VMKeyboardSimple : VMKeyboard<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
    {
        double _viewportHeight;
        double _viewportWidth;

        int _x;
        int _y;

        public VMKeyboardSimple( VMContextSimple ctx, IKeyboard kb )
            : base( ctx, kb )
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            _viewportHeight = ViewportSize.Height;
            _viewportWidth = ViewportSize.Width;
            System.Drawing.Point p =  CheckPostion();
            _x = Convert.ToInt32( p.X );
            _y = Convert.ToInt32( p.Y );
        }

        protected override void OnDispose()
        {
            ViewportSize = new Size( _viewportWidth, _viewportHeight );
            Context.Config[Layout]["PositionX"] = _x;
            Context.Config[Layout]["PositionY"] = _y;

            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            base.OnDispose();
        }

        protected override void OnTriggerPropertyChanged()
        {
            int y = Y;
            OnPropertyChanged( "X" );
            Y = y;
            OnPropertyChanged( "Y" );

            double w = ViewportWidth;
            OnPropertyChanged( "ViewportHeight" );
            ViewportWidth = w;
            OnPropertyChanged( "ViewportWidth" );
            
            base.OnTriggerPropertyChanged();
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout)
            {
               if( e.Key == "KeyboardBackground" )
                {
                    OnPropertyChanged( "Background" );                   
                }
               else if( e.Key == "InsideBorderColor" )
               {
                   OnPropertyChanged( "InsideBorderColor" );
               }
               else if( e.Key == "ViewPortSize" )
               {
                   ViewportWidth = ViewportSize.Width;
                   ViewportHeight = ViewportSize.Height;
                   OnPropertyChanged( "ViewportWidth" );
                   OnPropertyChanged( "ViewportHeight" );
               }
            }
        }

        System.Drawing.Point CheckPostion()
        {
            Size sF = ViewportSize;
            System.Drawing.Size s = new System.Drawing.Size( (int)sF.Width, (int)sF.Height );

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
            if( Context.Config[Layout].Contains( "PositionX" ) )
            {
                rect.X = (int)Context.Config[Layout]["PositionX"];
                rect.Y = (int)Context.Config[Layout]["PositionY"];
                rect.Width = s.Width;
                rect.Height = s.Height;
            }
            if( rect.IsEmpty || !ScreenHelper.IsInScreen( rect ) )
            {
                System.Drawing.Point p = ScreenHelper.GetCenterOfParentScreen( rect );
                int hScreen = p.Y * 2;
                rect.X = p.X - (s.Width / 2);
                rect.Y = hScreen - s.Height - 10;
                rect.Width = s.Width;
                rect.Height = s.Height;

                Context.Config[Layout]["PositionX"] = rect.X;
                Context.Config[Layout]["PositionY"] = rect.Y;
            }

            return rect.Location;
        }

        Size ViewportSize
        {
            get { return Context.Config[Layout].GetOrSet( "ViewPortSize", () => new Size( W, H ) ); }
            set { Context.Config[Layout]["ViewPortSize"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the window (not the keyboard, just the viewport).
        /// </summary>
        public double ViewportWidth
        {
            get { return _viewportWidth; }
            set { _viewportWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of the window (not the keyboard, just the viewport).
        /// </summary>
        public double ViewportHeight
        {
            get { return _viewportHeight; }
            set { _viewportHeight = value; }
        }

        /// <summary>
        /// Gets or sets the position of the viewport on the screen.
        /// </summary>
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Gets or sets the position of the viewport on the screen.
        /// </summary>
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public Brush InsideBorderColor 
        { 
            get 
            {
                if( Context.Config[Layout]["InsideBorderColor"] != null )
                    return new SolidColorBrush((Color)Context.Config[Layout]["InsideBorderColor"]);
                return null;
            } 
        }

        public object BackgroundImagePath
        {
            get
            {

                if( Context.Config[Layout]["KeyboardBackground"] == null )
                {
                    Context.Config[Layout]["KeyboardBackground"] = "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png";
                }
                ImageSourceConverter imsc = new ImageSourceConverter();
                return imsc.ConvertFromString( (string)Context.Config[Layout]["KeyboardBackground"] );
            }
        }

        public Brush Background
        {
            get;
            set;
            //get
            //{
            //if( Context.Config[Layout]["KeyboardBackground"] == null || Context.Config[Layout]["KeyboardBackgroundType"] == null )
            //{
            //    Context.Config[Layout]["KeyboardBackgroundType"] = "path";
            //    Context.Config[Layout]["KeyboardBackground"] = "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png";                    
            //}

            ////A kind of neatier way ?
            //object data = Context.Config[Layout]["KeyboardBackground"];
            //object dataType = Context.Config[Layout]["KeyboardBackgroundType"];

            //if( dataType.ToString() == "path" )
            //{
            //    ImageBrush b = new ImageBrush();
            //    ImageSourceConverter conv = new ImageSourceConverter();
            //    BitmapImage bitmapImage = new BitmapImage( new Uri( data.ToString() ) );
            //    b.ImageSource = bitmapImage;
            //    return b;
            //}
            //else if( dataType.ToString() == "image" )
            //    return data as ImageBrush;
            //else if( dataType.ToString() == "color" )
            //    return new SolidColorBrush( (Color)data );
            //return null; 


            //    //The blunt way :
            //    //Checking if the data is a path to an image
            //    //string[] splittedPath = data.ToString().Split('.');
            //    //string extension = splittedPath[splittedPath.Length - 1];
            //    //if( extension == "jpg" ||
            //    //    extension == "jpeg" ||
            //    //    extension == "jpe"  ||
            //    //    extension == "png"  ||
            //    //    extension == "bmp"  ||
            //    //    extension == "svg"  )
            //    //{
            //    //    ImageBrush b = new ImageBrush();
            //    //    ImageSourceConverter conv = new ImageSourceConverter();
            //    //    ImageSource imageSource = (ImageSource)conv.ConvertFrom( data.ToString() );
            //    //    if(imageSource != null)                        
            //    //        b.ImageSource = imageSource;                       
            //    //    return b;
            //    //}
            //    //else if( data as ImageBrush != null ) // Checking if the data is an image
            //    //    return data as ImageBrush;
            //    //else
            //    //{
            //    //    return new SolidColorBrush( (Color)data ); //then the data should be a color                        
            //    //}
            //}
            //return null;
            //}
        }
    }
}
