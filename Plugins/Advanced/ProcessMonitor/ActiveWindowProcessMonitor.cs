using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessMonitor
{
    /// <summary>
    /// Utility class that advertises an event when the process of the active foreground window changes.
    /// </summary>
    /// <remarks>The returned Process instance will always be a new one. If you want to compare two Process instances, use Process.Id.</remarks>
    class ActiveWindowProcessMonitor : IDisposable
    {
        public event EventHandler<ActiveWindowProcessEventArgs> ActiveWindowProcessChanged;

        WinEventDelegate _eventDelegate;
        IntPtr _eventHook;
        int _lastProcessId = -1;

        public ActiveWindowProcessMonitor()
        {
            _eventDelegate = new WinEventDelegate( WinEventProc );
            _eventHook = SetWinEventHook( EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _eventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT );
        }

        delegate void WinEventDelegate( IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime );

        [DllImport( "user32.dll", SetLastError = true )]
        static extern IntPtr SetWinEventHook( uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags );

        [DllImport( "user32.dll", SetLastError = true )]
        static extern IntPtr GetForegroundWindow();

        [DllImport( "user32.dll", SetLastError = true )]
        static extern int GetWindowText( IntPtr hWnd, StringBuilder text, int count );

        [DllImport( "user32.dll", SetLastError = true )]
        static extern UInt32 GetWindowThreadProcessId( Int32 hWnd, out Int32 lpdwProcessId );

        [DllImport( "user32.dll", SetLastError = true )]
        static extern bool UnhookWinEvent( IntPtr hhk );

        static Int32 GetWindowProcessID( Int32 hwnd )
        {
            Int32 pid = 1;
            GetWindowThreadProcessId( hwnd, out pid );
            return pid;
        }

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder( nChars );
            handle = GetForegroundWindow();

            if( handle != IntPtr.Zero && GetWindowText( handle, Buff, nChars ) > 0 )
            {
                return Buff.ToString();
            }
            return null;
        }

        void WinEventProc( IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime )
        {
            Process p = GetForegroundWindowProcess();

            if( p != null )
            {
                string appName = p.ProcessName;

                if( p.Id != _lastProcessId )
                {
                    _lastProcessId = p.Id;
                    FireActiveWindowProcessChanged( p );
                }
            }
        }

        /// <summary>
        /// Gets the Process from the current foreground window.
        /// </summary>
        /// <remarks>The returned Process instance will always be a new one. If you want to compare two Process instances, use Process.Id.</remarks>
        /// <returns>A new instance of a System.Diagnostics.Process pointing on the process owning the foreground Window, or null when no active window is defined.</returns>
        public static Process GetForegroundWindowProcess()
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = GetForegroundWindow();

            if( hWnd != IntPtr.Zero )
            {
                Int32 pid = GetWindowProcessID( hWnd.ToInt32() );
                Process p = Process.GetProcessById( pid );

                return p;
            }
            else
            {
                return null;
            }
        }

        void FireActiveWindowProcessChanged( Process process )
        {
            if( ActiveWindowProcessChanged != null )
            {
                ActiveWindowProcessChanged( this, new ActiveWindowProcessEventArgs( process ) );
            }
        }

        #region IDisposable Members
        bool _disposed = false;

        public void Dispose()
        {
            if( !_disposed )
            {
                bool successfullyUnhooked = UnhookWinEvent( _eventHook );
                if( !successfullyUnhooked )
                {
                    throw new Win32Exception( Marshal.GetLastWin32Error() );
                }

                _eventDelegate = null;
                _eventHook = IntPtr.Zero;

                _disposed = true;
            }
        }

        #endregion
    }

    class ActiveWindowProcessEventArgs : EventArgs
    {
        readonly Process _activeWindowProcess;
        public Process ActiveWindowProcess { get { return _activeWindowProcess; } }

        public ActiveWindowProcessEventArgs( Process activeWindowProcess )
        {
            _activeWindowProcess = activeWindowProcess;
        }
    }
}
