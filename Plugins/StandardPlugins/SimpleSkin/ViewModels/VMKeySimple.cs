using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using SimpleSkin.Helper;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;

namespace SimpleSkin.ViewModels
{
    internal class VMKeySimple : VMKey<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
    {
        public VMKeySimple( VMContextSimple ctx, IKey k ) 
            : base( ctx, k )
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "ShowLabel" );
            } );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "ShowLabel" );
            }
        }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
            base.OnDispose();
        }

        public Image Image
        {
            get { return LayoutKeyMode.GetPropertyValue<Image>( Context.Config, "Image" ); }
        }

        public Color Background
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ); }
        }

        public Color HoverBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ); }
        }

        public Color PressedBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ); }
        }

        public Color LetterColor
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ); }
        }

        public FontStyle FontStyle
        {
            get { return LayoutKeyMode.GetPropertyValue<FontStyle>( Context.Config, "FontStyle" ); }
        }

        public FontWeight FontWeight
        {
            get { return LayoutKeyMode.GetPropertyValue<FontWeight>( Context.Config, "FontWeight", FontWeights.Normal ); }
        }

        public double FontSize
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return LayoutKeyMode.GetPropertyValue<TextDecorationCollection>( Context.Config, "TextDecorations" ); }
        }

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        public double Opacity
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
        }
    }
}
