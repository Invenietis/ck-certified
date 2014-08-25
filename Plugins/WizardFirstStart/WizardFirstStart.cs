#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\WizardFirstStart\WizardFirstStart.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Navigation;
using Caliburn.Micro;
using CK.Plugin;

namespace CK.Plugins.WizardFirstStart
{
    [Plugin( "{A4CD5FA6-C9ED-40D5-9BF7-B5C96ADA73C0}", PublicName = "Wizard First Start" )]
    public class WizardFirstStart : IPlugin
    {
        WizardViewModel _w;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            WindowManager windowManager = new WindowManager();
            _w = new WizardViewModel();
            windowManager.ShowWindow( _w, null, null );
        }

        public void Stop()
        {
            _w.TryClose();
        }

        public void Teardown()
        {
        }
    }
}
