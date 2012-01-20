using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using CK.Windows.App;

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
            // Crash logs upload and updater availability is managed during this initialization.
            using( var init = CKApp.Initialize( new CKAppParameters( "CiviKey", "Standard" ) ) )
            {
                // Common logger is actually bound to log4net.
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
