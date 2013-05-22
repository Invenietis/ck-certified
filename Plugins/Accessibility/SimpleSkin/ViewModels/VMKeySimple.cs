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

namespace SimpleSkin.ViewModels
{
    public class VMKeySimple : VMContextElement, IHighlightableElement
    {
        Dictionary<string, ActionSequence> _actionsOnPropertiesChanged;
        ICommand _keyPressedCmd;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        IKey _key;

        public VMKeySimple( VMContextSimple ctx, IKey k )
            : base( ctx )
        {
            _actionsOnPropertiesChanged = new Dictionary<string, ActionSequence>();
            _key = k;

            ResetCommands();
            RegisterEvents();
        }

        #region OnXXX

        public void OnKeyPropertyChanged( object sender, KeyPropertyChangedEventArgs e )
        {
            if( _actionsOnPropertiesChanged.ContainsKey( e.PropertyName ) )
                _actionsOnPropertiesChanged[e.PropertyName].Run();
        }

        protected override void OnDispose()
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
                PropertyChangedTriggers();
            }
        }

        private void RegisterEvents()
        {
            SetActionOnPropertyChanged( "Current", () =>
            {
                OnPropertyChanged( "UpLabel" );
                OnPropertyChanged( "DownLabel" );
                OnPropertyChanged( "Enabled" );
            } );

            SetActionOnPropertyChanged( "X", () => OnPropertyChanged( "X" ) );
            SetActionOnPropertyChanged( "Y", () => OnPropertyChanged( "Y" ) );
            SetActionOnPropertyChanged( "W", () => OnPropertyChanged( "Width" ) );
            SetActionOnPropertyChanged( "H", () => OnPropertyChanged( "Height" ) );
            SetActionOnPropertyChanged( "Image", () => OnPropertyChanged( "Image" ) );
            SetActionOnPropertyChanged( "Visible", () => OnPropertyChanged( "Visible" ) );
            SetActionOnPropertyChanged( "Enabled", () => OnPropertyChanged( "Enabled" ) );
            SetActionOnPropertyChanged( "UpLabel", () => OnPropertyChanged( "UpLabel" ) );
            SetActionOnPropertyChanged( "DownLabel", () => OnPropertyChanged( "DownLabel" ) );
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

        private void PropertyChangedTriggers()
        {
            OnPropertyChanged( "X" );
            OnPropertyChanged( "Y" );
            OnPropertyChanged( "Width" );
            OnPropertyChanged( "Image" );
            OnPropertyChanged( "Height" );
            OnPropertyChanged( "Opacity" );
            OnPropertyChanged( "Visible" );
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

        #endregion

        #region "Design" properties

        /// <summary>
        /// Gets the current actualKey layout.
        /// </summary>
        public ILayoutKeyMode LayoutKeyMode
        {
            get { return _key.CurrentLayout.Current; }
        }

        public ILayoutKey LayoutKey
        {
            get { return _key.CurrentLayout; }
        }

        /// <summary>
        /// Gets the X coordinate of this key.
        /// </summary>
        public int X
        {
            get { return _key.CurrentLayout.Current.X; }
        }

        /// <summary>
        /// Gets the Y coordinate of this key.
        /// </summary>
        public int Y
        {
            get { return _key.CurrentLayout.Current.Y; }
        }

        /// <summary>
        /// Gets the width of this key.
        /// </summary>
        public int Width
        {
            get { return _key.CurrentLayout.Current.Width; }
        }

        /// <summary>
        /// Gets the height of this key.
        /// </summary>
        public int Height
        {
            get { return _key.CurrentLayout.Current.Height; ; }
        }

        /// <summary>
        /// Gets a value indicating whether this actual key is visible or not.
        /// </summary>
        public Visibility Visible
        {
            get { return LayoutKeyMode.Visible ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return _key.Current.Enabled; } }

        /// <summary>
        /// Gets the label that must be used when the key is up.
        /// </summary>
        public string UpLabel
        {
            get { return _key.Current.UpLabel; }
        }

        /// <summary>
        /// Gets the label that must be used when the key is down.
        /// </summary>
        public string DownLabel
        {
            get { return _key.Current.DownLabel; }
        }

        /// <summary>
        /// Gets the description of the key
        /// </summary>
        public string Description
        {
            get { return _key.Current.Description; }
        }

        /// <summary>
        /// Gets the logical position of the <see cref="IKey"/> in the zone.
        /// </summary>
        public int Index
        {
            get { return _key.Index; }
            set
            {
                _key.Index = value;
                OnPropertyChanged( "Index" );
            }
        }

        /// <summary>
        /// Gets if the current keymode is a fallback or not.
        /// </summary>
        public bool IsFallback { get { return _key.Current.IsFallBack; } }

        public Image Image
        {
            get
            {
                object imageData = Context.Config[LayoutKeyMode]["Image"];

                if( imageData != null )
                {
                    return WPFImageProcessingHelper.ProcessImage( imageData );
                }

                return null;
            }
            //get { return LayoutKeyMode.GetPropertyValue<Image>( Context.Config, "Image" ); }
        }

        public Color Background
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "Background", Colors.White ); }
        }

        public Color HoverBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HoverBackground", Background ); }
        }

        public Color HighlightBackground
        {
            get { return LayoutKeyMode.GetPropertyValue( Context.Config, "HighlightBackground", Background ); }
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

        public bool ShowImage
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowImage", true ); }
        }

        public double Opacity
        {
            get { return LayoutKeyMode.GetPropertyValue<double>( Context.Config, "Opacity", 1.0 ); }
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
                    _isHighlighting = value;
                    OnPropertyChanged( "IsHighlighting" );
                }
            }
        }

        public IReadOnlyList<IHighlightableElement> Children
        {
            get { return ReadOnlyListEmpty<IHighlightableElement>.Empty; }
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
