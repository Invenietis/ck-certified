#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMKeySimple.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using HighlightModel;
using CK.Core;

namespace SimpleSkin.ViewModels
{
    /// <summary>
    /// In this "Simple" implementation we don't use this model level.
    /// KeyModes are handled by the upper level : see <see cref="VMKeySimple"/>
    /// </summary>
    internal class VMKeyModeSimple : VMKeyMode<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple, VMKeyModeSimple, VMLayoutKeyModeSimple>
    {
        public VMKeyModeSimple( VMContextSimple ctx, IKeyMode keyMode )
            : base( ctx, keyMode )
        {
            
        }

        public override VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple, VMKeyModeSimple, VMLayoutKeyModeSimple> Parent
        {
            get { return null; }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return null; }
        }

        public override bool IsSelected
        {
            get { return false; }
            set { }
        }
    }
}
