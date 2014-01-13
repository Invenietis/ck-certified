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

using CK.Keyboard.Model;
using HighlightModel;
using CK.Core;
using System.Linq;

namespace SimpleSkin.ViewModels
{
    public class VMZoneSimple : VMContextElement, IHighlightableElement
    {
        public CKObservableSortedArrayKeyList<VMKeySimple, int> Keys { get { return _keys; } }
        protected CKObservableSortedArrayKeyList<VMKeySimple, int> _keys;
        public string Name { get { return _zone.Name; } }
        IZone _zone;

        private int _index;
        public int Index
        {
            get { return _zone.Index; }
        }

        internal VMZoneSimple( VMContextSimpleBase ctx, IZone zone, int index )
            : base( ctx )
        {
            _zone = zone;
            _index = index;
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
            if( e.Obj == _zone && e.Key == "Index" )
            {
                _index = (int)e.Value;
                OnPropertyChanged( "Index" );
                //TODO : trigger IndexChanged
            }
        }


        public VMZoneSimple( VMContextSimpleBase ctx )
            : base( ctx )
        {
            _keys = new CKObservableSortedArrayKeyList<VMKeySimple, int>( k => k.Index );
        }

        internal override void Dispose()
        {
            Keys.Clear();
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
                if( value != _isHighlighting )
                {
                    ThreadSafeSet<bool>( value, ( v ) => _isHighlighting = v );
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
    }
}
