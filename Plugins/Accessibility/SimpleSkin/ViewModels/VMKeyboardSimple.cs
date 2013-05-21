#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMKeyboardSimple.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CK.Windows.Helpers;
using HighlightModel;
using CK.Core;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleSkin.ViewModels
{
    internal class VMKeyboardSimple : VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>, IHighlightableElement
    {
        public VMKeyboardSimple( VMContextSimple ctx, IKeyboard kb )
            : base( ctx )
        {
            Context.Config.ConfigChanged += new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            _zones = new ObservableCollection<VMZoneSimple>();
            _keys = new ObservableCollection<VMKeySimple>();

            _keyboard = kb;

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




        IKeyboard _keyboard;
        ObservableCollection<VMZoneSimple> _zones;
        ObservableCollection<VMKeySimple> _keys;

        /// <summary>
        /// Gets the current layout used by the current keyboard.
        /// </summary>
        public ILayout Layout { get { return _keyboard.CurrentLayout; } }

        /// <summary>
        /// Gets the width of the current layout.
        /// </summary>
        public int W { get { return _keyboard.CurrentLayout.W; } }

        /// <summary>
        /// Gets the height of the current layout.
        /// </summary>
        public int H { get { return _keyboard.CurrentLayout.H; } }

        public ObservableCollection<VMZoneSimple> Zones { get { return _zones; } }
        public ObservableCollection<VMKeySimple> Keys { get { return _keys; } }

        protected override void OnDispose()
        {
            Context.Config.ConfigChanged -= new EventHandler<CK.Plugin.Config.ConfigChangedEventArgs>( OnConfigChanged );

            foreach( VMZoneSimple zone in Zones )
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
            VMKeySimple kvm = Context.Obtain( e.Key );
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

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        OnPropertyChanged( "BackgroundImagePath" );
                        break;
                    case "InsideBorderColor":
                        OnPropertyChanged( "InsideBorderColor" );
                        break;
                }
            }
        }

        public Brush InsideBorderColor
        {
            get
            {
                if( Context.Config[Layout]["InsideBorderColor"] != null )
                    return new SolidColorBrush( (Color)Context.Config[Layout]["InsideBorderColor"] );
                return null;
            }
        }

        ImageSourceConverter imsc;
        public object BackgroundImagePath
        {
            get
            {
                if( imsc == null ) imsc = new ImageSourceConverter();
                return imsc.ConvertFromString( Context.Config[Layout].GetOrSet( "KeyboardBackground", "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png" ) );
            }
        }

        #region IHighlightableElement Members

        public IReadOnlyList<IHighlightableElement> Children
        {
            get
            {
                if( Zones.Count > 0 )
                {
                    return new ReadOnlyListOnIList<IHighlightableElement>( Zones.Cast<IHighlightableElement>().ToList() );
                }
                return new ReadOnlyListOnIList<IHighlightableElement>( new List<IHighlightableElement>() );
            }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return W; }
        }

        public int Height
        {
            get { return H; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.EnterChildren; }
        }

        #endregion
    }
}
