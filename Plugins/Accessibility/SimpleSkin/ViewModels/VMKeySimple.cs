#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMKeySimple.cs) is part of CiviKey. 
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
using HighlightModel;
using CK.Core;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xaml;
using System.Diagnostics;
using CK.Storage;

namespace SimpleSkin.ViewModels
{
    public class VMKeySimple : VMContextElement, IHighlightableElement
    {
        Dictionary<string, ActionSequence> _actionsOnPropertiesChanged;
        ICommand _keyPressedCmd;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        IKey _key;

        internal VMKeySimple( VMContextSimple ctx, IKey k )
            : base( ctx )
        {
            _actionsOnPropertiesChanged = new Dictionary<string, ActionSequence>();
            _key = k;

            ResetCommands();
            RegisterEvents();

            SafeUpdateBackground();
            SafeUpdateDescription();
            SafeUpdateDownLabel();
            SafeUpdateFontSize();
            SafeUpdateFontStyle();
            SafeUpdateFontWeight();
            SafeUpdateHeight();
            SafeUpdateHighlightBackground();
            SafeUpdateHoverBackground();
            SafeUpdateImage();
            SafeUpdateIndex();
            SafeUpdateIsEnabled();
            SafeUpdateIsFallback();
            SafeUpdateLetterColor();
            SafeUpdateOpacity();
            SafeUpdatePressedBackground();
            SafeUpdateShowImage();
            SafeUpdateShowLabel();
            SafeUpdateUpLabel();
            SafeUpdateVisible();
            SafeUpdateWidth();
            SafeUpdateX();
            SafeUpdateY();

        }

        #region OnXXX

        public void OnKeyPropertyChanged( object sender, KeyPropertyChangedEventArgs e )
        {
            if( _actionsOnPropertiesChanged.ContainsKey( e.PropertyName ) )
                _actionsOnPropertiesChanged[e.PropertyName].Run();
        }

        internal override void Dispose()
        {
            UnregisterEvents();
        }

