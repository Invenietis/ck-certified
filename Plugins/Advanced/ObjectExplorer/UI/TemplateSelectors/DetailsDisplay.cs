#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\TemplateSelectors\DetailsDisplay.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace CK.Plugins.ObjectExplorer.UI.TemplateSelectors
{
    public class DetailsDisplay : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate( object item, System.Windows.DependencyObject container )
        {
            VMIBase inspector = item as VMIBase;
            if( inspector != null && inspector.DetailsTemplateName != null )
            {
                var finder = container as FrameworkElement;
                if( finder != null )
                    return finder.FindResource( inspector.DetailsTemplateName ) as DataTemplate;
            }
            return base.SelectTemplate( item, container );
        }
    }
}
