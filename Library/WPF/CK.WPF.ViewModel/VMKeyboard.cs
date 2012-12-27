#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMKeyboard.cs) is part of CiviKey. 
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

using System.Collections.ObjectModel;
using System.Drawing;
using System;
using CK.Keyboard.Model;

namespace CK.WPF.ViewModel
{
    public abstract class VMKeyboard<TC, TB, TZ, TK, TKM, TLKM> : VMContextElement<TC, TB, TZ, TK, TKM, TLKM>
        where TC : VMContext<TC, TB, TZ, TK, TKM, TLKM>
        where TB : VMKeyboard<TC, TB, TZ, TK, TKM, TLKM>
        where TZ : VMZone<TC, TB, TZ, TK, TKM, TLKM>
        where TK : VMKey<TC, TB, TZ, TK, TKM, TLKM>
        where TKM : VMKeyMode<TC, TB, TZ, TK, TKM, TLKM>
        where TLKM : VMLayoutKeyMode<TC, TB, TZ, TK, TKM, TLKM>
    {
        IKeyboard _keyboard;
        ObservableCollection<TZ> _zones;
        ObservableCollection<TK> _keys;

        /// <summary>
        /// Gets the current layout used by the current keyboard.
        /// </summary>
        public ILayout Layout { get { return _keyboard.CurrentLayout; } }

        /// <summary>
        /// Gets or sets the width of the current layout.
        /// </summary>
        public int W
        {
            get { return _keyboard.CurrentLayout.W; }
            set { _keyboard.CurrentLayout.W = value; }
        }

        /// <summary>
        /// Gets or sets the height of the current layout.
        /// </summary>
        public int H
        {
            get { return _keyboard.CurrentLayout.H; }
            set { _keyboard.CurrentLayout.H = value; }
        }

        /// <summary>
        /// Gets the available modes, concatenated as one, seperated by "+" characters.
        /// </summary>
        public IKeyboardMode Modes { get { return _keyboard.AvailableMode; } }

        /// <summary>
        /// Gets the viewmodels for each <see cref="IZone"/> of the linked <see cref="IKeyboard"/>
        /// </summary>
        public ObservableCollection<TZ> Zones { get { return _zones; } }

        /// <summary>
        /// Gets the viewmodels for each  <see cref="IKey"/> of the linked <see cref="IKeyboard"/>
        /// </summary>
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
                var vmz = Context.Obtain( zone );
                // TODO: find a better way....
                if( zone.Name == "Prediction" ) Zones.Insert( 0, vmz );
                else Zones.Add( vmz );

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
            OnTriggerKeyCreated();
        }

        protected virtual void OnTriggerKeyCreated()
        {
        }

        void OnKeyMoved( object sender, KeyMovedEventArgs e )
        {
            Context.Obtain( e.Key ).PositionChanged();
            OnTriggerKeyMoved();
        }

        protected virtual void OnTriggerKeyMoved()
        {
        }

        void OnKeyDestroyed( object sender, KeyEventArgs e )
        {
            Context.Obtain( e.Key.Zone ).Keys.Remove( Context.Obtain( e.Key ) );
            _keys.Remove( Context.Obtain( e.Key ) );
            Context.OnModelDestroy( e.Key );
            OnTriggerKeyDestroyed();
        }


        protected virtual void OnTriggerKeyDestroyed()
        {
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            //TODO
            var vmz = Context.Obtain( e.Zone );
            if( e.Zone.Name == "Prediction" ) Zones.Insert( 0, vmz );
            else Zones.Add( vmz);
            OnTriggerZoneCreated();
        }


        protected virtual void OnTriggerZoneCreated()
        {
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            var zone = Context.Obtain( e.Zone );
            if( zone != null )
            {
                foreach( var k in e.Zone.Keys )
                {
                    var mk = Context.Obtain( k );
                    Keys.Remove( mk );
                    Context.OnModelDestroy( k );
                }

                Zones.Remove( zone );
                Context.OnModelDestroy( e.Zone );
                OnTriggerZoneDestroyed();
            }
        }

        protected virtual void OnTriggerZoneDestroyed()
        {
        }

        void OnLayoutSizeChanged( object sender, LayoutEventArgs e )
        {
            if( e.Layout == _keyboard.CurrentLayout )
            {
                OnPropertyChanged( "W" );
                OnPropertyChanged( "H" );
                OnTriggerLayoutSizeChanged();
            }
        }

        protected virtual void OnTriggerLayoutSizeChanged()
        {
        }

        #endregion
    }
}
