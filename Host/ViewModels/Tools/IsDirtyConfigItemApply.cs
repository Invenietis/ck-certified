#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\Tools\IsDirtyConfigItemApply.cs) is part of CiviKey. 
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
using System.Windows.Input;
using CK.Windows.Config;
using System.Linq;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;

namespace Host.VM
{
    /// <summary>
    /// Handles an ICommand and an array of INotifySelectionChanged objects.
    /// This class keeps track of the selected object among its array.
    /// When the selected changes, the button is enabled. otherwise it is disabled.
    /// Note : if no objects are linked, works as a simple button triggering the action set as parameter of the constructor.
    /// </summary>
    public class IsDirtyConfigItemApply : ConfigItemAction
    {
        Func<bool> _getIsDirty;

        public IsDirtyConfigItemApply( ConfigManager configManager, ICommand cmd, Func<bool> getIsDirty )
            : base( configManager, cmd )
        {
            _getIsDirty = getIsDirty;
        }

        public bool IsEnabled
        {
            get
            {
                return _getIsDirty();
            }
        }

        public void UpdateIsEnabled()
        {
            NotifyOfPropertyChange( "IsEnabled" );
        }
    }
}
