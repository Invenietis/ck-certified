#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
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
using CK.Core;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using CommonServices;
using CK.Windows.App;

namespace KeyboardEditor.ViewModels
{
    public partial class VMKeyEditable : VMContextElementEditable
    {
        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<string>( Context.SkinConfiguration, "DisplayType", Context.SkinConfiguration[LayoutKeyMode]["Image"] != null ? "Image" : "Label" ) == "Label"; }
            set 
            { 
                if(value) Context.SkinConfiguration[LayoutKeyMode]["DisplayType"] = "Label";
                else Context.SkinConfiguration[LayoutKeyMode]["DisplayType"] = "Image"; 
            }
        }

        public bool ShowImage
        {
            get { return LayoutKeyMode.GetPropertyValue<string>( Context.SkinConfiguration, "DisplayType", Context.SkinConfiguration[LayoutKeyMode]["Image"] != null ? "Image" : "Label" ) == "Image"; }
            set 
            { 
                if(value) Context.SkinConfiguration[LayoutKeyMode]["DisplayType"] = "Image";
                else Context.SkinConfiguration[LayoutKeyMode]["DisplayType"] = "Label"; 
            }
        }

        /// <summary>
        /// Gets the opacity of the key.
        /// When the key has <see cref="IsBeingEdited"/> set to false, the opacity is lowered.
        /// returns 1 otherwise.
        /// </summary>
        public double Opacity
        {
            get { return IsBeingEdited ? 1 : 0.3; }
        }

        double _zIndex = 50;
        public double ZIndex 
        { 
            get { return _zIndex; } 
            set 
            { 
                _zIndex = value; 
                OnPropertyChanged( "ZIndex" ); 
            }
        }

        CK.Windows.App.VMCommand _selectLayoutKeyMode;
        public CK.Windows.App.VMCommand SelectLayoutKeyModeCommand
        {
            get
            {
                if( _selectLayoutKeyMode == null )
                {
                    _selectLayoutKeyMode = new CK.Windows.App.VMCommand( () =>
                    {
                         LayoutKeyModeVM.IsSelected = true;
                    });
                }
                return _selectLayoutKeyMode;
            }
        }
    }
}
