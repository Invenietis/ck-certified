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

        public VMKeyboardSimple( VMContextSimple ctx, IKeyboard kb )
            : base( ctx, kb )
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            _viewportHeight = ViewportSize.Height;
            _viewportWidth = ViewportSize.Width;
            System.Drawing.Point p = CheckPostion();
        }

        protected override void OnDispose()
        {
            ViewportSize = new Size( _viewportWidth, _viewportHeight );
            Context.Config[Layout]["PositionX"] = X;
            Context.Config[Layout]["PositionY"] = Y;

            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            base.OnDispose();
        }

        /// <summary>
        /// When there is a change of Keyboard, need to handle some PropertyChanges manually
        /// </summary>
        protected override void OnTriggerPropertyChanged()
        {
            OnPropertyChanged( "Y" );
            OnPropertyChanged( "X" );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        OnPropertyChanged( "Background" );
                        break;
                    case "InsideBorderColor":
                        OnPropertyChanged( "InsideBorderColor" );
                        break;
                    case "ViewPortSize":
                        OnPropertyChanged( "ViewportHeight" );
                        OnPropertyChanged( "ViewportWidth" );
                        break;
                    case "PositionX":
                        OnPropertyChanged( "X" );
                        break;
                    case "PositionY":
                        OnPropertyChanged( "Y" );
                        break;
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
                rect.X = p.X - ( s.Width / 2 );
                rect.Y = hScreen - s.Height - 10;
                rect.Width = s.Width;
                rect.Height = s.Height;

                X = rect.X;
                X = rect.Y;
            }

            return rect.Location;
        }

        Size ViewportSize
        {
            get { return Context.Config[Layout].GetOrSet( "ViewPortSize", new Size( W, H ) ); }
            set { Context.Config[Layout]["ViewPortSize"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the window (not the keyboard, just the viewport).
        /// </summary>
        public double ViewportWidth
        {
            get { return ViewportSize.Width; }
            set { _viewportWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of the window (not the keyboard, just the viewport).
        /// </summary>
        public double ViewportHeight
        {
            get { return ViewportSize.Height; }
            set { _viewportHeight = value; }
        }

        /// <summary>
        /// Gets or sets the position of the viewport on the screen.
        /// </summary>
        public int X
        {
            get { return Context.Config[Layout].GetOrSet( "PositionX", 300 ); }
            set { Context.Config[Layout].Set( "PositionX", value ); }
        }

        /// <summary>
        /// Gets or sets the position of the viewport on the screen.
        /// </summary>
        public int Y
        {
            get { return Context.Config[Layout].GetOrSet( "PositionY", 300 ); }
            set { Context.Config[Layout].Set( "PositionY", value ); }
        }

        public Brush InsideBorderColor
        {
            get
            {
                if( Context.Config[Layout]["InsideBorderColor"] != null )
                    return new SolidColorBrush( (Color)Context.Config[Layout]["InsideBorderColor"] );
                return null;
            }
        }

        ImageSourceConverter imsc;
        public object BackgroundImagePath
        {
            get
            {
                if( imsc == null ) imsc = new ImageSourceConverter();
                return imsc.ConvertFromString( Context.Config[Layout].GetOrSet( "KeyboardBackground", "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png" ) );
            }
        }
    }
}
