#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMZoneSimple.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;
using HighlightModel;

namespace SimpleSkin.ViewModels
{
    public class VMZoneSimple : VMContextElement, IHighlightableElement, IHighlightableElementController
    {
        public CKObservableSortedArrayKeyList<VMKeySimple, int> Keys { get { return _keys; } }
        protected CKObservableSortedArrayKeyList<VMKeySimple, int> _keys;
        public string Name { get { return _zone.Name; } }
        IZone _zone;

        private int _loopCount;
        private int _initialLoopCount;
        private int _index;

        public int LoopCount
        {
            get { return _loopCount; }
        }

        private void SafeUpdateLoopCount()
        {
            SafeSet<int>( _zone.GetPropertyValue( Context.Config, "LoopCount", LoopCount ), ( v ) => _initialLoopCount = _loopCount = v );
        }

        //private void UpdateLoopCount()
        //{
        //    int i = Context.Config[_zone].GetOrSet<int>( "LoopCount", 1 );
        //    SafeSet<int>( i, ( v ) =>
        //    {
        //        if( v != 0 ) _loopCount = 1;
        //        else _loopCount = v;
        //    } );
        //}

        public int InitialLoopCount
        {
            get { return _initialLoopCount; }
        }

        public int Index
        {
            get { return _zone.Index; }
        }

        internal VMZoneSimple( VMContextSimpleBase ctx, IZone zone, int index )
            : base( ctx )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            _zone = zone;
            _index = index;

            _initialLoopCount = _loopCount = _zone.GetPropertyValue( Context.Config, "LoopCount", 1 );
            //SafeUpdateLoopCount();

            _keys = new CKObservableSortedArrayKeyList<VMKeySimple, int>( k => k.Index );

            foreach( IKey key in _zone.Keys )
            {
                VMKeySimple k = Context.Obtain( key );
                Keys.Add( k );
            }
            Context.Config.ConfigChanged += OnConfigChanged;
        }

        void OnConfigChanged( object sender, CK.Plugin.Config.ConfigChangedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            if( e.Obj == _zone && e.Key == "Index" )
            {
                _index = (int)e.Value;
                Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "Index" );
                }) );
                //TODO : trigger IndexChanged
            }
            if( _zone.CurrentLayout == e.Obj && e.Key == "LoopCount" )
            {
                Context.Config[_zone].Set( "LoopCount", e.Value );
            }
            if( e.Key == "LoopCount" )
            {
                _initialLoopCount = _loopCount = _zone.GetPropertyValue( Context.Config, "LoopCount", 1 );
                Context.NoFocusManager.NoFocusDispatcher.BeginInvoke( (Action)(() =>
                {
                    OnPropertyChanged( "LoopCount" );
                }) );
            }
        }

        public VMZoneSimple( VMContextSimpleBase ctx )
            : base( ctx )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            _keys = new CKObservableSortedArrayKeyList<VMKeySimple, int>( k => k.Index );
        }

        internal override void Dispose()
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
            Context.Config.ConfigChanged -= OnConfigChanged;

            foreach( var key in _keys )
            {
                key.Dispose();
            }

            Context.NoFocusManager.NoFocusDispatcher.Invoke( (Action)(() =>
            {
                Keys.Clear();
            }) );
        }

        #region IHighlightable members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return Keys; }
        }

        public int X
        {
            get { return Keys.Min( k => k.X ); }
        }

        public int Y
        {
            get { return Keys.Min( k => k.Y ); }
        }

        public int Width
        {
            get { return Keys.Max( k => k.X + k.Width ) - X; }
        }

        public int Height
        {
            get { return Keys.Max( k => k.Y + k.Height ) - Y; }
        }

        public SkippingBehavior Skip
        {
            get
            {
                Debug.Assert( Dispatcher.CurrentDispatcher == Context.NoFocusManager.ExternalDispatcher, "This method should only be called by the ExternalThread." );
                if( Keys.Count == 0 || Keys.All( k => k.Skip == SkippingBehavior.Skip ) )
                {
                    return SkippingBehavior.Skip;
                }
                else if( Keys.Count == 1 || Context.KeyboardVM.Zones.Count == 1 ) //if this zone has only one child or if this zone is the only one in the keyboard, 
                {                                                                                   //then we directly scroll through its children
                    return SkippingBehavior.EnterChildren;
                }

                return SkippingBehavior.None;

                //one case is not taken into account : when there are several zones, but only one has a skip behavior != Skip. In this case, we will not skip the zone, and highlight all the keys until the user triggers the event that makes the basicscroll enter the zone.
                //that can only happen when all the zones but one are not visible.
            }
        }

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
                    foreach( var key in Keys )
                    {
                        key.IsHighlighting = value;
                    }
                }
            }
        }

        public bool IsRoot
        {
            get { return false; }
        }

        #endregion

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
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return false; }
        }

        IHighlightableElement _previousElement;
        public ActionType PreviewChildAction( IHighlightableElement element, ActionType action )
        {
            ActionType a = action;
            if( _initialLoopCount != 1 && _previousElement != element && _keys[_keys.Count - 1] == element )
            {
                if( _loopCount == 1 )
                {
                    _loopCount = _initialLoopCount;
                    a = ActionType.UpToParent;
                }
                else
                {
                    _loopCount--;
                    a = ActionType.MoveToFirst;
                }
            }
            _previousElement = element;
            return a;
        }

        public void OnChildAction( ActionType action )
        {
            if( action == ActionType.GoToRelativeRoot || action == ActionType.GoToAbsoluteRoot )  _loopCount = _initialLoopCount;
        }

        #region IHighlightableElementUnregisterSensitive Members

        public void OnUnregisterTree()
        {
            //Reset the loopCount if we unregistered the element during a loop
            _loopCount = _initialLoopCount;
        }

        #endregion
    }
}
