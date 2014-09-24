using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using CK_WindowManager;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Controls;

namespace CK_WindowViewer
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin(PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion)]
    public class CK_WindowViewer : IPlugin
    {
        //This GUID has been generated when you created the project, you may use it as is
        const string PluginGuidString = "{72ccde47-d534-490c-be9e-58de0a945407}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "CK-WindowViewer";
        const int coeff = 3;
        const double opacity = 0.75;
        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public static IWindowManager windowManager { get; set; }
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }
        Viewer v;
        Color[] colorsTab = new Color[255];
        Random random = new Random();

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
            v = new Viewer(windowManager.ScreenList);
            v.Show();
            fillTabColor();
            v.Width = windowManager.My_WorkingAreaWidth / coeff;
            v.Height = windowManager.My_WorkingAreaHeight / coeff;

            v.Left = Screen.PrimaryScreen.WorkingArea.Width - v.Width;
            v.Top = Screen.PrimaryScreen.WorkingArea.Height - v.Height;
            v.Topmost = true;
            v.ResizeMode = System.Windows.ResizeMode.NoResize;
            INotifyCollectionChanged z = windowManager.WindowList;
            z.CollectionChanged += new NotifyCollectionChangedEventHandler(windowListener);
            for (int i = 0; i < windowManager.WindowList.Count; i++)
                windowManager.WindowList[i].PropertyChanged += new PropertyChangedEventHandler(parametersListener);
            initialize();
        }
        public void initialize()
        {
            
                v.Dispatcher.Invoke(new Action(delegate()
                {
                    for (int i = 0; i < windowManager.WindowList.Count; i++)
                    {
                        IWindowsInformations iWindows = windowManager.WindowList[i];
                        if (iWindows.X > -10000 && iWindows.Title != "" && iWindows.Title != "Program Manager" && iWindows.Title != "Démarrer" && iWindows.Title != "MainWindow")
                        {
                            if (windowManager.ScreenList.Count == 1)
                            {
                                v.CreateRect(iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, iWindows.X / coeff, iWindows.Y / coeff, opacity, new SolidColorBrush(colorsTab[i]));
                            }
                            else if (windowManager.ScreenList.Count == 2)
                            {
                                int y = iWindows.Y / coeff;
                                int x = iWindows.X / coeff;
                                for (int j = 0; j < windowManager.ScreenList.Count; j++)
                                {

                                    if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.Y < 0)
                                    {
                                        y = (iWindows.Y + Math.Abs(windowManager.ScreenList[j].Bounds.Y)) / coeff;
                                    }
                                    if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.X < 0)
                                    {
                                        x = (iWindows.X + Math.Abs(windowManager.ScreenList[j].Bounds.X)) / coeff;
                                    }
                                }
                                v.CreateRect(iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, x, y, opacity, new SolidColorBrush(colorsTab[i]));
                            }
                        }
                    }
                }));

            
        }
        public void fillTabColor()
        {
            Random randonGen = new Random();
            for (int i = 0; i < colorsTab.Length; i++)
            {
                colorsTab[i] = Color.FromRgb((byte)randonGen.Next(255), (byte)randonGen.Next(255), (byte)randonGen.Next(255));
            }
        }
        /// <summary>
        /// First method called when the plugin is stopping
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        { }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        { }

        public void windowListener(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                IWindowsInformations newWindow = windowManager.WindowList[e.NewStartingIndex];
                newWindow.PropertyChanged += new PropertyChangedEventHandler(parametersListener);
                v.Dispatcher.Invoke(new Action(delegate()
                {

                    if (newWindow.X > -10000 && newWindow.Title != "" && newWindow.Title != "Program Manager" && newWindow.Title != "Démarrer" && newWindow.Title != "MainWindow")
                    {
                        if (windowManager.ScreenList.Count == 1)
                        {
                            v.CreateRect(newWindow, windowManager.WindowList.Count, newWindow.Z, newWindow.Height / coeff, newWindow.Width / coeff, newWindow.X / coeff, newWindow.Y / coeff, opacity, new SolidColorBrush(colorsTab[e.NewStartingIndex]));
                        }
                        else if (windowManager.ScreenList.Count == 2)
                        {
                            int y = newWindow.Y / coeff;
                            int x = newWindow.X / coeff;
                            for (int i = 0; i < windowManager.ScreenList.Count; i++)
                            {

                                if (!windowManager.ScreenList[i].IsPrimaryScreen && windowManager.ScreenList[i].Bounds.Y < 0)
                                {
                                    y = (newWindow.Y + Math.Abs(windowManager.ScreenList[i].Bounds.Y)) / coeff;
                                }
                                if (!windowManager.ScreenList[i].IsPrimaryScreen && windowManager.ScreenList[i].Bounds.X < 0)
                                {
                                    x = (newWindow.X + Math.Abs(windowManager.ScreenList[i].Bounds.X)) / coeff;
                                }
                            }
                            v.CreateRect(newWindow, windowManager.WindowList.Count, newWindow.Z, newWindow.Height / coeff, newWindow.Width / coeff, x, y, opacity, new SolidColorBrush(colorsTab[e.NewStartingIndex]));
                        }

                    }

                }));
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                v.Dispatcher.Invoke(new Action(delegate()
                {
                    v.resetCanvas();
                }));

                v.Dispatcher.Invoke(new Action(delegate()
                {
                    for (int i = 0; i < windowManager.WindowList.Count; i++)
                    {
                        IWindowsInformations iWindows = windowManager.WindowList[i];
                        if (iWindows.X > -10000 && iWindows.Title != "" && iWindows.Title != "Program Manager" && iWindows.Title != "Démarrer" && iWindows.Title != "MainWindow")
                        {
                            if (windowManager.ScreenList.Count == 1)
                            {
                                v.CreateRect(iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, iWindows.X / coeff, iWindows.Y / coeff, opacity, new SolidColorBrush(colorsTab[i]));
                            }
                            else if (windowManager.ScreenList.Count == 2)
                            {
                                int y = iWindows.Y / coeff;
                                int x = iWindows.X / coeff;
                                for (int j = 0; j < windowManager.ScreenList.Count; j++)
                                {

                                    if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.Y < 0)
                                    {
                                        y = (iWindows.Y + Math.Abs(windowManager.ScreenList[j].Bounds.Y)) / coeff;
                                    }
                                    if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.X < 0)
                                    {
                                        x = (iWindows.X + Math.Abs(windowManager.ScreenList[j].Bounds.X)) / coeff;
                                    }
                                }
                                v.CreateRect(iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, x, y, opacity, new SolidColorBrush(colorsTab[i]));
                            }
                        }
                    }
                }));
            }
        }
        public void parametersListener(Object sender, PropertyChangedEventArgs e)
        {
            v.Dispatcher.Invoke(new Action(delegate()
            {
                for (int i = 0; i < v.getCanvas().Children.Count; i++)
                {
                    if ((IWindowsInformations)sender == ((CanvasModel)v.getCanvas().Children[i]).getWindowsInfo())
                    {
                        CanvasModel myCanvas = (CanvasModel)v.getCanvas().Children[i];
                        IWindowsInformations iWindows = (IWindowsInformations)sender;
                        if (windowManager.ScreenList.Count == 1)
                        {
                            v.updateCanvasModel(myCanvas, iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, iWindows.X / coeff, iWindows.Y / coeff);
                        }
                        else if (windowManager.ScreenList.Count == 2)
                        {
                            int y = iWindows.Y / coeff;
                            int x = iWindows.X / coeff;
                            for (int j = 0; j < windowManager.ScreenList.Count; j++)
                            {

                                if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.Y < 0)
                                {
                                    y = (iWindows.Y + Math.Abs(windowManager.ScreenList[j].Bounds.Y)) / coeff;
                                }
                                if (!windowManager.ScreenList[j].IsPrimaryScreen && windowManager.ScreenList[j].Bounds.X < 0)
                                {
                                    x = (iWindows.X + Math.Abs(windowManager.ScreenList[j].Bounds.X)) / coeff;
                                }
                            }
                            v.updateCanvasModel(myCanvas, iWindows, windowManager.WindowList.Count, iWindows.Z, iWindows.Height / coeff, iWindows.Width / coeff, x, y);
                        }

                    }
                }
            }));
        }
    }
}
