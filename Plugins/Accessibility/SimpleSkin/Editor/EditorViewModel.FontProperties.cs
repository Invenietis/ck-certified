#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\Editor\EditorViewModel.FontProperties.cs) is part of CiviKey. 
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
