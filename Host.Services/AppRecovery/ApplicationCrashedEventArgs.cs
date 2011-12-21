using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace CK.AppRecovery
{
    public class ApplicationCrashedEventArgs : EventArgs, IDisposable
    {
        static object _lock = new Object();
        static ApplicationCrashedEventArgs _oneCrashOnly;

        public CrashLogWriter CrashLog { get; private set; }

        ApplicationCrashedEventArgs( CrashLogWriter w )
        {
            Debug.Assert( w != null );
            CrashLog = w;
            w.WriteProperty( "CurrentCrashCount", ApplicationRecovery.CurrentCrashCount );
        }

        void IDisposable.Dispose()
        {
            CrashLog.Dispose();
        }

        static internal ApplicationCrashedEventArgs InitOnce()
        {
            lock( _lock )
            {
                if( _oneCrashOnly != null ) return null;
                return _oneCrashOnly = new ApplicationCrashedEventArgs( CrashLogManager.CreateNewCrashLog() );
            }
        }

    }

}
