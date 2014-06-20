#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\ScrollingStrategies\OneByOneScrollingStrategy.cs) is part of CiviKey. 
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
using System.Windows.Threading;
using CK.Core;
using HighlightModel;
using CK.Plugin.Config;
using System.Diagnostics;
using System.Timers;
using System.Linq;
namespace Scroller
{
    /// <summary>
    /// A ScrollingStrategy that scroll only on sheets elements.
    /// </summary>
    [StrategyAttribute( OneByOneScrollingStrategy.StrategyName )]
    public class OneByOneScrollingStrategy : ScrollingStrategyBase
    {
        const string StrategyName = "OneByOneScrollingStrategy";
        public override string Name
        {
            get { return StrategyName; }
        }

        protected override void ProcessSkipBehavior(ActionType action)
        { 
            switch( Walker.Current.Skip )
            {
                case SkippingBehavior.Skip:
                    MoveNext( ActionType.MoveNext );
                    break;
                default:

                    if( Walker.Current.Children.Count > 0 && !Walker.Current.IsHighlightableTreeRoot || Walker.Current.Skip == SkippingBehavior.EnterChildren || Walker.Sibblings.Count( s => s.Skip != SkippingBehavior.Skip ) == 1 && Walker.Current.Children.Count > 0 )
                    {
                        if( action != ActionType.UpToParent )
                            MoveNext( ActionType.EnterChild );
                        else
                            MoveNext( ActionType.MoveNext );
                    }
                    break;
            }
        }

        public override void OnExternalEvent()
        {
            if( Walker.Current != null )
            {
                FireSelectElement();
            }
        }
    }
}
