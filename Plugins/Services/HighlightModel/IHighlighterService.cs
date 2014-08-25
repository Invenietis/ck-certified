#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\HighlightModel\IHighlighterService.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Plugin;
using HighlightModel;

namespace CommonServices.Accessibility
{
    /// <summary>
    /// Service that allow other plugins to register themselves in order to be highlighted.
    /// </summary>
    public interface IHighlighterService : IDynamicService
    {
        /// <summary>
        /// Gets if the highlighter is running or not (maybe stopped, or just paused).
        /// </summary>
        bool IsHighlighting { get; }

        /// <summary>
        /// This function forces the highlight of the given element.
        /// </summary>
        /// <param name="element">The element highlight.</param>
        /// <remarks> 
        /// This function is useful only when there is no alternative to.
        /// </remarks>
        void HighlightImmediately( IHighlightableElement element );

        /// <summary>
        /// Register a highlightable tree in the service in order to be available for the highlighting.
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="root"></param>
        void RegisterTree( string elementID, string elementDisplayName, IHighlightableElement root, bool HighlightDirectly = false );

        /// <summary>
        /// Remove an tree that have been registered before. It can be a subtree of any highlightable element.
        /// </summary>
        /// <remarks>The elementID and the element must match the association made ​​in the collection</remarks>
        /// <param name="elementID"></param>
        /// <param name="element"></param>
        void UnregisterTree( string elementID, IHighlightableElement element );

        /// <summary>
        /// Adds an element at the beginning or end of an existing element that implements <see cref="IExtensibleHighlightableElement"/>.
        /// </summary>
        /// <param name="targetModuleName">It is the elementID in which we will search  the <see cref="IExtensibleHighlightableElement"/> to add the element</param>
        /// <param name="extensibleElementName">It is the name of <see cref="IExtensibleHighlightableElement"/> in which we want to add the element</param>
        /// <param name="position">Pre or Post position of the element</param>
        /// <param name="element"></param>
        /// <returns>Return true, if the targetModuleName contains the wanted <see cref="IExtensibleHighlightableElement"/> and if the <see cref="IExtensibleHighlightableElement"/> doesn't already contain the added element. Otherwise return false</returns>
        bool RegisterInRegisteredElementAt( string targetModuleName, string extensibleElementName, ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// Removes an element at the beginning or end of an existing element that implements <see cref="IExtensibleHighlightableElement"/>.
        /// </summary>
        /// <param name="targetModuleName">It is the elementID in which we will search  the <see cref="IExtensibleHighlightableElement"/> to remove the element</param>
        /// <param name="extensibleElementName">It is the name of <see cref="IExtensibleHighlightableElement"/> in which we want to remove the element</param>
        /// <param name="position">Pre or Post position of the element</param>
        /// <param name="element"></param>
        /// <returns>Return true, if the targetModuleName contains the wanted <see cref="IExtensibleHighlightableElement"/> and if the <see cref="IExtensibleHighlightableElement"/> contains the removed element. Otherwise return false</returns>
        bool UnregisterInRegisteredElement( string targetModuleName, string extensibleElementName, ChildPosition position, IHighlightableElement element );

        /// <summary>
        /// Pause the highlighter scroller. Call Resume to resume the execution where it was paused.
        /// </summary>
        /// <param name="forceEndHighlight">If set to true, the highlighter will manually fire Endhighlight event to unselect the potentially highlighted current element</param>
        void Pause( bool forceEndHighlight = false );

        /// <summary>
        /// Resume the highlighter scroller.
        /// </summary>
        void Resume();

        IDictionary<string, string> RegisteredElements { get; }

        event EventHandler<HighlightElementRegisterEventArgs> ElementRegisteredOrUnregistered;

        event EventHandler<HighlightEventArgs> BeginHighlight;

        event EventHandler<HighlightEventArgs> EndHighlight;
        
    }

    public class HighlightEventArgs : EventArgs
    {
        public HighlightEventArgs( IHighlightableElement element )
        {
            Element = element;
        }

        public IHighlightableElement Element { get; private set; }
    }

    public class HighlightElementRegisterEventArgs : EventArgs
    {
        public HighlightElementRegisterEventArgs( IHighlightableElement element, string internalName, bool hasRegistered, bool isRoot )
        {
            Element = element;
            InternalName = internalName;
            HasRegistered = hasRegistered;
            IsRoot = isRoot;
        }

        public readonly IHighlightableElement Element;
        public readonly string InternalName;
        public readonly bool HasRegistered;
        public readonly bool IsRoot;
    }
}
