#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMKey.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    [Flags]
    public enum FallbackVisibility
    {
        /// <summary>
        /// Enables the Fallback on the nearest <see cref="ILayoutKeyMode"/>
        /// </summary>
        FallbackOnLayout = 1,

        /// <summary>
        /// Enables the Fallback on the nearest <see cref="IKeyMode"/>
        /// </summary>
        FallbackOnKeyMode = 2
    }

    public abstract class VMKey<TC, TB, TZ, TK> : VMContextElement<TC, TB, TZ, TK>
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        Dictionary<string, ActionSequence> _actionsOnPropertiesChanged;
        ICommand _keyPressedCmd;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        IKey _key;

        #region Properties

        /// <summary>
        /// Gets the model linked to this ViewModel
        /// </summary>
        public IKey Model { get { return _key; } }

        FallbackVisibility _showFallback;
        public FallbackVisibility ShowFallback
        {
            get { return _showFallback; }
            set
            {
                _showFallback = value;
                OnPropertyChanged( "ShowFallback" );
            }
        }

        /// <summary>
        /// If there is no <see cref="IKeyMode"/> for the underlying <see cref="IKey"/> on the current <see cref="IKeyboardMode"/>, gets whether propeties of the nearest <see cref="IKeyMode"/> should be displayed.
        /// </summary>
        public bool ShowKeyModeFallback { get { return ( ShowFallback & FallbackVisibility.FallbackOnKeyMode ) == FallbackVisibility.FallbackOnKeyMode; } }

        /// <summary>
        /// If there is no <see cref="ILayoutKeyMode"/> for the underlying <see cref="IKey"/> on the current <see cref="IKeyboardMode"/>, gets whether propeties of the nearest <see cref="ILayoutKeyMode"/> should be displayed.
        /// </summary>
        public bool ShowLayoutFallback { get { return ( ShowFallback & FallbackVisibility.FallbackOnLayout ) == FallbackVisibility.FallbackOnLayout; } }

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

        #region Layout Properties


        /// <summary>
        /// Gets the X coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int X
        {
            get { return _key.CurrentLayout.Current.X; }
            set
            {
                //if( _key.CurrentLayout.Current.IsFallBack )
                //{
                //    _key.KeyModes.Create( _key.Keyboard.CurrentMode );
                //    _key.CurrentLayout.LayoutKeyModes.Create( _key.Keyboard.CurrentMode );
                //}
                _key.CurrentLayout.Current.X = value;
                OnPropertyChanged( "X" );
            }
        }

        /// <summary>
        /// Gets the Y coordinate of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Y
        {
            get { return _key.CurrentLayout.Current.Y; }
            set
            {
                _key.CurrentLayout.Current.Y = value;
                OnPropertyChanged( "Y" );
            }
        }

        /// <summary>
        /// Gets or sets the width of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Width
        {
            get { return _key.CurrentLayout.Current.Width; }
            set 
            { 
                _key.CurrentLayout.Current.Width = value;
                OnPropertyChanged( "Width" );
            }
        }

        /// <summary>
        /// Gets or sets the height of this key, for the current <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public int Height
        {
            get { return _key.CurrentLayout.Current.Height; }
            set
            {
                _key.CurrentLayout.Current.Height = value;
                OnPropertyChanged( "Height" );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this actual key is visible or not, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public Visibility Visible
        {
            get { return LayoutKeyMode.Visible ? Visibility.Visible : Visibility.Collapsed; }
            set
            {
                LayoutKeyMode.Visible = ( value == Visibility.Visible );
                OnPropertyChanged( "Visible" );
            }
        }

        #endregion

        #region KeyMode Properties

        /// <summary>
        /// Gets a value indicating wether the current keymode is enabled or not.
        /// </summary>
        public bool Enabled { get { return _key.Current.Enabled; } }

        /// <summary>
        /// Gets or sets the label that must be used when the key is up, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public string UpLabel
        {
            get { return _key.Current.UpLabel; }
            set
            {
                _key.Current.UpLabel = value;
                OnPropertyChanged( "UpLabel" );
            }
        }

        /// <summary>
        /// Makes sure there is a KeyMode on this key for the current mode.
        /// </summary>
        private void EnsureKeyMode()
        {
            //If the user is modifying a property linked to the KeyMode and that there is no KeyMode for this mode; we create one.
            if( _key.Current.IsFallBack )
            {
                _key.KeyModes.Create( _key.Keyboard.CurrentMode );
            }
        }

        /// <summary>
        /// Gets or sets the label that must be used when the key is down, for the current <see cref="IKeyMode"/>.
        /// </summary>
        public string DownLabel
        {
            get { return _key.Current.DownLabel; }
            set
            {
                _key.Current.DownLabel = value;
                OnPropertyChanged( "DownLabel" );
            }
        }

        /// <summary>
        /// Gets or sets the description of the current <see cref="IKeyMode"/> of this Key
        /// </summary>
        public string Description
        {
            get { return _key.Current.Description; }
            set
            {
                _key.Current.Description = value;
                OnPropertyChanged( "Description" );
            }
        }

        #endregion

        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="IKeyMode"/>
        /// </summary>
        private IKeyboardMode CurrentKeyModeMode { get { return _key.Current.Mode; } }

        /// <summary>
        /// Gets the current <see cref="IKeyboardMode"/> of the underlying <see cref="ILayoutKeyMode"/>
        /// </summary>
        private IKeyboardMode CurrentLayoutKeyModeMode { get { return _key.CurrentLayout.Current.Mode; } }

        /// <summary>
        /// Gets if the current <see cref="IKeyMode"/> is a fallback or not.
        /// </summary>
        public bool IsKeyModeFallback { get { return _key.Current.IsFallBack; } }

        /// <summary>
        /// Gets if the current <see cref="ILayoutKeyMode"/> is a fallback or not.
        /// </summary>
        public bool IsLayoutKeyModeFallback { get { return _key.CurrentLayout.Current.IsFallBack; } }

        /// <summary>
        /// Gets the command called when the user releases the left click on the key
        /// </summary>
        public ICommand KeyUpCommand { get { return _keyUpCmd; } set { _keyUpCmd = value; } }

        /// <summary>
        /// Gets the command called when the user pushes the left click on the key
        /// </summary>
        public ICommand KeyDownCommand { get { return _keyDownCmd; } set { _keyDownCmd = value; } }

        /// <summary>
        /// Gets the command called when the user (lef-click) presses the key
        /// </summary>
        public ICommand KeyPressedCommand { get { return _keyPressedCmd; } set { _keyPressedCmd = value; } }

        #endregion

        /// <summary>
        /// Ctor for VMKey
        /// </summary>
        /// <param name="context">The VMContext</param>
        /// <param name="key">The underlying model</param>
        public VMKey( TC context, IKey key )
            : this( context, key, true )
        {
        }

        /// <summary>
        /// Advanced Ctor for a VMKey. Enables setting <see cref="KeyUpCommand"/> <see cref="KeyDownCommand"/> and <see cref="KeyPressedCommand"/>
        /// </summary>
        /// <param name="context">The VMContext</param>
        /// <param name="key">The underlying model</param>
        /// <param name="presetPushBehavior">Set to true if you want <see cref="KeyUpCommand"/> <see cref="KeyDownCommand"/> and <see cref="KeyPressedCommand"/> to transfer the pushes to the underlying <see cref="IKey"/> (classic behavior for a VMKey used as a keyboard key) </param>
        public VMKey( TC context, IKey key, bool presetPushBehavior )
            : base( context )
        {
            //By default, we show the fallback.
            ShowFallback = FallbackVisibility.FallbackOnLayout | FallbackVisibility.FallbackOnKeyMode;

            _key = key;

            if( presetPushBehavior )
                SetCommands();

            _actionsOnPropertiesChanged = new Dictionary<string, ActionSequence>();

            SetActionOnPropertyChanged( "Current", () =>
            {
                OnPropertyChanged( "UpLabel" );
                OnPropertyChanged( "DownLabel" );
                OnPropertyChanged( "Enabled" );
            } );

            SetActionOnPropertyChanged( "CurrentLayout", () =>
            {
                OnPropertyChanged( "X" );
                OnPropertyChanged( "Y" );
                OnPropertyChanged( "Width" );
                OnPropertyChanged( "Height" );
                OnPropertyChanged( "Visible" );
            } );

            SetActionOnPropertyChanged( "X", () => OnPropertyChanged( "X" ) );
            SetActionOnPropertyChanged( "Y", () => OnPropertyChanged( "Y" ) );
            SetActionOnPropertyChanged( "W", () => OnPropertyChanged( "Width" ) );
            SetActionOnPropertyChanged( "H", () => OnPropertyChanged( "Height" ) );
            SetActionOnPropertyChanged( "Width", () => OnPropertyChanged( "Width" ) );
            SetActionOnPropertyChanged( "Height", () => OnPropertyChanged( "Height" ) );
            SetActionOnPropertyChanged( "Visible", () => OnPropertyChanged( "Visible" ) );
            SetActionOnPropertyChanged( "Enabled", () => OnPropertyChanged( "Enabled" ) );
            SetActionOnPropertyChanged( "UpLabel", () => OnPropertyChanged( "UpLabel" ) );
            SetActionOnPropertyChanged( "DownLabel", () => OnPropertyChanged( "DownLabel" ) );
            SetActionOnPropertyChanged( "Description", () => OnPropertyChanged( "Description" ) );

            _key.KeyPropertyChanged += new EventHandler<KeyPropertyChangedEventArgs>( OnKeyPropertyChanged );
            _key.Keyboard.CurrentModeChanged += new EventHandler<KeyboardModeChangedEventArgs>( OnModeChanged );
        }

        protected virtual void OnTriggerModeChanged()
        {
        }

        void OnModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            OnTriggerModeChanged();
            OnPropertyChanged( "IsKeyModeFallback" );
            OnPropertyChanged( "IsLayoutKeyModeFallback" );
        }

        protected void SetActionOnPropertyChanged( string propertyName, Action action )
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

        internal void PositionChanged()
        {
            OnPropertyChanged( "X" );
            OnPropertyChanged( "Y" );
        }

        public void OnKeyPropertyChanged( object sender, KeyPropertyChangedEventArgs e )
        {
            if( _actionsOnPropertiesChanged.ContainsKey( e.PropertyName ) )
                _actionsOnPropertiesChanged[e.PropertyName].Run();
        }

        void SetCommands()
        {
            _keyDownCmd = new KeyCommand( () => { if( !_key.IsDown )_key.Push(); } );
            _keyUpCmd = new KeyCommand( () => { if( _key.IsDown ) _key.Release(); } );
            _keyPressedCmd = new KeyCommand( () => { if( _key.IsDown )_key.Release( true ); } );
        }

        protected override void OnDispose()
        {
            _key.KeyPropertyChanged -= new EventHandler<KeyPropertyChangedEventArgs>( OnKeyPropertyChanged );
            _key.Keyboard.CurrentModeChanged -= new EventHandler<KeyboardModeChangedEventArgs>( OnModeChanged );
        }
    }

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