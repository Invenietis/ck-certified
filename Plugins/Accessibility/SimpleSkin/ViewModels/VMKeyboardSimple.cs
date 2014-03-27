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
using System.Windows.Media;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using HighlightModel;
using CK.Core;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SimpleSkin.ViewModels
{
    public class VMKeyboardSimple : VMContextElement, IHighlightableElement
    {
        #region Properties & variables

        ObservableCollection<VMZoneSimple> _zones;
        ObservableCollection<VMKeySimple> _keys;
        IKeyboard _keyboard;

        public ObservableCollection<VMZoneSimple> Zones { get { return _zones; } private set { _zones = value; } }
        public IKeyboard Keyboard
        {
            get { return _keyboard; }
        }

        public ObservableCollection<VMKeySimple> Keys { get { return _keys; } }

        /// <summary>
        /// Gets the current layout used by the current keyboard.
        /// </summary>
        public ILayout Layout { get { return _keyboard.CurrentLayout; } }

        #endregion

        internal VMKeyboardSimple( VMContextSimpleBase ctx, IKeyboard kb )
            : base( ctx )
        {
            _zones = new ObservableCollection<VMZoneSimple>();
            _keys = new ObservableCollection<VMKeySimple>();

            _keyboard = kb;

            RegisterEvents();

            foreach( IZone zone in _keyboard.Zones.ToList() )
            {
                VMZoneSimple zoneVM = Context.Obtain( zone );
                Zones.Add( zoneVM );
                foreach( IKey key in zone.Keys )
                {
                    _keys.Add( Context.Obtain( key ) );
                }
            }


            SafeUpdateW();
            SafeUpdateH();
            SafeUpdateInsideBorderColor();
            SafeUpdateHighlightBackground();
            UpdateBackgroundPath();
        }

        internal override void Dispose()
        {
            Context.SkinDispatcher.Invoke( (Action)( () =>
            {
                _zones.Clear();
                _keys.Clear();
            } ), null );
            UnregisterEvents();
        }

        public void TriggerPropertyChanged()
        {
            OnPropertyChanged( "Keys" );
            OnPropertyChanged( "BackgroundImagePath" );
        }

        #region OnXXXXX

        void OnKeyCreated( object sender, KeyEventArgs e )
        {
            VMKeySimple kvm = Context.Obtain( e.Key );

            Context.SkinDispatcher.Invoke( (Action)( () =>
            {
                Context.Obtain( e.Key.Zone ).Keys.Add( kvm );
                _keys.Add( kvm );
            } ) );
        }

        void OnKeyDestroyed( object sender, KeyEventArgs e )
        {
            Context.SkinDispatcher.Invoke( (Action)( () =>
            {
                Context.Obtain( e.Key.Zone ).Keys.Remove( Context.Obtain( e.Key ) );
                _keys.Remove( Context.Obtain( e.Key ) );
            } ) );
            Context.OnModelDestroy( e.Key );
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            Context.SkinDispatcher.Invoke( (Action)( () =>
           {
               Zones.Add( Context.Obtain( e.Zone ) );
           } ) );
        }

        void OnZoneMoved( object sender, ZoneEventArgs e )
        {
            VMZoneSimple zoneVM = Zones.Where( z => z.Name == e.Zone.Name ).Single();

            ObservableCollection<VMZoneSimple> temp = new ObservableCollection<VMZoneSimple>();

            foreach( var item in Zones.OrderBy<VMZoneSimple, int>( z => z.Index ).ToList() )
            {
                temp.Add( item );
            }
            Zones.Clear();
            Zones = temp;

            OnPropertyChanged( "Zones" );
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {

            foreach( var k in e.Zone.Keys )
            {
                var mk = Context.Obtain( k );
                Context.SkinDispatcher.Invoke( (Action)( () =>
                {
                    Keys.Remove( mk );
                } ) );
                Context.OnModelDestroy( k );
            }
            Context.SkinDispatcher.Invoke( (Action)( () =>
            {
                Zones.Remove( Context.Obtain( e.Zone ) );
            } ) );
            Context.OnModelDestroy( e.Zone );

        }

        void OnLayoutSizeChanged( object sender, LayoutEventArgs e )
        {
            if( e.Layout == _keyboard.CurrentLayout )
            {
                SafeUpdateH();
                OnPropertyChanged( "H" );

                SafeUpdateW();
                OnPropertyChanged( "W" );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        UpdateBackgroundPath();
                        OnPropertyChanged( "BackgroundImagePath" );
                        break;
                    case "InsideBorderColor":
                        SafeUpdateInsideBorderColor();
                        OnPropertyChanged( "InsideBorderColor" );
                        break;
                    case "HighlightBackground":
                        SafeUpdateHighlightBackground();
                        OnPropertyChanged( "HighlightBackground" );
                        break;
                }
            }
        }

        private void RegisterEvents()
        {
            _keyboard.KeyCreated += OnKeyCreated;
            _keyboard.KeyDestroyed += OnKeyDestroyed;
            _keyboard.Zones.ZoneMoved += OnZoneMoved;
            _keyboard.Zones.ZoneCreated += OnZoneCreated;
            _keyboard.Zones.ZoneDestroyed += OnZoneDestroyed;
            _keyboard.Layouts.LayoutSizeChanged += OnLayoutSizeChanged;
            Context.Config.ConfigChanged += OnConfigChanged;
        }


        private void UnregisterEvents()
        {
            _keyboard.KeyCreated -= OnKeyCreated;
            _keyboard.KeyDestroyed -= OnKeyDestroyed;
            _keyboard.Zones.ZoneMoved -= OnZoneMoved;
            _keyboard.Zones.ZoneCreated -= OnZoneCreated;
            _keyboard.Zones.ZoneDestroyed -= OnZoneDestroyed;
            _keyboard.Layouts.LayoutSizeChanged -= OnLayoutSizeChanged;
            Context.Config.ConfigChanged -= OnConfigChanged;
        }

        #endregion

        #region "Design" properties


        private int _w;
        /// <summary>
        /// Gets the width of the current layout.
        /// </summary>
        public int W { get { return _w; } }


        private int _h;
        /// <summary>
        /// Gets the height of the current layout.
        /// </summary>
        public int H { get { return _h; } }

        private Brush _insideBorderColor;
        public Brush InsideBorderColor { get { return _insideBorderColor; } }

        ImageSourceConverter _imsc;
        ImageSourceConverter Imsc
        {
            get
            {
                if( _imsc == null ) _imsc = new ImageSourceConverter();
                return _imsc;
            }
        }

        object _backgroundImagePath;
        public object BackgroundImagePath { get { return _backgroundImagePath; } }

        #endregion

        #region IHighlightableElement Members

        private bool _isHighlighting;
        public bool IsHighlighting
        {
            get { return _isHighlighting; }
            set
            {
                if( value != _isHighlighting )
                {
                    ThreadSafeSet<bool>( value, ( v ) => _isHighlighting = v );
                    OnPropertyChanged( "IsHighlighting" );
                    //foreach( var key in Keys )
                    //{
                    //    key.IsHighlighting = value;
                    //}
                }
            }
        }

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get
            {
                if( Zones.Count > 0 )
                {
                    return new CKReadOnlyListOnIList<IHighlightableElement>( Zones.Cast<IHighlightableElement>().ToList() );
                }
                return CKReadOnlyListEmpty<IHighlightableElement>.Empty;
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

        Color _highlightBackground;
        public Color HighlightBackground
        {
            get { return _highlightBackground; }
        }

        private void SafeUpdateHighlightBackground()
        {
            Color c = Context.Config[Layout].GetOrSet<Color>( "HighlightBackground", (Color)ColorConverter.ConvertFromString( "#FFA2BDF2" ) );
            ThreadSafeSet<Color>( c, ( v ) =>
            {
                if( v == null ) _highlightBackground = (Color)ColorConverter.ConvertFromString( "#FFA2BDF2" );
                else _highlightBackground = v;
            } );
        }

        public SkippingBehavior Skip
        {
            get
            {
                //TODO : Improve (temporary)
                if( Keyboard.Name == "Prediction" ) return SkippingBehavior.EnterChildren;

                if( Zones.Count == 0 || Zones.All( z => z.Skip == SkippingBehavior.Skip ) )
                    return SkippingBehavior.Skip; //If there are no zones or that they are all to be skipped, we skip this root element
                return SkippingBehavior.None;
            }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {

            if( beginScrollingInfo.PreviousElement != this )
                IsHighlighting = true;
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( endScrollingInfo.ElementToBeHighlighted != this )
                IsHighlighting = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            scrollingDirective.NextActionType = ActionType.EnterChild;
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }

        #endregion

        #region Threadsafe updates

        private void UpdateBackgroundPath()
        {
            object keyboardBackgroundObject = Context.Config[Layout]["KeyboardBackground"];

            //Some contexts have a color in the KeyboardBackground.
            if( keyboardBackgroundObject == null || keyboardBackgroundObject is Color )
            {
                keyboardBackgroundObject = "pack://application:,,,/SimpleSkin;component/Images/skinBackground.png";
                Context.Config[Layout].Set( "KeyboardBackground", keyboardBackgroundObject );
            }

            ThreadSafeSet<string>( keyboardBackgroundObject.ToString(), ( v ) =>
            {
                if( String.IsNullOrWhiteSpace( v ) ) _backgroundImagePath = null;
                else _backgroundImagePath = Imsc.ConvertFromString( v );
            } );
        }

        private void SafeUpdateInsideBorderColor()
        {
            Color c = Context.Config[Layout].GetOrSet<Color>( "InsideBorderColor", null );
            ThreadSafeSet<Color>( c, ( v ) =>
            {
                if( v == null ) _insideBorderColor = null;
                else _insideBorderColor = new SolidColorBrush( v );
            } );
        }

        private void SafeUpdateH()
        {
            ThreadSafeSet<int>( _keyboard.CurrentLayout.H, ( v ) => _h = v );
        }

        private void SafeUpdateW()
        {
            ThreadSafeSet<int>( _keyboard.CurrentLayout.W, ( v ) => _w = v );
        }

        #endregion

    }
}
