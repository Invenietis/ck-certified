#region LGPL License
/*----------------------------------------------------------------------------
* This file (CiviKey\Config\Win32Utils.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CK.Config.UI
{
	/// <summary>Contains all messages systems and all other methods useful for the Skin</summary>
	public static class Win32
	{
		/// <summary>
		/// Systems messages to catch in WndProc function
		/// </summary>
		/// <summary>Command system message</summary>
		public const int WM_SYSCOMMAND = 0x112;
		public const int WM_NCHITTEST = 0x84;
		public const int HTCLIENT = 1;
		public const int HTCAPTION = 2;
		public const int WS_CLIPCHILDREN = 0x02000000;
		public const int WS_CAPTION = 0xc00000;
		public const int WS_BORDER = 0x800000;
		public const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;
		public const int WS_EX_NOACTIVATE = 134217728;
		public const int HWND_TOPMOST = -1; // 0xffff 
		public const int WM_SIZING = 0x0214;
		public const int WM_CAPTURECHANGED = 0x215;
		public const int WM_MOVING = 0x0216;
		public const int WMSZ_LEFT = 1;
		public const int WMSZ_RIGHT = 2;
		public const int WMSZ_TOP = 3;
		public const int WMSZ_TOPLEFT = 4;
		public const int WMSZ_TOPRIGHT = 5;
		public const int WMSZ_BOTTOM = 6;
		public const int WMSZ_BOTTOMLEFT = 7;
		public const int WMSZ_BOTTOMRIGHT = 8;
		public const int SWP_NOSIZE = 1; // 0x0001 
		public const int SWP_NOMOVE = 2; // 0x0002 
		public const int SWP_NOZORDER = 4; // 0x0004 
		public const int SWP_NOACTIVATE = 16; // 0x0010 
		public const int SWP_SHOWWINDOW = 64; // 0x0040 
		public const int SWP_HIDEWINDOW = 128; // 0x0080 
		public const int SWP_DRAWFRAME = 32; // 0x0020 
		public const int HTLEFT = 10;
		public const int HTRIGHT = 11;
		public const int HTTOP = 12;
		public const int HTTOPLEFT = 13;
		public const int HTTOPRIGHT = 14;
		public const int HTBOTTOM = 15;
		public const int HTBOTTOMLEFT = 16;
		public const int HTBOTTOMRIGHT = 17;

		/// <summary>
		/// Helper structure used to retrieve the location of the form to
		/// move its despite of its chronic inactivity !
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		/// <summary>Sends a message to a window</summary>
		[DllImport( "User32.dll", EntryPoint="SendMessageA" )]
		public static extern int SendMessage( IntPtr hwnd, int wMsg, int wParam, bool lParam );

		/// <summary>Sets the normal location of the Mouse</summary>
		[DllImport( "User32.dll", EntryPoint="ReleaseCapture" )]
		public static extern bool ReleaseCapture();


		public enum Bool
		{
			False=0,
			True
		};


		[StructLayout( LayoutKind.Sequential )]
		public struct Point
		{
			public Int32 x;
			public Int32 y;

			public Point( Int32 x, Int32 y ) { this.x= x; this.y= y; }
		}


		[StructLayout( LayoutKind.Sequential )]
		public struct Size
		{
			public Int32 cx;
			public Int32 cy;

			public Size( Int32 cx, Int32 cy ) { this.cx= cx; this.cy= cy; }
		}


		[StructLayout( LayoutKind.Sequential, Pack=1 )]
		struct ARGB
		{
			public byte Blue;
			public byte Green;
			public byte Red;
			public byte Alpha;
		}


		[StructLayout( LayoutKind.Sequential, Pack=1 )]
		public struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}


		public const Int32 ULW_COLORKEY = 0x00000001;
		public const Int32 ULW_ALPHA    = 0x00000002;
		public const Int32 ULW_OPAQUE   = 0x00000004;

		public const byte AC_SRC_OVER  = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;


		[DllImport( "user32.dll", ExactSpelling=true, SetLastError=true )]
		public static extern Bool UpdateLayeredWindow( IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags );

		[DllImport( "user32.dll", ExactSpelling=true, SetLastError=true )]
		public static extern IntPtr GetDC( IntPtr hWnd );

		[DllImport( "user32.dll", ExactSpelling=true )]
		public static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );

		[DllImport( "gdi32.dll", ExactSpelling=true, SetLastError=true )]
		public static extern IntPtr CreateCompatibleDC( IntPtr hDC );

		[DllImport( "gdi32.dll", ExactSpelling=true, SetLastError=true )]
		public static extern Bool DeleteDC( IntPtr hdc );

		[DllImport( "gdi32.dll", ExactSpelling=true )]
		public static extern IntPtr SelectObject( IntPtr hDC, IntPtr hObject );

		[DllImport( "gdi32.dll", ExactSpelling=true, SetLastError=true )]
		public static extern Bool DeleteObject( IntPtr hObject );
	}
}
