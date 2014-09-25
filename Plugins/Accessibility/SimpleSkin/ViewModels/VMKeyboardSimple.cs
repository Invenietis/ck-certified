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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using HighlightModel;

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
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

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

            UpdateW();
            UpdateH();
            SafeUpdateKeyboardOpacity();
            SafeUpdateKeyboardBackgroundColor();
            SafeUpdateKeyboardBorderBrush();
            SafeUpdateKeyboardBorderThickness();
            UpdateHighlightBackground();
            UpdateHighlightFontColor();
            UpdateLoopCount();
            UpdateBackgroundPath();

            IsHighlightableTreeRoot = true;
        }

        internal override void Dispose()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            UnregisterEvents();

            foreach( var zone in _zones )
            {
                zone.Dispose();
            }

            Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                _zones.Clear();
                _keys.Clear();
            }), null );
        }

        public void TriggerPropertyChanged()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
            {
                OnPropertyChanged( "Keys" );
                OnPropertyChanged( "BackgroundImagePath" );
            }) );
        }

        #region OnXXXXX

        void OnKeyCreated( object sender, KeyEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            VMKeySimple kvm = Context.Obtain( e.Key );
            VMZoneSimple zvm = Context.Obtain( e.Key.Zone );

            Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
            {
                zvm.Keys.Add( kvm );
                _keys.Add( kvm );
            }) );
        }

        void OnKeyDestroyed( object sender, KeyEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                Context.Obtain( e.Key.Zone ).Keys.Remove( Context.Obtain( e.Key ) );
                _keys.Remove( Context.Obtain( e.Key ) );
            }) );
            Context.OnModelDestroy( e.Key );
        }

        void OnZoneCreated( object sender, ZoneEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            var zvm =  Context.Obtain( e.Zone );

            Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
           {
               Zones.Add( zvm );
           }) );
        }

        void OnZoneMoved( object sender, ZoneEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            VMZoneSimple zoneVM = Zones.Where( z => z.Name == e.Zone.Name ).Single();

            ObservableCollection<VMZoneSimple> temp = new ObservableCollection<VMZoneSimple>();
            foreach( var item in Zones.OrderBy<VMZoneSimple, int>( z => z.Index ).ToList() )
            {
                temp.Add( item );
            }
            Zones.Clear();

            Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
           {
               Zones = temp;
               OnPropertyChanged( "Zones" );
           }) );
        }

        void OnZoneDestroyed( object sender, ZoneEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            foreach( var k in e.Zone.Keys )
            {
                var mk = Context.Obtain( k );
                Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
                {
                    Keys.Remove( mk );
                }) );
                Context.OnModelDestroy( k );
            }

            Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                Zones.Remove( Context.Obtain( e.Zone ) );
            }) );

            Context.OnModelDestroy( e.Zone );
        }

        void OnLayoutSizeChanged( object sender, LayoutEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Layout == _keyboard.CurrentLayout )
            {
                UpdateH();
                UpdateW();

                Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
                {
                    OnPropertyChanged( "H" );
                    OnPropertyChanged( "W" );
                }) );
            }
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Obj == Layout )
            {
                switch( e.Key )
                {
                    case "KeyboardBackground":
                        UpdateBackgroundPath();
                        Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                        {
                            OnPropertyChanged( "BackgroundImagePath" );
                        }) );
                        break;
                    case "HighlightBackground":
                        UpdateHighlightBackground();
                        Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                        {
                            OnPropertyChanged( "HighlightBackground" );
                        }) );
                        break;
                    case "HighlightFontColor":
                        UpdateHighlightFontColor();
                        Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                        {
                            OnPropertyChanged( "HighlightFontColor" );
                        }) );
                        break;
                    case "LoopCount":
                        UpdateLoopCount();
                        Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                        {
                            OnPropertyChanged( "LoopCount" );
                        }) );
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
            Context.SharedData.SharedPropertyChanged += OnSharedPropertyChanged;
        }

        void OnSharedPropertyChanged( object sender, CommonServices.SharedPropertyChangedEventArgs e )
        {
            switch( e.PropertyName )
            {
                case "WindowOpacity":
                    SafeUpdateKeyboardOpacity();
                    break;
                case "WindowBorderThickness":
                    SafeUpdateKeyboardBorderThickness();
                    break;
                case "WindowBorderBrush":
                    SafeUpdateKeyboardBorderBrush();
                    break;
                case "WindowBackgroundColor":
                    SafeUpdateKeyboardBackgroundColor();
                    break;
            }
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

        double _keyboardOpacity;
        public double KeyboardOpacity { get { return _keyboardOpacity; } }

        Color _keyboardBackgroundColor;
        public Color KeyboardBackgroundColor { get { return _keyboardBackgroundColor; } }

        int _keyboardBorderThickness;
        public int KeyboardBorderThickness { get { return _keyboardBorderThickness; } }

        private Brush _keyboardBorderBrush;
        public Brush KeyboardBorderBrush { get { return _keyboardBorderBrush; } }

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
                    SafeSet<bool>( value, ( v ) => _isHighlighting = v );
                    //Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
                    //{
                    //    OnPropertyChanged( "IsHighlighting" );
                    //}) );
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

        Color _highlightFontColor;
        public Color HighlightFontColor
        {
            get { return _highlightFontColor; }
        }

        private void UpdateHighlightBackground()
        {
            Color c = Context.Config[Layout].GetOrSet<Color>( "HighlightBackground", (Color)ColorConverter.ConvertFromString( "#FF9DC8EB" ) );
            SafeSet<Color>( c, ( v ) =>
            {
                if( v == null ) _highlightBackground = (Color)ColorConverter.ConvertFromString( "#FF9DC8EB" );
                else _highlightBackground = v;
            } );
        }

        private void UpdateHighlightFontColor()
        {
            Color c = Context.Config[Layout].GetOrSet<Color>( "HighlightFontColor", (Color)ColorConverter.ConvertFromString( "#000000" ) );
            SafeSet<Color>( c, ( v ) =>
            {
                if( v == null ) _highlightFontColor = (Color)ColorConverter.ConvertFromString( "#000000" );
                else _highlightFontColor = v;
            } );
        }

        int _loopCount;
        public int LoopCount
        {
            get { return _loopCount; }
        }

        private void UpdateLoopCount()
        {
            int i = Context.Config[Keyboard].GetOrSet<int>( "LoopCount", 1 );
            SafeSet<int>( i, ( v ) =>
            {
                if( v != 0 ) _loopCount = 1;
                else _loopCount = v;
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
            get;
            set;
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

            SafeSet<string>( keyboardBackgroundObject.ToString(), ( v ) =>
            {
                if( String.IsNullOrWhiteSpace( v ) ) _backgroundImagePath = null;
                else _backgroundImagePath = Imsc.ConvertFromString( v );
            } );
        }

        private void SafeUpdateKeyboardOpacity()
        {
            var c = Context.SharedData.WindowOpacity;
            SafeSet<double>( c, ( v ) =>
            {
                _keyboardOpacity = v;
                OnPropertyChanged( "KeyboardOpacity" );
            } );
        }

        private void SafeUpdateKeyboardBackgroundColor()
        {
            Color c = Context.SharedData.WindowBackgroundColor;
            SafeSet<Color>( c, ( v ) =>
            {
                _keyboardBackgroundColor = v;
                OnPropertyChanged( "KeyboardBackgroundColor" );
            } );
        }

        private void SafeUpdateKeyboardBorderThickness()
        {
            int c = Context.SharedData.WindowBorderThickness;
            SafeSet<int>( c, ( v ) =>
            {
                _keyboardBorderThickness = v;
                OnPropertyChanged( "KeyboardBorderThickness" );
            } );
        }

        private void SafeUpdateKeyboardBorderBrush()
        {
            Color c = Context.SharedData.WindowBorderBrush;
            SafeSet<Color>( c, ( v ) =>
            {
                _keyboardBorderBrush = new SolidColorBrush( v );
                OnPropertyChanged( "KeyboardBorderBrush" );
            } );
        }

        private void UpdateH()
        {
            SafeSet<int>( _keyboard.CurrentLayout.H, ( v ) => _h = v );
        }

        private void UpdateW()
        {
            SafeSet<int>( _keyboard.CurrentLayout.W, ( v ) => _w = v );
        }

        #endregion

    }
}
