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
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows;
using CK.Plugin.Config;
using HighlightModel;
using CK.Core;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using CK.Storage;
using Color = System.Windows.Media.Color;
using FontStyle = System.Windows.FontStyle;
using Image = System.Windows.Controls.Image;
using System.Windows.Interop;
using CK.Windows;
using System.Diagnostics;
using System.Windows.Threading;

namespace SimpleSkin.ViewModels
{
    public class VMKeySimple : VMContextElement, IHighlightableElement
    {
        Dictionary<string, ActionSequence> _actionsOnPropertiesChanged;
        ICommand _keyPressedCmd;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        IKey _key;

        internal VMKeySimple( VMContextSimpleBase ctx, IKey k )
            : base( ctx )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
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
            SafeUpdateHighlightFontColor();
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
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( _actionsOnPropertiesChanged.ContainsKey( e.PropertyName ) )
                _actionsOnPropertiesChanged[e.PropertyName].Run();
        }

        internal override void Dispose()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            UnregisterEvents();
        }

        void OnCurrentModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            OnPropertyChanged( "IsFallback" );
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( LayoutKeyMode.GetPropertyLookupPath().Contains( e.Obj ) )
            {
                LayoutPropertyChangedTriggers( e.Key );
            }

            if( _key.Current.GetPropertyLookupPath().Contains( e.Obj ) )
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
            SetActionOnPropertyChanged( "CurrentLayout", () => LayoutPropertyChangedTriggers() );

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
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            switch( propertyName )
            {
                case "Image":
                    SafeUpdateImage();
                    OnPropertyChanged( "Image" );
                    break;
                case "DisplayType":
                    SafeUpdateShowLabel();
                    SafeUpdateShowImage();
                    OnPropertyChanged( "ShowLabel" );
                    OnPropertyChanged( "ShowImage" );
                    break;
            }
        }
        private void LayoutPropertyChangedTriggers( string propertyName = "" )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( String.IsNullOrWhiteSpace( propertyName ) )
            {
                SafeUpdateX();
                SafeUpdateY();
                SafeUpdateWidth();
                SafeUpdateHeight();
                SafeUpdateVisible();
                OnPropertyChanged( "X" );
                OnPropertyChanged( "Y" );
                OnPropertyChanged( "Width" );
                OnPropertyChanged( "Height" );
                OnPropertyChanged( "Visible" );

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
                SafeUpdateHighlightFontColor();

                OnPropertyChanged( "Image" );
                OnPropertyChanged( "Opacity" );
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
                OnPropertyChanged( "HighlightFontColor" );
            }
            else
            {
                switch( propertyName )
                {
                    case "Opacity":
                        SafeUpdateOpacity();
                        OnPropertyChanged( "Opacity" );
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
                    case "HighlightFontColor":
                        SafeUpdateHighlightFontColor();
                        OnPropertyChanged( "HighlightFontColor" );
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
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher );
            _keyDownCmd = new KeyCommand( () =>
            {
                Context.NoFocusManager.ExternalDispatcher.BeginInvoke( (Action)(() => _key.Push()), null );
            } );

            _keyUpCmd = new KeyCommand( () =>
            {
                Context.NoFocusManager.ExternalDispatcher.BeginInvoke( (Action)(() => _key.Release()), null );
            } );

            _keyPressedCmd = new KeyCommand( () =>
            {
                Context.NoFocusManager.ExternalDispatcher.BeginInvoke( (Action)(() => _key.Release( true )), null );
            } );
        }

        private void SafeUpdateX()
        {
            SafeSet<int>( _key.CurrentLayout.Current.X, ( v ) => _x = v );
        }

        private void SafeUpdateY()
        {
            SafeSet<int>( _key.CurrentLayout.Current.Y, ( v ) => _y = v );
        }

        private void SafeUpdateHeight()
        {
            SafeSet<int>( _key.CurrentLayout.Current.Height, ( v ) => _height = v );
        }

        private void SafeUpdateWidth()
        {
            SafeSet<int>( _key.CurrentLayout.Current.Width, ( v ) => _width = v );
        }

        private void SafeUpdateIsEnabled()
        {
            SafeSet<bool>( _key.Current.Enabled, ( v ) => _isEnabled = v );
        }

        private void SafeUpdateUpLabel()
        {
            SafeSet<string>( _key.Current.UpLabel, ( v ) => _upLabel = v );
        }

        private void SafeUpdateDownLabel()
        {
            SafeSet<string>( _key.Current.DownLabel, ( v ) => _downLabel = v );
        }

        private void SafeUpdateDescription()
        {
            SafeSet<string>( _key.Current.Description, ( v ) => _description = v );
        }

        private void SafeUpdateIndex()
        {
            SafeSet<int>( _key.Index, ( v ) => _index = v );
        }

        private void SafeUpdateIsFallback()
        {
            SafeSet<bool>( _key.Current.IsFallBack, ( v ) => _isFallback = v );
        }

        private void SafeUpdateVisible()
        {
            SafeSet<bool>( LayoutKeyMode.Visible, ( v ) => _visible = v );
        }

        private void SafeUpdateImage()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            var o = Context.Config[_key.Current].GetOrSet<object>( "Image", null );

            if( o != null )
            {
                object source = String.Empty;

                if( o is Image ) //If there is an image in the config, the NoFocus Thread needs to deserialize the image, in order to be its owner.
                {
                    source = ((Image)o).Source.ToString();
                }
                else if( o is BitmapSource )
                {
                    ((ImageSource)o).Freeze();
                    source = o;
                }
                else //otherwise, the config only holds a string. ProcessImage will therefor work properly.
                {
                    source = o.ToString();
                }

                SafeSet( source, v =>
                {
                    _image = WPFImageProcessingHelper.ProcessImage( v );
                    OnPropertyChanged( "Image" );
                } );
            }
            else _image = null;
        }

        private void SafeUpdateBackground()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ), ( v ) => _background = v );
        }

        private void SafeUpdateHoverBackground()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ), ( v ) => _hoverBackground = v );
        }

        private void SafeUpdateHighlightBackground()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ), ( v ) => _highlightBackground = v );
        }

        private void SafeUpdateHighlightFontColor()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightFontColor", Background ), ( v ) => _highlightFontColor = v );
        }

        private void SafeUpdatePressedBackground()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "PressedBackground", HoverBackground ), ( v ) => _pressedBackground = v );
        }

        private void SafeUpdateLetterColor()
        {
            SafeSet<Color>( LayoutKeyMode.GetPropertyValue( Context.Config, "LetterColor", Colors.Black ), ( v ) => _letterColor = v );
        }

        private void SafeUpdateFontSize()
        {
            SafeSet<double>( LayoutKeyMode.GetPropertyValue<double>( Context.Config, "FontSize", 15 ), ( v ) => _fontSize = v );
        }

        private void SafeUpdateFontStyle()
        {
            SafeSet<FontStyle>( LayoutKeyMode.GetPropertyValue<FontStyle>( Context.Config, "FontStyle" ), ( v ) => _fontStyle = v );
        }

        private void SafeUpdateFontWeight()
        {
            SafeSet<FontWeight>( LayoutKeyMode.GetPropertyValue<FontWeight>( Context.Config, "FontWeight", FontWeights.Normal ), ( v ) => _fontWeight = v );
        }

        private void SafeUpdateShowLabel()
        {
            SafeSet<string>( Context.Config[_key.Current].GetOrSet<string>( "DisplayType", Context.Config[_key.Current]["Image"] != null ? "Image" : "Label" ), ( v ) => _showLabel = (v == "Label") );
        }

        private void SafeUpdateShowImage()
        {
            SafeSet<string>( Context.Config[_key.Current].GetOrSet<string>( "DisplayType", Context.Config[_key.Current]["Image"] != null ? "Image" : "Label" ), ( v ) => _showImage = (v == "Image") );
        }

        private void SafeUpdateOpacity()
        {
            SafeSet<double>( LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ), ( v ) => _opacity = v );
        }

        private void SafeUpdateTextDecorations()
        {
            MemoryStream stream = new MemoryStream();
            TextDecorationCollection obj = LayoutKeyMode.GetPropertyValue<TextDecorationCollection>( Context.Config, "TextDecorations" );
            if( obj != null ) System.Windows.Markup.XamlWriter.Save( obj, stream );
            stream.Seek( 0, SeekOrigin.Begin );
            SafeSet<Stream>( stream, ( v ) =>
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
            get { return _x; }
        }

        private int _y;
        /// <summary>
        /// Gets the Y coordinate of this key.
        /// </summary>
        public int Y
        {
            get { return _y; }
        }

        private int _width;
        /// <summary>
        /// Gets the width of this key.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        private int _height;
        /// <summary>
        /// Gets the height of this key.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        private bool _visible;
        /// <summary>
        /// Gets a value indicating whether this actual key is visible or not.
        /// </summary>
        public Visibility Visible
        {
            get { return _visible ? Visibility.Visible : Visibility.Collapsed; }
        }

        bool _isEnabled;
        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get { return _isEnabled; }
        }

        private string _upLabel;
        /// <summary>
        /// Gets the label that must be used when the key is up.
        /// </summary>
        public string UpLabel
        {
            get { return _upLabel; }
        }

        private string _downLabel;
        /// <summary>
        /// Gets the label that must be used when the key is down.
        /// </summary>
        public string DownLabel
        {
            get { return _downLabel; }
        }

        private string _description;
        /// <summary>
        /// Gets the description of the key
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        private int _index;
        /// <summary>
        /// Gets the logical position of the <see cref="IKey"/> in the zone.
        /// </summary>
        public int Index
        {
            get { return _index; }
            set
            {
                SafeSet<int>( value, ( v ) => _key.Index = v );
                OnPropertyChanged( "Index" );
            }
        }

        private bool _isFallback;
        /// <summary>
        /// Gets if the current keymode is a fallback or not.
        /// </summary>
        public bool IsFallback
        {
            get { return _isFallback; }

        }

        Image _image;
        public Image Image { get { return _image; } }

        Color _background;
        public Color Background
        {
            get { return _background; }
        }

        Color _hoverBackground;
        public Color HoverBackground
        {
            get { return _hoverBackground; }
        }

        Color _highlightBackground;
        public Color HighlightBackground
        {
            get { return _highlightBackground; }
        }

        Color _highlightFontColor;
        public Color HighlightFontColor
        {
            get { return _highlightFontColor; }
        }

        Color _pressedBackground;
        public Color PressedBackground
        {
            get { return _pressedBackground; }
        }

        Color _letterColor;
        public Color LetterColor
        {
            get { return _letterColor; }
        }

        FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
        }

        FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            get { return _fontWeight; }
        }

        double _fontSize;
        public double FontSize
        {
            get { return _fontSize; }
        }

        TextDecorationCollection _textDecorations;
        public TextDecorationCollection TextDecorations
        {
            get { return _textDecorations; }
        }

        bool _showLabel;
        public bool ShowLabel
        {
            get { return _showLabel; }
        }

        bool _showImage;
        public bool ShowImage
        {
            get { return _showImage; }
        }

        double _opacity;
        public double Opacity
        {
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
                Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
                if( value != _isHighlighting )
                {
                    SafeSet<bool>( value, ( v ) => _isHighlighting = v );
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

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighting = true;
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            IsHighlighting = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            scrollingDirective.NextActionType = ActionType.GoToRelativeRoot;

            //allows the repeat of the same key
            scrollingDirective.ActionTime = ActionTime.Delayed;

            if( KeyDownCommand.CanExecute( null ) )
            {
                KeyDownCommand.Execute( null );
                if( KeyUpCommand.CanExecute( null ) )
                {
                    KeyUpCommand.Execute( null );
                }
            }
            return scrollingDirective;
        }


        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }
    }
}
