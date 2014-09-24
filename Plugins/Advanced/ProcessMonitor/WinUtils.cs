using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessMonitor
{
    internal delegate void WinEventDelegate( IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime );

    internal static class WinUtils
    {
        [DllImport( "user32.dll" )]
        public static extern IntPtr SetWinEventHook( uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags );
        
        [DllImport( "user32.dll" )]
        public static extern IntPtr GetForegroundWindow();

        [DllImport( "user32.dll" )]
        public static extern uint GetWindowThreadProcessId( IntPtr hWnd, out uint lpdwProcessId );

        [DllImport( "kernel32.dll" )]
        public static extern IntPtr OpenProcess( uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId );

        [DllImport( "psapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode )]
        public static extern uint GetModuleFileNameEx( IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize );


        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

    }
}
