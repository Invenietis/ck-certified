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
using System.Linq;
using System.Windows;
using CK.Windows.App;
using System.Threading;
using System.Globalization;
using System.Reflection;
using Host.Services.Helpers;

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

            //uncomment if you wan not to be told when WPF binding fails (can be useful to remove the annoying CK-Desktop's "ImagePath is null" errors.
            //Be careful, it will also remove the other binding errors
#if DEBUG
            //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
#endif

            CultureInfo ci = new CultureInfo( "fr-FR" );
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;

            //Getting the distributionname from AssemblyInfo
            string distributionName = "Std";
            var attribute = Assembly.GetExecutingAssembly()
                                        .GetCustomAttributes( typeof( DistributionAttribute ), false )
                                        .Cast<DistributionAttribute>().SingleOrDefault();
            if( attribute != null && !String.IsNullOrWhiteSpace(attribute.DistributionName) ) distributionName = attribute.DistributionName; 
            

            // Crash logs upload and updater availability is managed during this initialization.
             
            using( var init = CKApp.Initialize( new CKAppParameters( "CiviKey", distributionName ) ) )
            {
                // Common logger is actually bound to log4net.UpdateDone

                // CK-Windows must not depend on log4Net: its initialization must be done here.
                CommonLogger.Initialize( CKApp.CurrentParameters.ApplicationDataPath + @"AppLogs\", false );
                if( init != null )
                {
                    CKApp.Run( () =>
                    {
                        App app = new App();
                        app.InitializeComponent();
                        CivikeyStandardHost.Instance.CreateContext();
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
