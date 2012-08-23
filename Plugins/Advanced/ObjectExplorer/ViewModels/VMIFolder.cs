#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\ViewModels\VMIFolder.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Core;
using CK.WPF.ViewModel;
using System.Windows.Input;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CK.Plugins.ObjectExplorer
{
    public class VMIFolder : VMBase, ISelectableElement
    {
        public IEnumerable Items { get; private set; }

        public String Name { get; private set; }

        public VMIFolder( IEnumerable items, string name )
        {
            Name = name;
            Items = items;
        }

        #region ISelectableElement Members

        public bool IsSelected
        {
            get { return false; }
            set { }
        }

        public void SelectedChanged()
        {
            OnPropertyChanged( "IsSelected" );
        }

        #endregion
    }
}