using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CK_WindowManager
{
    public class WindowsInformations : IWindowsInformations,INotifyPropertyChanged
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        public static class HWND
        {
            public static IntPtr
            NoTopMost = new IntPtr(-2),
            TopMost = new IntPtr(-1),
            Top = new IntPtr(0),
            Bottom = new IntPtr(1);
        }

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        static uint WM_CLOSE = 0x10;
        private int _x;
        private int _y;
        private int _z;
        private int _height;
        private int _width;
        private IntPtr _handle;
        private string _title;
        private IScreenInformations _primaryScreen;

        public WindowsInformations(int x,int y,int z, int width,int height, String title,IScreenInformations primaryScreen,IntPtr handle)
        {
            _x= x;
            _y = y;
            _z= z;
            _width = width;
            _height = height;
            _title = title;
            _primaryScreen=primaryScreen;
            _handle = handle;
        }

        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_x != value)
                {
                    _x = value; 
                    NotifyPropertyChanged("X");
                }
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (_y != value)
                {
                    _y = value;
                    NotifyPropertyChanged("Y");
                }
            }
        }
        public int Z
        {
            get
            {
                return _z;
            }
            set
            {
                if (_z != value)
                {
                    _z = value;
                    NotifyPropertyChanged("Z");
                }
            }
        }
        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    NotifyPropertyChanged("Width");
                }
            }
        }
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    NotifyPropertyChanged("Height");
                }
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        public IScreenInformations ScreenInfo
        {
            get
            {
                return _primaryScreen;
            }
            set
            {
                if (_primaryScreen != value)
                {
                    _primaryScreen = value;
                    NotifyPropertyChanged("PrimaryScreen");
                }
            }
        }
        public Rectangle Rect
        {
            get
            {
                return new Rectangle(_x,_y,_width,_height);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public IntPtr Handle
        {
            get { return _handle; }
        }
        
        public override String ToString()
        {
            return " Title=" + _title+" X =" + _x + " Y=" + _y + " Z=" + _z + " Width=" + _width + " Height=" + _height+ " PrimaryScreen=" + _primaryScreen;
        }

        public void Move(int x, int y)
        {
            MoveWindow(_handle, x, y, _width, _height, true);
        }

        public void Close()
        {
            SendMessage(_handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
