using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    public abstract class VMKey<TC, TB, TZ, TK> : VMContextElement<TC, TB, TZ, TK>
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        IKey _key;
        ICommand _keyDownCmd;
        ICommand _keyUpCmd;
        ICommand _keyPressedCmd;
        Dictionary<string,ActionSequence> _actionsOnPropertiesChanged;

        #region Properties

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
        /// Gets if the current keymode is a fallback or not.
        /// </summary>
        public bool IsFallback { get { return _key.Current.IsFallBack; } }

        public ICommand KeyDownCommand { get { return _keyDownCmd; } }

        public ICommand KeyUpCommand { get { return _keyUpCmd; } }

        public ICommand KeyPressedCommand { get { return _keyPressedCmd; } }

        #endregion

        public VMKey( TC context, IKey key )
            : base( context )
        {
            _key = key;

            ResetCommands();

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
            SetActionOnPropertyChanged( "Width", () => OnPropertyChanged( "Width" ) );
            SetActionOnPropertyChanged( "Height", () => OnPropertyChanged( "Height" ) );
            SetActionOnPropertyChanged( "Visible", () => OnPropertyChanged( "Visible" ) );
            SetActionOnPropertyChanged( "Enabled", () => OnPropertyChanged( "Enabled" ) );
            SetActionOnPropertyChanged( "UpLabel", () => OnPropertyChanged( "UpLabel" ) );
            SetActionOnPropertyChanged( "DownLabel", () => OnPropertyChanged( "DownLabel" ) );

            _key.KeyPropertyChanged += new EventHandler<KeyPropertyChangedEventArgs>( OnKeyPropertyChanged );
            _key.Keyboard.CurrentModeChanged += new EventHandler<KeyboardModeChangedEventArgs>( OnModeChanged );
        }

        void OnModeChanged( object sender, KeyboardModeChangedEventArgs e )
        {
            OnPropertyChanged( "IsFallback" );
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

        void ResetCommands()
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