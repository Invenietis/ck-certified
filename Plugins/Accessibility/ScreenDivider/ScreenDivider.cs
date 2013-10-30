using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using CommonServices.Accessibility;
using ScreenDivider.ViewModels;
using ScreenDivider.Views;
using ScreenDivider.Events;
using System.Windows.Controls;
using HighlightModel;
using KeyScroller;

namespace ScreenDivider
{
    [Plugin( ScreenDividerPlugin.PluginIdString,
           PublicName = ScreenDividerPlugin.PluginPublicName,
           Version = ScreenDividerPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScreenDividerPlugin : IPlugin, IActionableElement
    {
        const string PluginIdString = "{7942DA26-8181-4B32-9F94-03C4DB0D7915}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Screen Divider";

        IList<MainWindow> _attachedWindows = new List<MainWindow>();
        int _currentWindow = -1;
        int _loop = 0;
        bool _switchingActive = true;

        public IPluginConfigAccessor Config { get; set; }

        public MainWindow CurrentWindow { get { return _attachedWindows[_currentWindow]; } }

        public int MaxLoop { get { return _attachedWindows.Count * Config.User.GetOrSet( "ScrollingBeforeStop", 2 ); } }

        public bool PluginInPause { get { return _attachedWindows.All( a => ((WindowViewModel)a.DataContext).IsPause ); } }

        public bool IsEnter { get { return _attachedWindows.Any( a => ((WindowViewModel)a.DataContext).IsEnter ); } }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            foreach( Screen s in Screen.AllScreens.Reverse() )
                ConfigureScreen( s );

            ActionType = ActionType.StayOnTheSame;

            return true;
        }

        /// <summary>
        /// Configure a Window for a Screen
        /// </summary>
        /// <param name="screen">Screen where Window should be displayed</param>
        void ConfigureScreen( Screen screen )
        {
            MainWindow w = new MainWindow();
            w.Closed += ( o, e ) =>
            {
                ((MainWindow)o).IsClosed = true;
            };

            w.Left = screen.WorkingArea.Left;
            w.Top = screen.WorkingArea.Top;

            w.Show();
            w.WindowState = WindowState.Maximized;
            //w.Closed += WindowClosed;

            _attachedWindows.Add( w );
        }

        #region OnSelectedElement

        void EnterKeyUp()
        {
            MainWindow w = CurrentWindow;

            WindowViewModel wdc = (WindowViewModel)w.DataContext;
            if( !wdc.IsEnter )
            {
                if( ActionType == ActionType.StayOnTheSame )
                {
                    _attachedWindows.Where( a => a != w ).All( ( a ) =>
                    {
                        a.Hide();
                        return true;
                    }
                    );

                    CreateFirstGrid( w, wdc );
                    _loop = 0;
                }
                else
                {
                    PauseAllWindows();
                }
            }
            else
            {
                wdc.Enter();
            }

            ActionType = ActionType.StayOnTheSame;
            _switchingActive = true;
        }

        private void CreateFirstGrid( MainWindow w, WindowViewModel wdc, bool onlyOneMode = false )
        {
            Grid myGrid = w.MainWindowGrid;
            myGrid.Children.Add( wdc.Enter( onlyOneMode ) );
            wdc.GridOwned.ExitNode += ExitGridNode;
        }

        /// <summary>
        /// This method will be executed when a grid exited event is sent by a main grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ExitGridNode( object sender, ExitGridEventArgs e )
        {
            if( _attachedWindows.Count > 1 )
            {
                foreach( var w in _attachedWindows )
                {
                    if( !w.IsClosed )
                    {
                        w.Show();
                        Grid myGrid = w.MainWindowGrid;
                        myGrid.Children.Clear();
                    }
                }
                _attachedWindows[_currentWindow].Focus();
            }
            else
            {
                Debug.Assert( _attachedWindows.Count > 0 );
                WindowViewModel wdc = (WindowViewModel)_attachedWindows[0].DataContext;
                wdc.GridOwned.RestartSwitch();
            }
        }

        void SwitchWindow()
        {
            if( !_switchingActive ) return;
            if( _loop++ < MaxLoop )
            {
                if( !IsEnter )
                {
                    if( _currentWindow < _attachedWindows.Count - 1 ) _currentWindow++;
                    else _currentWindow = 0;

                    if( _currentWindow > 0 ) ((WindowViewModel)_attachedWindows[_currentWindow - 1].DataContext).IsActive = false;
                    else ((WindowViewModel)_attachedWindows[_attachedWindows.Count - 1].DataContext).IsActive = false;

                    ((WindowViewModel)_attachedWindows[_currentWindow].DataContext).IsActive = true;
                    _attachedWindows[_currentWindow].Focus();
                }
            }
            else
            {
                ActionType = ActionType.Normal;
                _switchingActive = false;
                _loop = 0;
                PauseAllWindows();
            }
        }

        #endregion

        #region Hightlight Methods

        void OnSelectElement( object sender, HighlightEventArgs e )
        {
            EnterKeyUp();
        }

        void OnBeginHighlight( object sender, HighlightEventArgs e )
        {
            if( !IsEnter )
                SwitchWindow();
        }

        void OnEndHighlight( object sender, HighlightEventArgs e )
        {
            // StayOnTheSame
        }

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterHighlighter();
            }
        }

        private void InitializeHighligther()
        {
            Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
        }

        private void UnInitializeHighlighter()
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                UnregisterHighlighter();
            }
            Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
        }

        private void UnregisterHighlighter()
        {
            Highlighter.Service.UnregisterTree( this );
            Highlighter.Service.BeginHighlight -= OnBeginHighlight;
            Highlighter.Service.EndHighlight -= OnEndHighlight;
            Highlighter.Service.SelectElement -= OnSelectElement;
        }

        private void RegisterHighlighter()
        {
            Highlighter.Service.RegisterTree( this );
            Highlighter.Service.BeginHighlight += OnBeginHighlight;
            Highlighter.Service.EndHighlight += OnEndHighlight;
            Highlighter.Service.SelectElement += OnSelectElement;
        }

        #endregion

        public void Start()
        {
            ActionType = ActionType.StayOnTheSame;
            if( _attachedWindows.Count == 1 )
            {
                MainWindow w = _attachedWindows[0];
                WindowViewModel wdc = (WindowViewModel)w.DataContext;
                CreateFirstGrid( w, wdc, true );
                _currentWindow = 0;
            }
            InitializeHighligther();
        }

        private void PauseAllWindows()
        {
            foreach( var w in _attachedWindows )
                ((WindowViewModel)w.DataContext).Pause();
        }

        public void Stop()
        {
            UnInitializeHighlighter();
        }

        public void Teardown()
        {
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return (int)CurrentWindow.Width; }
        }

        public int Height
        {
            get { return (int)CurrentWindow.Height; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        #endregion

        #region IActionableElement Members

        public ActionType ActionType { get; set; }

        #endregion
    }
}
