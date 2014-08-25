#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\ScrollingStrategies\Tools\IScrollingStrategy.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
using HighlightModel;

namespace Scroller
{
    internal interface IScrollingStrategy
    {
        /// <summary>
        /// Fired when an element is going to highlight
        /// </summary>
        event EventHandler<HighlightEventArgs> BeginHighlightElement;

        /// <summary>
        /// Fired when an element is going to unhighlight
        /// </summary>
        event EventHandler<HighlightEventArgs> EndHighlightElement;

        /// <summary>
        /// True, when the strategy is setup and have Elements
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Represent the state timer
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// The displayable unique strategy name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initialize the scrolling strategy with the given parameters
        /// </summary>
        /// <param name="timer">The heart beat timer</param>
        /// <param name="elements">the dictionnary of the registered elements</param>
        /// <param name="config">the config accessor</param>
        void Setup( DispatcherTimer timer, Func<ICKReadOnlyList<IHighlightableElement>> elements, IPluginConfigAccessor config );

        /// <summary>
        /// Start the scrolling strategy
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the scrolling strategy
        /// </summary>
        void Stop();

        /// <summary>
        /// This function forces the highlight of the given element.
        /// </summary>
        /// <param name="element">The element highlight.</param>
        /// <remarks> 
        /// This function is useful only when there is no alternative to.
        /// </remarks>
        void GoToElement( IHighlightableElement element );

        /// <summary>
        /// Suspend the scrolling strategy
        /// </summary>
        void Pause( bool forceEndHighlight );

        /// <summary>
        /// Resumes the scrillong strategy
        /// </summary>
        void Resume();

        /// <summary>
        /// Called after the trigger event is fired
        /// </summary>
        void OnExternalEvent();

        /// <summary>
        /// Warns the strategy that an element has been unregistered.
        /// Typically used to check whether the current element belongs to the unregistered element's tree, in order to call endhighlight on it.
        /// </summary>
        void ElementUnregistered( IHighlightableElement element );
    }
}
