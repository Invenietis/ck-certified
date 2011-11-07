using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Linq;
using CK.AppRecovery;
using System.Diagnostics;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Host.Resources;
//using CK.Interop;

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
            string culture = "fr-FR";
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo( culture );

            // This is the mutex used as a flag for installer.
            // Since it is "Global\", it is shared among multiple terminal services client.
            string uniqueAppKeyForInstaller = @"Global\Install-Civikey-Standard-974C8CB3EF148F145";
            Mutex mutextForInstaller = new Mutex( false, uniqueAppKeyForInstaller );

            // This unique Application key is "local": it is not shared among terminal server connections.
            string uniqueAppKey = @"Local\Civikey-Standard-BB63974C14584E6A";
            bool isNew;
            using( Mutex mutex = new Mutex( true, uniqueAppKey, out isNew ) )
            {
                if( isNew )
                {
                    Debug.Assert( CivikeyStandardHost.Instance.CommonApplicationDataPath.EndsWith( @"\" ) );
                    CrashLogManager.CrashLogDirectory = CivikeyStandardHost.Instance.ApplicationDataPath + @"CrashLogs\";
                    ApplicationRecovery.Initialize();

                    CommonLogger.Initialize( CivikeyStandardHost.Instance.ApplicationDataPath + @"AppLogs\", false );
                    ApplicationRecovery.ApplicationCrashed += CommonLogger.OnApplicationCrash;

                    // Here is where handling any existing CrashLogDirectory must be processed.
                    // If the logs are sent (or are to be sent), this directory must no more exist otherwise the user will
                    // be prompted again next time the application runs.
                    CrashLogManager.HandleExistingCrashLogs();
                    
                    string updateFile = Path.Combine( Path.GetTempPath(), CivikeyStandardHost.Instance.AppName + @"\Updates\Update.exe" );
                    string isUpdateDone = Path.Combine( Path.GetTempPath(), CivikeyStandardHost.Instance.AppName + @"\UpdateDone" );

                    if( File.Exists( isUpdateDone ) )
                    {
                        if( File.Exists( updateFile ) ) File.Delete( updateFile );
                        File.Delete( isUpdateDone );
                    }
                    
                    if( File.Exists( updateFile ) )
                    {
                        if( MessageBox.Show( R.UpdateMessage, R.Update, MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                        {
                            Process.Start( updateFile );
                            return;
                        }
                    }
                    // Application itself.
                    {
                        SplashScreen splashScreen = new SplashScreen( "App/Views/splash.png" );
                        splashScreen.Show( false );

                        CivikeyStandardHost.Instance.CreateContext();
                        Host.App app = new Host.App();
                        app.InitializeComponent();

                        // Due to a bug in the SplashScreen control, we need to catch exceptions while trying to close it.
                        try
                        {
                            splashScreen.Close( TimeSpan.FromSeconds( 0.3 ) );
                        }
                        catch
                        {
                            try
                            {
                                splashScreen.Close( TimeSpan.Zero );
                            }
                            catch { }
                        }
                        app.Run();
                    }
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

        //public Window GetTopWindow()
        //{
        //    var byHandle = Windows.OfType<Window>().ToDictionary( win => new WindowInteropHelper( win ) );
        //    IntPtr hWnd = GetTopWindow( IntPtr.Zero );
        //    while( hWnd != IntPtr.Zero )
        //    {
        //        if( byHandle.ContainsKey( hWnd ) ) return byHandle[hWnd];
        //        hWnd = GetNextWindow( hWnd, GW_HWNDNEXT );
        //    }
        //    return null;
        //}

        //const uint GW_HWNDNEXT = 2;

        //[DllImport( "User32" )]
        //extern static IntPtr GetTopWindow( IntPtr hWnd );

        //[DllImport( "User32" )]
        //extern static IntPtr GetNextWindow( IntPtr hWnd, uint wCmd ); 


    }
}
