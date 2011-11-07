using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using SimpleSkin;
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Media;

namespace SimpleSkinEditor
{
    public partial class EditorViewModel
    {
        IEnumerable<double> _sizes;
        VMCommand<string> _clearCmd;

        IEnumerable<double> GetSizes( int from, int to )
        {
            for( int i = from; i <= to; i++ ) yield return i;
        }

        public IEnumerable<double> FontSizes { get { return _sizes == null ? _sizes = GetSizes( 10, 30 ) : _sizes; } }

        public double FontSize
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "FontSize", 15.0 ).Value; }
            set { _config[ConfigHolder]["FontSize"] = value; }
        }

        public bool FontWeight
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "FontWeight", FontWeights.Normal ).Value == FontWeights.Bold; }
            set
            {
                if( value ) _config[ConfigHolder]["FontWeight"] = FontWeights.Bold;
                else _config[ConfigHolder]["FontWeight"] = FontWeights.Normal;
            }
        }

        public bool FontStyle
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "FontStyle", FontStyles.Normal ).Value == FontStyles.Italic; }
            set
            {
                if( value ) _config[ConfigHolder]["FontStyle"] = FontStyles.Italic;
                else _config[ConfigHolder]["FontStyle"] = FontStyles.Normal;
            }
        }

        public bool TextDecorations
        {
            get
            {
                var val = ConfigHolder.GetWrappedPropertyValue<TextDecorationCollection>( _config, "TextDecorations" );
                return val.Value != null && val.Value.Count > 0;
            }
            set
            {
                if( value ) _config[ConfigHolder]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "Underline" );
                else _config[ConfigHolder]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "" );
            }
        }

        public Color FontColor
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "LetterColor", Colors.Black ).Value; }
            set { _config[ConfigHolder]["LetterColor"] = value; }
        }
    }
}
