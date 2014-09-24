using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using System.Collections.ObjectModel;
using System.Timers;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;
namespace CK_WindowManager
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin(PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion)]
    public class CK_WindowManager : IPlugin,IWindowManager
    {
        //This GUID has been generated when you created the project, you may use it as is
        const string PluginGuidString = "{ca491321-28c9-43ae-92d0-dda7a7d61da3}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "CK-WindowManager";

        //DllImport 
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);
        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }

        //Private attributes
        private ObservableCollection<IWindowsInformations> _windowList;
        private ObservableCollection<IScreenInformations> _screenList;
        private DispatcherTimer _timer;
        ReadOnlyObservableCollection<IWindowsInformations> _readOnlywindowList;
        ReadOnlyObservableCollection<IScreenInformations> _readOnlyscreenList;
        public ReadOnlyObservableCollection<IWindowsInformations> WindowList { get { return _readOnlywindowList; } }
        public ReadOnlyObservableCollection<IScreenInformations> ScreenList { get { return _readOnlyscreenList; } }
        

        
        public int My_WorkingAreaWidth
        { 
            get
            {
                int xMax = 0;
                int xMin = 0;
                for(int i=0;i<_screenList.Count;i++)
                {
                    if (xMax < (_screenList[i].Bounds.X + _screenList[i].Bounds.Width))
                        xMax = _screenList[i].Bounds.X + _screenList[i].Bounds.Width;
                    if (xMin > _screenList[i].Bounds.X)
                        xMin = _screenList[i].Bounds.X;
                }
                return xMax-xMin;
            } 
        }
        
        public int My_WorkingAreaHeight
        {
            get
            {
                int yMax = 0;
                int yMin = 0;
                for (int i = 0; i < _screenList.Count; i++)
                {
                    if (yMax < (_screenList[i].Bounds.Y + _screenList[i].Bounds.Height))
                        yMax = _screenList[i].Bounds.Y+_screenList[i].Bounds.Height;
                    if (yMin > _screenList[i].Bounds.Y)
                        yMin = _screenList[i].Bounds.Y;
                }
                return yMax - yMin;
            } 
        }

        
        /// <summary>
        /// First called method on the class, at this point, all services are null.
        /// Used to set up the service exposed by this plugin (if any).
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Setup(IPluginSetupInfo info)
        {
            return true;
        }

        /// <summary>
        /// Called after the Setup method.
        /// All launched services are now set, you may now set up the link to the service used by this class
        /// </summary>
        public void Start()
        {
            _windowList = new ObservableCollection<IWindowsInformations>();
            _screenList= new ObservableCollection<IScreenInformations>();
            _readOnlywindowList = new ReadOnlyObservableCollection<IWindowsInformations>(_windowList);
            _readOnlyscreenList = new ReadOnlyObservableCollection<IScreenInformations>(_screenList);
            initialize();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0,0,0);  
            _timer.Tick += new EventHandler(OnTimedEvent);
            _timer.Start();
              
        }
        
        private void initialize()
        {
            foreach (var screen in Screen.AllScreens)
            {
                _screenList.Add(new ScreenInformations(screen.Bounds, screen.WorkingArea, screen.Primary, screen.DeviceName));
            }

            int z = 0;
            IntPtr hwnd = GetWindow(GetDesktopWindow(), GetWindow_Cmd.GW_CHILD);
            do
            {
                if (IsWindowVisible(hwnd))
                {
                    RECT rRect;
                    if (GetWindowRect(hwnd, out rRect))
                    {
                        Rectangle myRect = new Rectangle();
                        myRect.X = rRect.Left;
                        myRect.Y = rRect.Top;
                        myRect.Width = rRect.Right - rRect.Left;
                        myRect.Height = rRect.Bottom - rRect.Top;
                        StringBuilder sb = new StringBuilder(GetWindowTextLength(hwnd) + 1);
                        GetWindowText(hwnd,sb,sb.Capacity);
                        IScreenInformations text=null;
                        for (int i = 0; i < _screenList.Count; i++)
                        {
                            if(_screenList[i].Bounds.Contains(new Rectangle(myRect.X+8,myRect.Y+8,0,0)))
                                text=_screenList[i];
                        }
                        WindowsInformations wI = new WindowsInformations(myRect.X, myRect.Y, z, myRect.Width, myRect.Height, sb.ToString(), text,hwnd);
                        _windowList.Add(wI);
                        z++;
                    }

                }
                hwnd = GetWindow(hwnd, GetWindow_Cmd.GW_HWNDNEXT);
            }
            while (hwnd != null && hwnd != IntPtr.Zero);
        }
        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _timer = null;
        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        {
        }

        public void OnTimedEvent(object source, EventArgs e)
        {
            IEnumerator<IWindowsInformations> enumList = _windowList.GetEnumerator();
            bool find = true;      
            List<IWindowsInformations> list=new List<IWindowsInformations>();
            while (enumList.MoveNext() && find)
            {
                IntPtr currentElement = ((WindowsInformations)enumList.Current).Handle;
                if (!IsWindowVisible(currentElement))
                {
                    find = false;
                    list.Add(enumList.Current);
                }
            }
            enumList.Dispose();
                for (int i = 0; i < list.Count; i++)
                    _windowList.Remove(list[i]);
                
            int z = 0;
            IntPtr hwnd = GetWindow(GetDesktopWindow(), GetWindow_Cmd.GW_CHILD);
            do
            {
                if (IsWindowVisible(hwnd))
                {
                    RECT rRect;
                    if (GetWindowRect(hwnd, out rRect))
                    {
                        bool test=false;
                        int i=0;
                        while (i<_windowList.Count && !test)
                        {
                            if (((WindowsInformations)_windowList[i]).Handle == hwnd)
                            {
                                test=true;
                                Rectangle myRect = new Rectangle();
                                myRect.X = rRect.Left;
                                myRect.Y = rRect.Top;
                                myRect.Width = rRect.Right - rRect.Left;
                                myRect.Height = rRect.Bottom - rRect.Top;
                                StringBuilder sb = new StringBuilder(GetWindowTextLength(hwnd)+1);
                                GetWindowText(hwnd, sb, sb.Capacity);
                                _windowList[i].X = myRect.X;
                                _windowList[i].Y = myRect.Y;
                                _windowList[i].Width = myRect.Width;
                                _windowList[i].Height = myRect.Height;
                                _windowList[i].Z = z;
                                IScreenInformations text =_screenList[0];
                                for (int j = 0; j < _screenList.Count; j++)
                                {
                                    if (_screenList[j].Bounds.Contains(new Rectangle(myRect.X+8, myRect.Y+8, 0, 0)))
                                        text = _screenList[j];
                                }
                                _windowList[i].ScreenInfo = text;
                            }
                            i++;    
                        }
                        if (!test)
                        {
                            Rectangle myRect = new Rectangle();
                            myRect.X = rRect.Left;
                            myRect.Y = rRect.Top;
                            myRect.Width = rRect.Right - rRect.Left;
                            myRect.Height = rRect.Bottom - rRect.Top;
                            StringBuilder sb = new StringBuilder(GetWindowTextLength(hwnd)+1);
                            GetWindowText(hwnd, sb, sb.Capacity);
                            IScreenInformations text = null;
                            for (int j = 0; j < _screenList.Count; j++)
                            {
                                if (_screenList[j].Bounds.Contains(new Rectangle(myRect.X+8, myRect.Y+8, 0, 0)))
                                    text = _screenList[j];
                            }
                            WindowsInformations wi = new WindowsInformations(myRect.X, myRect.Y, z, myRect.Width, myRect.Height, sb.ToString(), text, hwnd);
                            _windowList.Add(wi);
                        }
                    }
                    z++;
                }
                hwnd = GetWindow(hwnd, GetWindow_Cmd.GW_HWNDNEXT);

            }
            while (hwnd != null && hwnd != IntPtr.Zero);
            foreach (var screen in Screen.AllScreens)
            {
                bool test=false;
                IEnumerator<IScreenInformations> screens = _screenList.GetEnumerator();
                while (screens.MoveNext() && !test)
                {
                    if (screens.Current.DeviceName == screen.DeviceName)
                    {
                        test = true;
                        screens.Current.IsPrimaryScreen = screen.Primary;
                        screens.Current.WorkingArea = screen.WorkingArea;
                        screens.Current.Bounds = screen.Bounds;
                    }
                }
                if (!test)
                    _screenList.Add(new ScreenInformations(screen.Bounds, screen.WorkingArea, screen.Primary, screen.DeviceName));
            }

        }        
    }
}
