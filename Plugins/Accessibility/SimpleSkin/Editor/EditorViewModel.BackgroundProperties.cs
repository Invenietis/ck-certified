#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\Editor\EditorViewModel.BackgroundProperties.cs) is part of CiviKey. 
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
using Caliburn.Micro;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using SimpleSkin;
using CK.WPF.ViewModel;
using System.Windows;
using System.Windows.Media;

namespace SimpleSkinEditor
{
    public partial class EditorViewModel
    {
        public Color Background
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "Background", Colors.White ).Value; }
            set
            {
                _config[ConfigHolder]["Background"] = value;
                NotifyOfPropertyChange( () => HoverBackground );
                NotifyOfPropertyChange( () => PressedBackground );
            }
        }

        public Color HoverBackground
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "HoverBackground", Background ).Value; }
            set 
            {
                _config[ConfigHolder]["HoverBackground"] = value;
                NotifyOfPropertyChange( () => PressedBackground );
            }
        }

        public Color PressedBackground
        {
            get { return ConfigHolder.GetWrappedPropertyValue( _config, "PressedBackground", HoverBackground ).Value; }
            set { _config[ConfigHolder]["PressedBackground"] = value; }
        }
    }
}
