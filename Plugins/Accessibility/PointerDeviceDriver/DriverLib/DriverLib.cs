#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\PointerDeviceDriver\DriverLib\DriverLib.cs) is part of CiviKey. 
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

//****************************************************
// Author : Isaac Duplan (Duplan@intechinfo.fr)
// Date : 01-05-2008
//****************************************************
using System;
using System.Runtime.InteropServices;


namespace PointerDeviceDriver
{

    public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);


    internal static class Win32Wrapper
    {
        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        internal static extern bool GetCursorPos( out PointStruct point );

        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        internal static extern IntPtr SetWindowsHookEx( HookType idHook, HookProc lpfn, IntPtr hInstance, int threadId );

        //This is the Import for the UnhookWindowsHookEx function.
        //Call this function to uninstall the hook.
        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        internal static extern bool UnhookWindowsHookEx( int idHook );

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport( "user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall )]
        internal static extern int CallNextHookEx( IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam );

        //This is the Import for the mouse_event function.
        //Use this function to send mouse event to system
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern void mouse_event(MouseEventFlags dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        //This is the Import for SetCursorPos From User32.Dll
        //It allow us to set the Cursor position
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

    }



    internal enum HookType : int
    {
        /// <summary>
        /// The WH_JOURNALRECORD hook enables you to monitor and record input events.
        /// Typically, you use this hook to record a sequence of mouse and keyboard events to play back later by using the WH_JOURNALPLAYBACK Hook.
        /// The WH_JOURNALRECORD hook is a global hook — it cannot be used as a thread-specific hook.
        /// </summary>
        WH_JOURNALRECORD = 0,
        /// <summary>
        /// The WH_JOURNALPLAYBACK hook enables an application to insert messages into the system message queue.
        /// You can use this hook to play back a series of mouse and keyboard events recorded earlier by using the WH_JOURNALRECORD Hook.
        /// Regular mouse and keyboard input is disabled as long as a WH_JOURNALPLAYBACK hook is installed.
        /// A WH_JOURNALPLAYBACK hook is a global hook — it cannot be used as a thread-specific hook.
        /// </summary>
        WH_JOURNALPLAYBACK = 1,
        /// <summary>
        /// The WH_KEYBOARD hook enables an application to monitor message traffic for WM_KEYDOWN and WM_KEYUP messages
        /// about to be returned by the GetMessage or PeekMessage function.
        /// You can use the WH_KEYBOARD hook to monitor keyboard input posted to a message queue.
        /// </summary>
        WH_KEYBOARD = 2,
        /// <summary>
        /// The WH_GETMESSAGE hook enables an application to monitor messages about to be returned by the GetMessage or PeekMessage function.
        /// You can use the WH_GETMESSAGE hook to monitor mouse and keyboard input and other messages posted to the message queue.
        /// </summary>
        WH_GETMESSAGE = 3,
        /// <summary>
        /// The WH_CALLWNDPROC and WH_CALLWNDPROCRET hooks enable you to monitor messages sent to window procedures.
        /// The system calls a WH_CALLWNDPROC hook procedure before passing the message to the receiving window procedure,
        /// and calls the WH_CALLWNDPROCRET hook procedure after the window procedure has processed the message.
        /// </summary>
        WH_CALLWNDPROC = 4,
        /// <summary>
        /// The system calls a WH_CBT hook procedure before activating, creating, destroying, minimizing, maximizing, moving, or sizing a window;
        /// before completing a system command; before removing a mouse or keyboard event from the system message queue;
        /// before setting the input focus; or before synchronizing with the system message queue.
        /// The value the hook procedure returns determines whether the system allows or prevents one of these operations.
        /// The WH_CBT hook is intended primarily for computer-based training (CBT) applications.
        /// </summary>
        WH_CBT = 5,
        /// <summary>
        /// The WH_MSGFILTER and WH_SYSMSGFILTER hooks enable you to monitor messages about to be processed by a menu,
        /// scroll bar, message box, or dialog box, and to detect when a different window is about to be activated as a result of the user's
        /// pressing the ALT+TAB or ALT+ESC key combination.
        /// The WH_MSGFILTER hook can only monitor messages passed to a menu, scroll bar,
        /// message box, or dialog box created by the application that installed the hook procedure.
        /// The WH_SYSMSGFILTER hook monitors such messages for all applications.
        /// 
        /// The WH_MSGFILTER and WH_SYSMSGFILTER hooks enable you to perform message filtering during modal loops that is equivalent to the filtering
        /// done in the main message loop. For example, an application often examines a new message in the main loop between the time it
        /// retrieves the message from the queue and the time it dispatches the message,
        /// performing special processing as appropriate. However, during a modal loop, the system retrieves and dispatches messages without allowing
        /// an application the chance to filter the messages in its main message loop.
        /// If an application installs a WH_MSGFILTER or WH_SYSMSGFILTER hook procedure, the system calls the procedure during the modal loop.
        /// </summary>
        WH_SYSMSGFILTER = 6,
        /// <summary>
        /// The WH_MOUSE hook enables you to monitor mouse messages about to be returned by the GetMessage or PeekMessage function.
        /// You can use the WH_
        /// MOUSE hook to monitor mouse input posted to a message queue.
        /// </summary>
        WH_MOUSE = 7,
        /// <summary>
        /// Unknown....
        /// </summary>
        WH_HARDWARE = 8,
        /// <summary>
        /// The system calls a WH_DEBUG hook procedure before calling hook procedures associated with any other hook in the system.
        /// You can use this hook to determine whether to allow the system to call hook procedures associated with other types of hooks.
        /// </summary>
        WH_DEBUG = 9,
        /// <summary>
        /// A shell application can use the WH_SHELL hook to receive important notifications.
        /// The system calls a WH_SHELL hook procedure when the shell application is about to be activated and when a top-level window is created or destroyed.
        /// </summary>
        WH_SHELL = 10,
        /// <summary>
        /// The WH_FOREGROUNDIDLE hook enables you to perform low priority tasks during times when its foreground thread is idle.
        /// The system calls a WH_FOREGROUNDIDLE hook procedure when the application's foreground thread is about to become idle.
        /// </summary>
        WH_FOREGROUNDIDLE = 11,
        /// <summary>
        /// The WH_CALLWNDPROC and WH_CALLWNDPROCRET hooks enable you to monitor messages sent to window procedures.
        /// The system calls a WH_CALLWNDPROC hook procedure before passing the message to the receiving window procedure,
        /// and calls the WH_CALLWNDPROCRET hook procedure after the window procedure has processed the message.
        /// </summary>
        WH_CALLWNDPROCRET = 12,
        /// <summary>
        /// The WH_KEYBOARD_LL hook enables you to monitor keyboard input events about to be posted in a thread input queue.
        /// </summary>
        WH_KEYBOARD_LL = 13,
        /// <summary>
        /// The WH_MOUSE_LL hook enables you to monitor mouse input events about to be posted in a thread input queue.
        /// </summary>
        WH_MOUSE_LL = 14
    }

    /// <summary>
    /// Define the mouse actions constant which indicate the actions to performe.
    /// </summary>
    internal enum MouseHookAction : int
    {
        /// <summary>
        /// The wParam and lParam parameters of the hook procedure contain information about a mouse message.
        /// </summary>
        HC_ACTION = 0,
        /// <summary>
        /// The wParam and lParam parameters of the hook procedure contain information about a mouse message,
        /// and the mouse message has not been removed from the message queue.
        /// </summary>
        HC_NOREMOVE = 3
    }

    /// <summary>
    /// Define the mouse message constant.
    /// </summary>
    internal enum MouseMessage : uint
    {
        /// <summary>
        /// Mouse position changed.
        /// </summary>
        WM_MOUSEMOVE = 0x0200,
        /// <summary>
        /// Left button pressed.
        /// </summary>
        WM_LBUTTONDOWN = 0x0201,
        /// <summary>
        /// Left button released.
        /// </summary>
        WM_LBUTTONUP = 0x0202,
        /// <summary>
        /// Left button double click.
        /// </summary>
        WM_LBUTTONDBLCLK = 0x0203,
        /// <summary>
        /// Right button pressed
        /// </summary>
        WM_RBUTTONDOWN = 0x0204,
        /// <summary>
        /// Right button released.
        /// </summary>
        WM_RBUTTONUP = 0x0205,
        /// <summary>
        /// Right button double click.
        /// </summary>
        WM_RBUTTONDBLCLK = 0x0206,
        /// <summary>
        /// Middle button pressed.
        /// </summary>
        WM_MBUTTONDOWN = 0x0207,
        /// <summary>
        /// Middle button released.
        /// </summary>
        WM_MBUTTONUP = 0x0208,
        /// <summary>
        /// Middle button double click.
        /// </summary>
        WM_MBUTTONDBLCLK = 0x0209

    }


    /// <summary>
    /// Define the mouse event flags for event generation using user32.dll mouse_event method.
    /// </summary>
    [Flags]
    internal enum MouseEventFlags : uint
    {
        LEFTDOWN = 0x00000002,
        LEFTUP = 0x00000004,
        MIDDLEDOWN = 0x00000020,
        MIDDLEUP = 0x00000040,
        MOVE = 0x00000001,
        ABSOLUTE = 0x00008000,
        RIGHTDOWN = 0x00000008,
        RIGHTUP = 0x00000010
    } 

    /// <summary>
    /// Define a point structure with its coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PointStruct
    {
        /// <summary>
        /// X coordinate of the point.
        /// </summary>
        public int X;
        /// <summary>
        /// Y coordinate of the point.
        /// </summary>
        public int Y;

        public PointStruct( int x, int y )
		{
			X = x;
			Y = y;
		}
    }

    /// <summary>
    /// The MouseHookStruct structure contains information about a mouse event passed to a WH_MOUSE hook procedure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class MouseHookStruct
    {
        /// <summary>
        /// Specifies a POINT structure that contains the x- and y-coordinates of the cursor, in screen coordinates.
        /// </summary>
        public PointStruct pt;
        /// <summary>
        /// Handle to the window that will receive the mouse message corresponding to the mouse event.
        /// </summary>
        public int hwnd;
        /// <summary>
        /// Specifies the hit-test value. For a list of hit-test values,
        /// see the description of the WM_NCHITTEST message in MSDN.
        /// </summary>
        public int wHitTestCode;
        /// <summary>
        /// Specifies extra information associated with the message.
        /// </summary>
        public int dwExtraInfo;
    }

    /// <summary>
    /// The LLMouseHookStruct structure contains information about a low-level mouse input event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class LLMouseHookStruct
    {
        /// <summary>
        /// Specifies a POINT structure that contains the x- and y-coordinates of the cursor, in screen coordinates.
        /// </summary>
        public PointStruct pt;
        /// <summary>
        /// If the message is WM_MOUSEWHEEL, the high-order word of this member is the wheel delta. The low-order word is reserved.
        /// A positive value indicates that the wheel was rotated forward, away from the user;
        /// a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120.
        /// If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, or WM_NCXBUTTONDBLCLK,
        /// the high-order word specifies which X button was pressed or released, and the low-order word is reserved. Otherwise, mouseData is not used. 
        /// </summary>
        public int mouseData;
        /// <summary>
        /// Specifies the event-injected flag.
        /// </summary>
        public int flags;
        /// <summary>
        /// Specifies the time stamp for this message.
        /// </summary>
        public int time;
        /// <summary>
        /// Specifies extra information associated with the message.
        /// </summary>
        public IntPtr dwExtraInfo;
    }
}