        void OnCurrentModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            OnPropertyChanged( "IsFallback" );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                PropertyChangedTriggers( e.Key );
            }
        }

        private void RegisterEvents()
        {
            SetActionOnPropertyChanged( "Current", () =>
            {
                SafeUpdateUpLabel();
                SafeUpdateDownLabel();
                SafeUpdateIsEnabled();
                OnPropertyChanged( "UpLabel" );
                OnPropertyChanged( "DownLabel" );
                OnPropertyChanged( "Enabled" );
            } );

            SetActionOnPropertyChanged( "X", () => { SafeUpdateX(); OnPropertyChanged( "X" ); } );
            SetActionOnPropertyChanged( "Y", () => { SafeUpdateY(); OnPropertyChanged( "Y" ); } );
            SetActionOnPropertyChanged( "W", () => { SafeUpdateWidth(); OnPropertyChanged( "Width" ); } );
            SetActionOnPropertyChanged( "H", () => { SafeUpdateHeight(); OnPropertyChanged( "Height" ); } );
            SetActionOnPropertyChanged( "Visible", () => { SafeUpdateVisible(); OnPropertyChanged( "Visible" ); } );
            SetActionOnPropertyChanged( "Enabled", () => { SafeUpdateIsEnabled(); OnPropertyChanged( "Enabled" ); } );
            SetActionOnPropertyChanged( "UpLabel", () => { SafeUpdateUpLabel(); OnPropertyChanged( "UpLabel" ); } );
            SetActionOnPropertyChanged( "DownLabel", () => { SafeUpdateDownLabel(); OnPropertyChanged( "DownLabel" ); } );
            SetActionOnPropertyChanged( "CurrentLayout", () => { PropertyChangedTriggers(); } );

            _key.KeyPropertyChanged += new EventHandler<KeyPropertyChangedEventArgs>( OnKeyPropertyChanged );
            _key.Keyboard.CurrentModeChanged += new EventHandler<KeyboardModeChangedEventArgs>( OnCurrentModeChanged );
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
        }

        private void UnregisterEvents()
        {
            _actionsOnPropertiesChanged.Clear();

            _key.KeyPropertyChanged -= new EventHandler<KeyPropertyChangedEventArgs>( OnKeyPropertyChanged );
            _key.Keyboard.CurrentModeChanged -= new EventHandler<KeyboardModeChangedEventArgs>( OnCurrentModeChanged );
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );
        }

        private void PropertyChangedTriggers( string propertyName = "" )
        {
            if( String.IsNullOrWhiteSpace( propertyName ) )
            {
                SafeUpdateOpacity();
                SafeUpdateFontSize();
                SafeUpdateFontStyle();
                SafeUpdateShowLabel();
                SafeUpdateShowImage();
                SafeUpdateFontWeight();
                SafeUpdateBackground();
                SafeUpdateLetterColor();
                SafeUpdateHoverBackground();
                SafeUpdateTextDecorations();
                SafeUpdatePressedBackground();
                SafeUpdateHighlightBackground();

                //These properties are on the LayoutKeyMode (not in the key's plugindatas), so we won't have the Config telling us that they have changed
                //OnPropertyChanged( "X" );
                //OnPropertyChanged( "Y" );
                //OnPropertyChanged( "Width" );
                //OnPropertyChanged( "Height" );
                //OnPropertyChanged( "Visible" );

                OnPropertyChanged( "Opacity" );
                OnPropertyChanged( "Image" );
                OnPropertyChanged( "FontSize" );
                OnPropertyChanged( "FontStyle" );
                OnPropertyChanged( "ShowLabel" );
                OnPropertyChanged( "ShowImage" );
                OnPropertyChanged( "FontWeight" );
                OnPropertyChanged( "Background" );
                OnPropertyChanged( "LetterColor" );
                OnPropertyChanged( "HoverBackground" );
                OnPropertyChanged( "TextDecorations" );
                OnPropertyChanged( "PressedBackground" );
                OnPropertyChanged( "HighlightBackground" );
            }
            else
            {
                switch( propertyName )
                {
                    case "Opacity":
                        SafeUpdateOpacity();
                        OnPropertyChanged( "Opacity" );
                        break;
                    case "Image":
                        SafeUpdateImage();
                        OnPropertyChanged( "Image" );
                        break;
                    case "Visible":
                        SafeUpdateVisible();
                        OnPropertyChanged( "Visible" );
                        break;
                    case "FontSize":
                        SafeUpdateFontSize();
                        OnPropertyChanged( "FontSize" );
                        break;
                    case "FontStyle":
                        SafeUpdateFontStyle();
                        OnPropertyChanged( "FontStyle" );
                        break;
                    case "ShowLabel":
                        SafeUpdateShowLabel();
                        OnPropertyChanged( "ShowLabel" );
                        break;
                    case "ShowImage":
                        SafeUpdateShowImage();
                        OnPropertyChanged( "ShowImage" );
                        break;
                    case "FontWeight":
                        SafeUpdateFontWeight();
                        OnPropertyChanged( "FontWeight" );
                        break;
                    case "Background":
                        SafeUpdateBackground();
                        OnPropertyChanged( "Background" );
                        break;
                    case "LetterColor":
                        SafeUpdateLetterColor();
                        OnPropertyChanged( "LetterColor" );
                        break;
                    case "HoverBackground":
                        SafeUpdateHoverBackground();
                        OnPropertyChanged( "HoverBackground" );
                        break;
                    case "TextDecorations":
                        SafeUpdateTextDecorations();
                        OnPropertyChanged( "TextDecorations" );
                        break;
                    case "PressedBackground":
                        SafeUpdatePressedBackground();
                        OnPropertyChanged( "PressedBackground" );
                        break;
                    case "HighlightBackground":
                        SafeUpdateHighlightBackground();
                        OnPropertyChanged( "HighlightBackground" );
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Tool methods

        void SetActionOnPropertyChanged( string propertyName, Action action )
        {
            if( !_actionsOnPropertiesChanged.ContainsKey( propertyName ) )
            {
                if( action != null )
                {
                    ActionSequence actions = new ActionSequence();
                    actions.Append( action );
                    _actionsOnPropertiesChanged.Add( propertyName, actions );
                }
            }
            else
            {
                if( action != null )
                    _actionsOnPropertiesChanged[propertyName].Append( action );
            }
        }

        void ResetCommands()
        {
            _keyDownCmd = new KeyCommand( () => { if( !_key.IsDown )_key.Push(); } );
            _keyUpCmd = new KeyCommand( () => { if( _key.IsDown ) _key.Release(); } );
            _keyPressedCmd = new KeyCommand( () => { if( _key.IsDown )_key.Release( true ); } );
        }

        private void SafeUpdateX()
        {
            ThreadSafeSet<int>( _key.CurrentLayout.Current.X, ( v ) => _x = v );
        }

        private void SafeUpdateY()
        {
            ThreadSafeSet<int>( _key.CurrentLayout.Current.Y, ( v ) => _y = v );
        }

        private void SafeUpdateHeight()
        {
            ThreadSafeSet<int>( _key.CurrentLayout.Current.Height, ( v ) => _height = v );
        }

        private void SafeUpdateWidth()
        {
            ThreadSafeSet<int>( _key.CurrentLayout.Current.Width, ( v ) => _width = v );
        }

        private void SafeUpdateIsEnabled()
        {
            ThreadSafeSet<bool>( _key.Current.Enabled, ( v ) => _isEnabled = v );
        }

        private void SafeUpdateUpLabel()
        {
            ThreadSafeSet<string>( _key.Current.UpLabel, ( v ) => _upLabel = v );
        }

        private void SafeUpdateDownLabel()
        {
            ThreadSafeSet<string>( _key.Current.DownLabel, ( v ) => _downLabel = v );
        }

        private void SafeUpdateDescription()
        {
            ThreadSafeSet<string>( _key.Current.Description, ( v ) => _description = v );
        }

        private void SafeUpdateIndex()
        {
            ThreadSafeSet<int>( _key.Index, ( v ) => _index = v );
        }

        private void SafeUpdateIsFallback()
        {
            ThreadSafeSet<bool>( _key.Current.IsFallBack, ( v ) => _isFallback = v );
        }

        private void SafeUpdateVisible()
        {
            ThreadSafeSet<bool>( LayoutKeyMode.Visible, ( v ) => _visible = v );
        }

        private void SafeUpdateImage()
        {
            object o = Context.Config[LayoutKeyMode].GetOrSet<object>( "Image", null );

            if( o != null )
            {
                string source = String.Empty;

                if( o.GetType() == typeof( Image ) ) //If there is an image in the config, the SkinThread needs to deserialize the image, in order to be its owner.
                {
                    source = ( (Image)o ).Source.ToString();
                }
                else //otherwise, the config only holds a string. ProcessImage will therefor work properly.
                {
                    source = o.ToString();
                }

                ThreadSafeSet<string>( source, ( v ) =>
                {
                    _image = WPFImageProcessingHelper.ProcessImage( v );
                } );
            }
            else _image = null;
        }

        private void SafeUpdateBackground()
        {
            ThreadSafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ), ( v ) => _background = v );
        }

        private void SafeUpdateHoverBackground()
        {
            ThreadSafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ), ( v ) => _hoverBackground = v );
        }

        private void SafeUpdateHighlightBackground()
        {
            ThreadSafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ), ( v ) => _highlightBackground = v );
        }

        private void SafeUpdatePressedBackground()
        {
            ThreadSafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ), ( v ) => _pressedBackground = v );
        }

        private void SafeUpdateLetterColor()
        {
            ThreadSafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ), ( v ) => _letterColor = v );
        }

        private void SafeUpdateFontSize()
        {
            ThreadSafeSet<double>( LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ), ( v ) => _fontSize = v );
        }

        private void SafeUpdateFontStyle()
        {
            ThreadSafeSet<FontStyle>( LayoutKeyMode.GetPropertyValue<FontStyle>( Context.Config, "FontStyle" ), ( v ) => _fontStyle = v );
        }

        private void SafeUpdateFontWeight()
        {
            ThreadSafeSet<FontWeight>( LayoutKeyMode.GetPropertyValue<FontWeight>( Context.Config, "FontWeight", FontWeights.Normal ), ( v ) => _fontWeight = v );
        }

        private void SafeUpdateShowLabel()
        {
            ThreadSafeSet<bool>( LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ), ( v ) => _showLabel = v );
        }

        private void SafeUpdateShowImage()
        {
            ThreadSafeSet<bool>( LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowImage", true ), ( v ) => _showImage = v );
        }

        private void SafeUpdateOpacity()
        {
            ThreadSafeSet<double>( LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ), ( v ) => _opacity = v );
        }

        private void SafeUpdateTextDecorations()
        {
            MemoryStream stream = new MemoryStream();
            TextDecorationCollection obj = LayoutKeyMode.GetPropertyValue<TextDecorationCollection>( Context.Config, "TextDecorations" );
            if( obj != null ) System.Windows.Markup.XamlWriter.Save( obj, stream );
            stream.Seek( 0, SeekOrigin.Begin );
            ThreadSafeSet<Stream>( stream, ( v ) =>
            {
                if( stream.Length > 0 )
                    _textDecorations = (TextDecorationCollection)System.Windows.Markup.XamlReader.Load( stream );
                else
                    _textDecorations = null;

                stream.Dispose();
            } );
        }

        #endregion

        #region "Design" properties

        //TODO : other way
        public ILayoutKeyMode LayoutKeyMode
        {
            get { return LayoutKey.Current; }
        }

        //TODO : other way
        public ILayoutKey LayoutKey
        {
            get { return _key.CurrentLayout; }
        }

        private int _x;
        /// <summary>
        /// Gets the X coordinate of this key.
        /// </summary>
        public int X
        {
            //get { return _key.CurrentLayout.Current.X; }
            get { return _x; }
        }

        private int _y;
        /// <summary>
        /// Gets the Y coordinate of this key.
        /// </summary>
        public int Y
        {
            //get { return _key.CurrentLayout.Current.Y; }
            get { return _y; }
        }

        private int _width;
        /// <summary>
        /// Gets the width of this key.
        /// </summary>
        public int Width
        {
            //get { return _key.CurrentLayout.Current.Width; }
            get { return _width; }
        }

        private int _height;
        /// <summary>
        /// Gets the height of this key.
        /// </summary>
        public int Height
        {
            //get { return _key.CurrentLayout.Current.Height; }
            get { return _height; }
        }

        private bool _visible;
        /// <summary>
        /// Gets a value indicating whether this actual key is visible or not.
        /// </summary>
        public Visibility Visible
        {
            //get { return LayoutKeyMode.Visible ? Visibility.Visible : Visibility.Collapsed; }
            get { return _visible ? Visibility.Visible : Visibility.Collapsed; }
        }

        bool _isEnabled;
        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled
        {
            //get { return _key.Current.Enabled; } 
            get { return _isEnabled; }
        }

        private string _upLabel;
        /// <summary>
        /// Gets the label that must be used when the key is up.
        /// </summary>
        public string UpLabel
        {
            //get { return _key.Current.UpLabel; }
            get { return _upLabel; }
        }

        private string _downLabel;
        /// <summary>
        /// Gets the label that must be used when the key is down.
        /// </summary>
        public string DownLabel
        {
            //get { return _key.Current.DownLabel; }
            get { return _downLabel; }
        }

        private string _description;
        /// <summary>
        /// Gets the description of the key
        /// </summary>
        public string Description
        {
            //get { return _key.Current.Description; }
            get { return _description; }
        }

        private int _index;
        /// <summary>
        /// Gets the logical position of the <see cref="IKey"/> in the zone.
        /// </summary>
        public int Index
        {
            //get { return _key.Index; }
            get { return _index; }
            set
            {
                ThreadSafeSet<int>( value, ( v ) => _key.Index = v );
                OnPropertyChanged( "Index" );
            }
        }

        private bool _isFallback;
        /// <summary>
        /// Gets if the current keymode is a fallback or not.
        /// </summary>
        public bool IsFallback
        {
            //get { return _key.Current.IsFallBack; } 
            get { return _isFallback; }

        }

        Image _image;
        public Image Image { get { return _image; } }

        Color _background;
        public Color Background
        {
            //get { return LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ); }
            get { return _background; }
        }

        Color _hoverBackground;
        public Color HoverBackground
        {
            //get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ); }
            get { return _hoverBackground; }
        }

        Color _highlightBackground;
        public Color HighlightBackground
        {
            //get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ); }
            get { return _highlightBackground; }
        }

        Color _pressedBackground;
        public Color PressedBackground
        {
            //get { return LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ); }
            get { return _pressedBackground; }
        }

        Color _letterColor;
        public Color LetterColor
        {
            //get { return LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ); }
            get { return _letterColor; }
        }

        FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            //get { return LayoutKeyMode.GetPropertyValue<FontStyle>( Context.Config, "FontStyle" ); }
            get { return _fontStyle; }
        }

        FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            //get { return LayoutKeyMode.GetPropertyValue<FontWeight>( Context.Config, "FontWeight", FontWeights.Normal ); }
            get { return _fontWeight; }
        }

        double _fontSize;
        public double FontSize
        {
            //get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ); }
            get { return _fontSize; }
        }

        TextDecorationCollection _textDecorations;
        public TextDecorationCollection TextDecorations
        {
            //get { return LayoutKeyMode.GetPropertyValue<TextDecorationCollection>( Context.Config, "TextDecorations" ); }
            get { return _textDecorations; }
        }

        bool _showLabel;
        public bool ShowLabel
        {
            //get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
            get { return _showLabel; }
        }

        bool _showImage;
        public bool ShowImage
        {
            //get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowImage", true ); }
            get { return _showImage; }
        }

        double _opacity;
        public double Opacity
        {
            //get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
            get { return _opacity; }
        }

        public double ZIndex
        {
            get { return 100; }
        }

        #endregion

        #region Commands

        public ICommand KeyDownCommand { get { return _keyDownCmd; } }

        public ICommand KeyUpCommand { get { return _keyUpCmd; } }

        public ICommand KeyPressedCommand { get { return _keyPressedCmd; } }

        #endregion

        #region IHighlightableElement Members

        bool _isHighlighting;
        public bool IsHighlighting
        {
            get { return _isHighlighting; }
            set
            {
                if( value != _isHighlighting )
                {
                    ThreadSafeSet<bool>( value, ( v ) => _isHighlighting = v );
                    OnPropertyChanged( "IsHighlighting" );
                }
            }
        }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public SkippingBehavior Skip
        {
            get { return Visible != Visibility.Visible ? SkippingBehavior.Skip : SkippingBehavior.None; }
        }

        #endregion

        internal class KeyCommand : ICommand
        {
            Action _del;

            public KeyCommand( Action del )
            {
                _del = del;
            }

            public bool CanExecute( object parameter )
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute( object parameter )
            {
                _del();
            }
        }
    }
}
