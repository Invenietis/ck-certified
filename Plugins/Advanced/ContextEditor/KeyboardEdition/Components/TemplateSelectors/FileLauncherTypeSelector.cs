#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Components\TemplateSelectors\FileLauncherTypeSelector.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Controls;
using BasicCommandHandlers;

namespace KeyboardEditor.ViewModels
{
    public class FileLauncherTypeSelector : DataTemplateSelector
    {
        public DataTemplate UrlTemplate { get; set; }
        public DataTemplate RegistryTemplate { get; set; }
        public DataTemplate BrowseTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item == null ) return RegistryTemplate;

            FileLauncherTypeSelection selection = (FileLauncherTypeSelection) item;
            if( selection == null ) throw new InvalidOperationException("Expected a FileLauncherTypeSelection but was something else");
            
            if( selection.Type == FileLauncherType.Url ) return UrlTemplate;
            if( selection.Type == FileLauncherType.Registry ) return RegistryTemplate;
            return BrowseTemplate;
        }
    }
}
