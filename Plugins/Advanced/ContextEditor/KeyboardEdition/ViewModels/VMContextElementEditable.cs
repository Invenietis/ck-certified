#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\ViewModels\VMContextElementEditable.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.WPF.ViewModel;
namespace KeyboardEditor.ViewModels
{
    /// <summary>
    /// </summary>
    public abstract class VMContextElementEditable : VMBase
    {
        VMContextEditable _context;

        protected VMContextElementEditable( VMContextEditable context )
        {
            _context = context;
            if( _context.SkinConfiguration != null )
                _context.SkinConfiguration.ConfigChanged += OnLayoutConfigChanged;
        }

        public ICollection<FontFamily> FontFamilies
        {
            get { return Fonts.SystemFontFamilies; }
        }

        /// <summary>
        /// Gets whether this element is being edited.
        /// An element is being edited if it IsSelected or one of its parents is being edited.
        /// can be overidden.
        /// </summary>
        public virtual bool IsBeingEdited
        {
            get { return IsSelected || Parent.IsBeingEdited; }
        }

        /// <summary>
        /// Gets whether the element is selected.
        /// </summary>
        public abstract bool IsSelected { get; set; }

        bool _isExpanded;
        /// <summary>
        /// Gets whether the element is expanded. Used to bind a treeview to a context element.
        /// Should only be used on an editor.
        /// </summary>
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if( value && Parent != null )
                {
                    Parent.IsExpanded = true;
                }
                OnPropertyChanged( "IsExpanded" );
            }
        }

        /// <summary>
        /// Override this method to trigger certain actions when the user presses keys while this ContextElement is selected
        /// </summary>
        /// <param name="keyCode">The keycode of the key that has been pressed and retrieved from the message pump</param>
        /// <param name="multiplier">optional integer</param>
        public virtual void OnKeyDownAction( int keyCode, int multiplier )
        {
        }

        private IEnumerable<VMContextElementEditable> GetParents()
        {
            VMContextElementEditable elem = this;
            while( elem != null )
            {
                elem = elem.Parent;

                if( elem != null )
                    yield return elem;
            }
        }

        /// <summary>
        /// Gets the parents of the element.
        /// </summary>
        public IEnumerable<VMContextElementEditable> Parents { get { return GetParents().Reverse(); } }

        /// <summary>
        /// Gets the parent of the object.
        /// </summary>
        public abstract VMContextElementEditable Parent { get; }

        /// <summary>
        /// Gets the <see cref="VMContextSimple"/> to which this element belongs.
        /// </summary>
        public VMContextEditable Context { get { return _context; } }

        /// <summary>
        /// Internal method called by this <see cref="Context"/> only.
        /// </summary>
        internal virtual void Dispose()
        {
            UnregisterEvents();
        }

        private void UnregisterEvents()
        {
            _context.SkinConfiguration.ConfigChanged -= OnLayoutConfigChanged;
        }

        void OnLayoutConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => String.Compare( c.UniqueId.ToString(), "36C4764A-111C-45E4-83D6-E38FC1DF5979", StringComparison.InvariantCultureIgnoreCase ) == 0 ) )
            {
                switch( e.Key )
                {
                    case "Background":
                    case "HoverBackground":
                    case "PressedBackground":
                    case "FontSize":
                    case "FontWeight":
                    case "FontSizes":
                    case "FontStyle":
                    case "TextDecorations":
                    case "FontColor":
                    case "FontFamily":
                    case "LetterColor":
                    case "HighlightBackground":
                    case "HighlightFontColor":
                        OnPropertyChanged( "TextDecorationsAsBool" );
                        OnPropertyChanged( "PressedBackground" );
                        OnPropertyChanged( "FontWeightAsBool" );
                        OnPropertyChanged( "FontStyleAsBool" );
                        OnPropertyChanged( "TextDecorations" );
                        OnPropertyChanged( "HoverBackground" );
                        OnPropertyChanged( "LetterColor" );
                        OnPropertyChanged( "FontWeight" );
                        OnPropertyChanged( "Background" );
                        OnPropertyChanged( "FontWeight" );
                        OnPropertyChanged( "FontStyle" );
                        OnPropertyChanged( "FontSizes" );
                        OnPropertyChanged( "FontStyle" );
                        OnPropertyChanged( "FontSize" );
                        OnPropertyChanged( "FontFamily" );
                        OnPropertyChanged( "HighlightBackground" );
                        OnPropertyChanged( "HighlightFontColor" );
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the element to which (IKeyboard) element plugindatas should be attached
        /// </summary>
        public abstract IKeyboardElement LayoutElement { get; }

        #region Layout Edition elements

        VMCommand<string> _clearCmd;
        public VMCommand<string> ClearPropertyCmd { get { return _clearCmd ?? ( _clearCmd = new VMCommand<string>( ClearProperty, CanClearProperty ) ); } }

        void ClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );
            foreach( var pname in names ) _context.SkinConfiguration[LayoutElement].Remove( pname );
        }

        bool CanClearProperty( string propertyName )
        {
            string[] names = propertyName.Split( ',' );

            // We can clear property if the property owns directly a value.
            foreach( var pname in names ) if( _context.SkinConfiguration[LayoutElement][pname] != null ) return true;
            return false;
        }

        public Color Background
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "Background", Colors.White ); }
            set { _context.SkinConfiguration[LayoutElement]["Background"] = value; }
        }

        public Color HoverBackground
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "HoverBackground", Background ); }
            set { _context.SkinConfiguration[LayoutElement]["HoverBackground"] = value; }
        }

        public Color HighlightBackground
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "HighlightBackground", Background ); }
            set { _context.SkinConfiguration[LayoutElement]["HighlightBackground"] = value; }
        }

        public Color HighlightFontColor
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "HighlightFontColor", Colors.Black ); }
            set { _context.SkinConfiguration[LayoutElement]["HighlightFontColor"] = value; }
        }

        public Color PressedBackground
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "PressedBackground", HoverBackground ); }
            set { _context.SkinConfiguration[LayoutElement]["PressedBackground"] = value; }
        }

        public Color LetterColor
        {
            get { return LayoutElement.GetPropertyValue( _context.SkinConfiguration, "LetterColor", Colors.Black ); }
            set { _context.SkinConfiguration[LayoutElement]["LetterColor"] = value; }
        }

        public FontStyle FontStyle
        {
            get { return LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontStyle", FontStyles.Normal ).Value; }
        }

        public FontWeight FontWeight
        {
            get { return LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontWeight", FontWeights.Normal ).Value; }
        }

        public FontFamily FontFamily
        {
            get 
            { 
                if( LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontFamily", "Arial" ).Value.Contains( "pack://" ) )
                {
                    string[] split = LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontFamily", "Arial" ).Value.Split( '|' );
                    return new FontFamily( new Uri( split[0] ), split[1] );
                }
                else
                {
                    return new FontFamily( LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontFamily", "Arial" ).Value );
                }
            }
            set
            {
                if( value.BaseUri == null )
                {
                    _context.SkinConfiguration[LayoutElement]["FontFamily"] = value.ToString();
                }
                else
                {
                    _context.SkinConfiguration[LayoutElement]["FontFamily"] = value.BaseUri.OriginalString + "|" + value.ToString();
                }
            }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return LayoutElement.GetWrappedPropertyValue<TextDecorationCollection>( _context.SkinConfiguration, "TextDecorations" ).Value; }
        }

        public double FontSize
        {
            get { return LayoutElement.GetPropertyValue<double>( _context.SkinConfiguration, "FontSize", 15 ); }
            set
            {
                _context.SkinConfiguration[LayoutElement]["FontSize"] = value;
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
            get { return LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontStyle", FontStyles.Normal ).Value == FontStyles.Italic; }
            set
            {
                if( value ) _context.SkinConfiguration[LayoutElement]["FontStyle"] = FontStyles.Italic;
                else _context.SkinConfiguration[LayoutElement]["FontStyle"] = FontStyles.Normal;
            }
        }

        /// <summary>
        /// Gets whether the FontWeight of the Current LayoutKeyMode is <see cref="FontWeights.Bold"/>.
        /// Returns false otherwise.
        /// Used to bind the FontWeight to a boolean control ( for example, a <see cref="ToggleButton"/>)
        /// </summary>
        public bool FontWeightAsBool
        {
            get { return LayoutElement.GetWrappedPropertyValue( _context.SkinConfiguration, "FontWeight", FontWeights.Normal ).Value == FontWeights.Bold; }
            set
            {
                if( value ) _context.SkinConfiguration[LayoutElement]["FontWeight"] = FontWeights.Bold;
                else _context.SkinConfiguration[LayoutElement]["FontWeight"] = FontWeights.Normal;
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
                var val = LayoutElement.GetWrappedPropertyValue<TextDecorationCollection>( _context.SkinConfiguration, "TextDecorations" );
                return val.Value != null && val.Value.Count > 0;
            }
            set
            {
                if( value ) _context.SkinConfiguration[LayoutElement]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "Underline" );
                else _context.SkinConfiguration[LayoutElement]["TextDecorations"] = TextDecorationCollectionConverter.ConvertFromString( "" );
            }
        }

        IEnumerable<double> _sizes;
        IEnumerable<double> GetSizes( int from, int to )
        {
            for( int i = from; i <= to; i++ ) yield return i;
        }
        public IEnumerable<double> FontSizes { get { return _sizes ?? (_sizes = GetSizes( 10, 50 )); } }

        IEnumerable<int> _loopCounts;
        IEnumerable<int> GetLoopCounts( int from, int to )
        {
            for( int i = from; i <= to; i++ ) yield return i;
        }
        public IEnumerable<int> LoopCounts { get { return _loopCounts ?? (_loopCounts = GetLoopCounts( 1, 5 )); } }


        #endregion

        #endregion

    }
}