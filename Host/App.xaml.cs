#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\App.xaml.cs) is part of CiviKey. 
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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using CK.Windows.App;
using System.Threading;
using System.Globalization;

namespace Host
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current { get { return (App)Application.Current; } }

        [STAThread]
        public static void Main( string[] args )
        {
            CultureInfo ci = new CultureInfo( "fr-FR" );
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;

            // Crash logs upload and updater availability is managed during this initialization.
            using( var init = CKApp.Initialize( new CKAppParameters( "CiviKey", "Std" ) ) )
            {
                // Common logger is actually bound to log4net.UpdateDone

                // CK-Windows must not depend on log4Net: its initialization must be done here.
                CommonLogger.Initialize( CKApp.CurrentParameters.ApplicationDataPath + @"AppLogs\", false );
                if( init != null )
                {
                    CKApp.Run( () =>
                    {
                        CivikeyStandardHost.Instance.CreateContext();
                        App app = new App();
                        app.InitializeComponent();
                        return app;
                    } );
                }
            }
        }

        public Window GetTopWindow()
        {
            Window top = null;
            foreach( Window w in Windows )
            {
                if( top == null || w.Topmost ) top = w;
            }
            return top;
        }
    }
}
