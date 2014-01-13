#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMIBase.cs) is part of CiviKey. 
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
using CK.WPF.ViewModel;

namespace CK.Plugins.ObjectExplorer
{
    /// <summary>
    /// Basic class used to expose core properties. 
    /// It will be used by the <see cref="ObjectExplorer">Object Explorer plugin</see>.
    /// </summary>
    public abstract class VMIBase : VMBase, IDisposable
    {
        /// <summary>
        /// Gets a reference to the context.
        /// </summary>
        public VMIContextViewModel VMIContext { get; private set; }

        /// <summary>
        /// Gets the text that describe (shortly) the object.
        /// </summary>
        public string Label { get; protected set; }

        /// <summary>
        /// Gets the prefered path of the <see cref="VMIBase"/> into the treeview.
        /// </summary>
        public IList<VMIBase> Path { get { return null; } }

        /// <summary>
        /// Gets the <see cref="VMIBase">parent</see> of the <see cref="VMIBase"/>.
        /// </summary>
        public VMIBase Parent { get; private set; }
        
        /// <summary>
        /// Gets the name of the template that will be used 
        /// to display view model details.
        /// </summary>
        public string DetailsTemplateName { get; protected set; }

        public VMIBase( VMIContextViewModel context, VMIBase parent )
        {
            VMIContext = context;
            Parent = parent;
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}