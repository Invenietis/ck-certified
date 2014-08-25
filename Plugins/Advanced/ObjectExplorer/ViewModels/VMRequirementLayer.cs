#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMRequirementLayer.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Linq;
using CK.WPF.ViewModel;
using CK.Plugin;

namespace CK.Plugins.ObjectExplorer.ViewModels
{
   

    public class VMRequirementLayer : VMBase
    {
        RequirementLayer _layer;
        VMICoreElement _element;
        CoreElementType _type;

        private enum CoreElementType
        {
            Unknown = 0,
            Plugin = 1,
            Service = 2
        }

        /// <summary>
        /// Gets whether this requirement layer has a requirement for the current plugin or service
        /// </summary>
        public bool HasRequirement
        {
            get
            {
                if( _type == CoreElementType.Plugin ) return _layer.PluginRequirements.FirstOrDefault( ( p ) => p.PluginId == ( (VMIPlugin)_element.Data ).Id ) != null;
                else if( _type == CoreElementType.Service ) return _layer.ServiceRequirements.FirstOrDefault( ( p ) => p.AssemblyQualifiedName == ( (VMIService)_element.Data ).AssembyQualifiedName ) != null;
                return false;
            }
        }

        /// <summary>
        /// Name of the layer
        /// </summary>
        public String Name { get { return _layer.LayerName; } }

        /// <summary>
        /// Gets the requirement that can be found in the layer
        /// Returns Optional if no requirements can be found
        /// </summary>
        public RunningRequirement Requirement { get { return GetRequirement(); } }

        public VMRequirementLayer( VMICoreElement element, RequirementLayer layer )
        {
            _layer = layer;
            _element = element;
            _type = CoreElementType.Unknown;
            element = ExtractCoreElementType( element );
        }

        private VMICoreElement ExtractCoreElementType( VMICoreElement element )
        {
            object el;

            //If the element is a VMAlias, we'll process only the Data that its wraps, that can either be a service, or a plugin 
            el = element as VMAlias<VMICoreElement>;
            if( el != null )
            {
                element = (VMICoreElement)( (VMAlias<VMICoreElement>)el ).Data;
            }

            el = element as VMIPlugin;
            if( el != null )
            {
                _type = CoreElementType.Plugin;
            }
            else
            {
                el = element as VMIService;
                if( el != null )
                {
                    _type = CoreElementType.Service;
                }
            }
            return element;
        }

        private RunningRequirement GetRequirement()
        {
            if( _type == CoreElementType.Plugin )
            {
                var req = _layer.PluginRequirements.FirstOrDefault( ( p ) => p.PluginId == ( (VMIPlugin)_element.Data ).Id );
                if( req != null ) return req.Requirement;
            }
            else if( _type == CoreElementType.Service )
            {
                var req = _layer.ServiceRequirements.FirstOrDefault( ( p ) => p.AssemblyQualifiedName == ( (VMIService)_element.Data ).AssembyQualifiedName );
                if( req != null ) return req.Requirement;
            }
            return RunningRequirement.Optional;
        }
    }
}
