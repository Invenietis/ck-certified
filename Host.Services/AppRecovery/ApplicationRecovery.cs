using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Windows;

namespace CK.AppRecovery
{

    public static class ApplicationRecovery
    {
        static IRecoveryFunctions _impl;
        static int _crashCount;
        static bool _isOSRecoveryAvailable;
        static List<string> _registeredExceptions;

        delegate int ApplicationRecoveryCallback( IntPtr pvParameter );
        // Keeps the static to avoid the delegate to be garbage collected.
        static ApplicationRecoveryCallback _recoverCallback;

        const string _crashcountParameterName = "CrashCountParameter";
        [Flags]
        public enum RestartFlags
        {
            NONE = 0,
            RESTART_NO_CRASH = 1,
            RESTART_NO_HANG = 2,
            RESTART_NO_PATCH = 4,
            RESTART_NO_REBOOT = 8
        }

        static public bool IsOSRecoveryAvailable
        {
            get { return _registeredExceptions != null ? _isOSRecoveryAvailable : CK.Core.OSVersionInfo.IsWindowsVistaOrGreater; }
        }

        public static event EventHandler<ApplicationCrashedEventArgs> ApplicationCrashed;

        public static int CurrentCrashCount { get { return _crashCount; } }

        /// <summary>
        /// Registers the application for notification by windows of a failure if <see cref="IsOSRecoveryAvailable"/> is true.
        /// </summary>
        /// <returns>True if successfully registered for restart notification. False otherwise.</returns>   
        /// <remarks>
        /// <para>
        /// The <see cref="AppDomain.CurrentDomain.UnhandledException"/> event is listened and react to such unhandled exceptions
        /// int two different ways: If <see cref="IsOSRecoveryAvailable"/> is true, the exception is memorized and will be dumped out
        /// to the crash log during application recovery. If it is false, this immediately triggers the <see cref="ApplicationCrashed"/>
        /// event.
        /// </para>
        /// <para>
        /// Windows forms is configured to let unhandled execptions be thrown thanks to a call to <see cref="Application.SetUnhandledExceptionMode"/> with <see cref="UnhandledExceptionMode.ThrowException"/>.
        /// </para>
        /// <para>
        /// This method also initializes the <see cref="CurrentCrashCount"/> from the <see cref="Environment.GetCommandLineArgs"/> (if any).
        /// </para>
        /// </remarks>
        public static bool Initialize()
        {
            if( _registeredExceptions != null ) return _isOSRecoveryAvailable;
            _registeredExceptions = new List<string>();
            System.Windows.Forms.Application.SetUnhandledExceptionMode( System.Windows.Forms.UnhandledExceptionMode.ThrowException );
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( CurrentDomain_UnhandledException );
            if( !CK.Core.OSVersionInfo.IsWindowsVistaOrGreater ) return _isOSRecoveryAvailable = false;
            string[] args = Environment.GetCommandLineArgs();
            if( args != null && args.Length == 3 && args[1] == _crashcountParameterName )
            {
                int.TryParse( args[2], out _crashCount );
            }
            // TODO: Parameters passing is far from perfect here. One should better reinject the current paramaters and append the crash count.
            uint i = RecoveryFunctions.RegisterApplicationRestart( String.Format( "{0} {1}", _crashcountParameterName, _crashCount + 1 ), RestartFlags.NONE );
            if( i == 0 )
            {
                // Hook the callback function.
                _recoverCallback = new ApplicationRecoveryCallback( HandleApplicationCrashByOS );
                IntPtr ptrOnApplicationCrash = Marshal.GetFunctionPointerForDelegate( _recoverCallback );
                i = RecoveryFunctions.RegisterApplicationRecoveryCallback( ptrOnApplicationCrash, IntPtr.Zero, 50000, 0 );
            }
            return _isOSRecoveryAvailable = i == 0;
        }

        static IRecoveryFunctions RecoveryFunctions
        {
            get
            {
                return _impl ?? (_impl = CK.Interop.PInvoker.GetInvoker<IRecoveryFunctions>());
            }
        }

        static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
        {
            // If it has been initialized.
            if( _registeredExceptions != null )
            {
                using( StringWriter w = new StringWriter() )
                {
                    CrashLogWriter.WriteException( w, e.ExceptionObject );
                    _registeredExceptions.Add( w.GetStringBuilder().ToString() );
                }
                if( !_isOSRecoveryAvailable || Debugger.IsAttached )
                {
                    RaiseApplicationCrashedEvent();
                    // To FailFast or not to FailFast? That's the question...
                    // I prefer here to fail aggressively if the IsTerminating is false...
                    if( !e.IsTerminating ) Environment.FailFast( "Unhandled exceptions are evil. (Spi)" );
                }
            }
        }

        /// <summary>
        /// This is the callback function that is executed in the event of the application crashing.
        /// </summary>
        /// <param name="pvParameter">Unused unmanaged pointer.</param>
        /// <returns>Always returns 0.</returns>
        private static int HandleApplicationCrashByOS( IntPtr pvParameter )
        {
            Debug.Assert( IsOSRecoveryAvailable );
            // Allow the user to cancel the recovery. The timer polls for that cancel.
            using( System.Threading.Timer t = new System.Threading.Timer( CheckForRecoveryCancel, null, 1000, 1000 ) )
            {
                RaiseApplicationCrashedEvent();
                RecoveryFunctions.ApplicationRecoveryFinished( true );
            }
            return 0;
        }

        private static void RaiseApplicationCrashedEvent()
        {
            var h = ApplicationCrashed;
            if( h != null )
            {
                using( var e = ApplicationCrashedEventArgs.InitOnce() )
                {
                    Delegate[] all = h.GetInvocationList();
                    foreach( Delegate d in all )
                    {
                        try
                        {
                            d.DynamicInvoke( null, e );
                        }
                        catch( Exception ex )
                        {
                            e.CrashLog.Writer.WriteLine();
                            e.CrashLog.Writer.WriteLine( "== Exception in ApplicationCrashed handling: Type={0}", d.Method.DeclaringType.AssemblyQualifiedName );
                            e.CrashLog.WriteException( ex );
                            e.CrashLog.Writer.WriteLine( "== End of Exception in ApplicationCrashed handling" );
                        }
                    }
                    if( e.CrashLog.IsValid )
                    {
                        e.CrashLog.Writer.WriteLine( "== Registered Exceptions" );
                        if( _registeredExceptions.Count > 0 )
                        {
                            foreach( string oEx in _registeredExceptions )
                            {
                                e.CrashLog.Writer.WriteLine( oEx );
                            }
                        }
                        e.CrashLog.Writer.WriteLine( "== End of Registered Exceptions" );
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the user has cancelled the recovery.
        /// </summary>
        /// <param name="o"></param>
        private static void CheckForRecoveryCancel( object o )
        {
            Debug.Assert( IsOSRecoveryAvailable );

            bool userCancelled;
            RecoveryFunctions.ApplicationRecoveryInProgress( out userCancelled );
            if( userCancelled )
            {
                Environment.FailFast( "User cancelled application recovery." );
            }
        }


    }
}
