#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\ScrollingStrategies\ZoneSplittingScrollingStrategy.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;
using SimpleSkin.ViewModels;
using System.Timers;
using System;

namespace Scroller
{
    [Strategy( HalfZoneScrollingStrategy.StrategyName )]
    internal class HalfZoneScrollingStrategy : ZoneScrollingStrategy
    {
        public static readonly int ZoneDivider = 2;
        const string StrategyName = "HalfZoneScrollingStrategy";

        public HalfZoneScrollingStrategy() : base()
        {
            if( ZoneDivider < 2 ) throw new InvalidOperationException( "The ZoneDivider can't be less than 2 !" );

            Walker = new ZondeDivderWalker( this );
        }

        public override string Name
        {
            get { return StrategyName; }
        }
    }

    public class ZondeDivderWalker : TreeWalker
    {
        public ZondeDivderWalker(IHighlightableElement root) : base ( root )
        { }

        public override bool EnterChild()
        {
            if( Current.Children.Count == 0 ) return false;

            //False if the current element is not a root/relative root or if there are not enough children or if one child is an KeySimple
            if( !Current.IsHighlightableTreeRoot && Current.Children.Count > HalfZoneScrollingStrategy.ZoneDivider && Current.Children[0] as VMKeySimple != null && Peek() as VirtualZone == null )
            {
                IHighlightableElement old = Current;
                Current = new VirtualZone(Current);
                
                //if( old.IsHighlightableTreeRoot ) ((VirtualZone) Current).Skip = SkippingBehavior.Skip;

                var parent = Peek() as VirtualZone;
                if( parent != null ) parent.UpdateChild( old, Current );
            }

            return base.EnterChild();
        }

        public override bool MoveNext()
        {
            if( Sibblings.Count <= 1 ) //false if there are no sibblings at all
                return false;

            int idx = Sibblings.IndexOf( Current );

            //False when the element is found in its parents 
            //or when the parent is a root element and the current element is a virtualzone : 
            //Means that we may found the WrappedElement of the current VirtualZone in the sibblings list.
            if( idx < 0 && (Peek() == null || !Peek().IsHighlightableTreeRoot && Current as VirtualZone == null) ) 
                throw new InvalidOperationException( "Something goes wrong : the current element is not contained by its parent !" );
            
            if( idx >= 0 )
            {
                //The current child is the last one
                if( idx + 1 >= Sibblings.Count ) return false;

                Current = Sibblings.ElementAt( idx + 1 );
                return true;
            }

            //Here we get the wrapped element and restart the procedure.
            Current = ((VirtualZone) Current).WrappedElement;
            return MoveNext();
        }
    }
}
