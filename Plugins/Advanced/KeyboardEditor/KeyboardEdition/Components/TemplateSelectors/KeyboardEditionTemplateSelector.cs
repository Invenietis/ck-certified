#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Components\TemplateSelectors\KeyboardEditionTemplateSelector.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.ViewModels
{
    public class KeyboardEditionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyEditionTemplate { get; set; }
        public DataTemplate ZoneEditionTemplate { get; set; }
        public DataTemplate KeyboardEditionTemplate { get; set; }
        public DataTemplate KeyModeEditionTemplate { get; set; }
        public DataTemplate LayoutKeyModeEditionTemplate { get; set; }

        public override DataTemplate SelectTemplate( object item,
          DependencyObject container )
        {
            if( item is VMKeyEditable ) return KeyEditionTemplate;
            else if( item is VMZoneEditable ) return ZoneEditionTemplate;
            else if( item is VMKeyModeEditable ) return KeyModeEditionTemplate;
            else if( item is VMLayoutKeyModeEditable ) return LayoutKeyModeEditionTemplate;
            else return KeyboardEditionTemplate;
        }
    }
}
