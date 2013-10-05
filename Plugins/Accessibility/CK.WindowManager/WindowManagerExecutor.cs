using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" } )]
    public class WindowManagerExecutor : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerWindow = e.Window;

            // Gets all windows attach to the given window
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerWindow );
            foreach( IWindowElement window in attachedElements )
            {
                Action action = new Action( () => window.Move( window.Top + e.DeltaTop, window.Left + e.DeltaLeft ) );
                Dispatcher.CurrentDispatcher.BeginInvoke( action, DispatcherPriority.Render );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerWindow = e.Window;

            // Gets all windows attach to the given window
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerWindow );
            foreach( IWindowElement window in attachedElements )
            {
                Delegate action = new Action( () => window.Resize( window.Width + e.DeltaWidth, window.Height + e.DeltaHeight ) );
                Dispatcher.CurrentDispatcher.BeginInvoke( action, DispatcherPriority.Render );
            }
        }

        void WindowBinder_BeforeBinding( object sender, WindowBindingEventArgs e )
        {
        }

        void WindowBinder_AfterBinding( object sender, WindowBindedEventArgs e )
        {
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            WindowManager.WindowResized += OnWindowManagerWindowResized;
            WindowManager.WindowMoved += OnWindowManagerWindowMoved;
            
            WindowBinder.BeforeBinding += WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding += WindowBinder_AfterBinding;
        }

        public void Stop()
        {
            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;

            WindowBinder.BeforeBinding -= WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding -= WindowBinder_AfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
