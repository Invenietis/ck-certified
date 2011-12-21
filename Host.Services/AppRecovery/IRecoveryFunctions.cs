using System;
using System.Runtime.InteropServices;
using CK.Interop;

namespace CK.AppRecovery
{
    [NativeDll( DefaultDllNameGeneric = "kernel32.dll" )]
    public interface IRecoveryFunctions
    {
        [CK.Interop.DllImport( CharSet = CharSet.Auto )]
        uint RegisterApplicationRestart( string pwsCommandLine, ApplicationRecovery.RestartFlags dwFlags );

        [CK.Interop.DllImport]
        uint RegisterApplicationRecoveryCallback( IntPtr pRecoveryCallback, IntPtr pvParameter, int dwPingInterval, int dwFlags );

        [CK.Interop.DllImport]
        uint ApplicationRecoveryInProgress( out bool pbCancelled );

        [CK.Interop.DllImport]
        uint ApplicationRecoveryFinished( bool bSuccess );
    }
}
