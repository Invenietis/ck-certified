using System.Collections.ObjectModel;
using System.Drawing;
using System;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    public abstract class VMKeyboard<TC, TB, TZ, TK> : VMContextElement<TC, TB, TZ, TK>
        where TC : VMContext<TC, TB, TZ, TK>
        where TB : VMKeyboard<TC, TB, TZ, TK>
        where TZ : VMZone<TC, TB, TZ, TK>
        where TK : VMKey<TC, TB, TZ, TK>
    {
        IKeyboard _keyboard;
        ObservableCollection<TZ> _zones;
        ObservableCollection<TK> _keys;

        /// <summary>
        /// Gets the current layout used by the current keyboard.
        /// </summary>
        public ILayout Layout { get { return _keyboard.CurrentLayout; } }

        /// <summary>
        /// Gets the width of the current layout.
        /// </summary>
        public int W 
        { 
            get 
            { 
                return _keyboard.CurrentLayout.W; 
            } 
        }

        /// <summary>
        /// Gets the height of the current layout.
        /// </summary>
        public int H { 
            get 
            { 
                return _keyboard.CurrentLayout.H; 
            } 
        }
        
        public ObservableCollection<TZ> Zones { get { return _zones; } }
        public ObservableCollection<TK> Keys { get { return _keys; } }

        public VMKeyboard( TC context, IKeyboard keyboard )
            : base( context )
        {
            _zones = new ObservableCollection<TZ>();
            _keys = new ObservableCollection<TK>();

            _keyboard = keyboard;

            _keyboard.KeyCreated += new EventHandler<KeyEventArgs>( OnKeyCreated );
            _keyboard.KeyMoved += new EventHandler<KeyMovedEventArgs>( OnKeyMoved );
            _keyboard.KeyDestroyed += new EventHandler<KeyEventArgs>( OnKeyDestroyed );            
            _keyboard.Zones.ZoneCreated += new EventHandler<ZoneEventArgs>( OnZoneCreated );
            _keyboard.Zones.ZoneDestroyed += new EventHandler<ZoneEventArgs>( OnZoneDestroyed );
            _keyboard.Layouts.LayoutSizeChanged += new EventHandler<LayoutEventArgs>( OnLayoutSizeChanged );

            foreach( IZone zone in _keyboard.Zones )
            {
                Zones.Add( Context.Obtain( zone ) );
                foreach( IKey key in zone.Keys )
                {
                    _keys.Add( Context.Obtain( key ) );
                }
            }
        }

        protected override void OnDispose()
        {
            foreach( TZ zone in Zones )
            {
                zone.Dispose();
            }
            _zones.Clear();
            _keys.Clear();

            _keyboard.KeyCreated -= new EventHandler<KeyEventArgs>( OnKeyCreated );
            _keyboard.KeyMoved -= new EventHandler<KeyMovedEventArgs>( OnKeyMoved );
            _keyboard.KeyDestroyed -= new EventHandler<KeyEventArgs>( OnKeyDestroyed );
            _keyboard.Zones.ZoneCreated -= new EventHandler<ZoneEventArgs>( OnZoneCreated );
            _keyboard.Zones.ZoneDestroyed -= new EventHandler<ZoneEventArgs>( OnZoneDestroyed );
            _keyboard.Layouts.LayoutSizeChanged -= new EventHandler<LayoutEventArgs>( OnLayoutSizeChanged );
        }

        public void TriggerPropertyChanged()
        {
            OnTriggerPropertyChanged();
            OnPropertyChanged( "Keys" );
            OnPropertyChanged( "BackgroundImagePath" );
        }

        protected virtual void OnTriggerPropertyChanged()
        {
        }

        #region OnXXXXX
        void OnKeyCreated( object sender, KeyEventArgs e )
        {
            TK kvm = Context.Obtain( e.Key );
            Context.Obtain( e.Key.Zone ).Keys.Add( kvm );
            _keys.Add( kvm );
        }

        void OnKeyMoved( object sender, KeyMovedEventArgs e )
        {
            Context.Obtain( e.Key ).PositionChanged();
        }

        void OnKeyDestroyed( object sender, KeyEventArgs e )
        {
            Context.Obtain( e.Key.Zone ).Keys.Remove( Context.Obtain( e.Key ) );
            _keys.Remove( Context.Obtain( e.Key ) );
            Context.OnModelDestroy( e.Key );
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            Zones.Add( Context.Obtain( e.Zone ) );
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            Zones.Remove( Context.Obtain( e.Zone ) );
            Context.OnModelDestroy( e.Zone );
        }

        void OnLayoutSizeChanged( object sender, LayoutEventArgs e )
        {
            if( e.Layout == _keyboard.CurrentLayout )
            {
                OnPropertyChanged( "W" );
                OnPropertyChanged( "H" );
            }
        }
        #endregion
    }
}
