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

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using HighlightModel;
using CK.Core;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace SimpleSkin.ViewModels
{
    internal class VMZoneSimple : VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>, IHighlightableElement
    {
        public ObservableSortedArrayKeyList<VMKeySimple, int> Keys { get { return _keys; } }
        ObservableSortedArrayKeyList<VMKeySimple, int> _keys;
        public string Name { get { return _zone.Name; } }
        IZone _zone;

        public VMZoneSimple( VMContextSimple ctx, IZone zone ) 
            : base( ctx )
        {
             _zone = zone;
            _keys = new ObservableSortedArrayKeyList<VMKeySimple, int>( k => k.Index );

            foreach( IKey key in _zone.Keys )
            {
                VMKeySimple k = Context.Obtain( key );
                Keys.Add( k );
            }
        }

        protected override void OnDispose()
        {
            foreach( VMKeySimple key in Keys )
            {
                key.Dispose();
            }
        }

        public IReadOnlyList<IHighlightableElement> Children
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
                if( string.IsNullOrEmpty( Name ) || Keys.All( k => k.Skip == SkippingBehavior.Skip ) )
                {
                    return SkippingBehavior.Skip;
                }
                else if( Keys.Count == 1 )
                {
                    return SkippingBehavior.EnterChildren;
                }
                return SkippingBehavior.None;
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
                    _isHighlighting = value;
                    OnPropertyChanged( "IsHighlighting" );
                    foreach( var key in Keys )
                    {
                        key.IsHighlighting = value;
                    }
                }
            }
        }
    }
}
