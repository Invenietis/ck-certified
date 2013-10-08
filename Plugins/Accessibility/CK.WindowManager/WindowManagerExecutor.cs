using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}", PublicName = "CK.WindowManager.Executor", Categories = new string[] { "Accessibility" }, Version = "1.0.0" )]
    public class WindowManagerExecutor : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IWindowBinder WindowBinder { get; set; }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;

            // Gets all windows attach to the given window
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerHolder );
            foreach( IWindowElement window in attachedElements )
            {
                window.Move( window.Top + e.DeltaTop, window.Left + e.DeltaLeft );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;

            // If the resized is horizontal
            // Resizes referential window.
            // Resizes all attached windows with position Left / Right
            // Moves all attached windows with position Top / Bottom

            // Gets all windows attach to the given window
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerHolder );
            foreach( IWindowElement window in attachedElements )
            {
                window.Resize( window.Width + e.DeltaWidth, window.Height + e.DeltaHeight );
            }
        }

        void WindowManager_WindowRestored( object sender, WindowElementEventArgs e )
        {
            // The Window that moves first
            IWindowElement triggerHolder = e.Window;
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerHolder );
            foreach( IWindowElement window in attachedElements )
            {
                window.Restore();
            }
        }

        void WindowManager_WindowHidden( object sender, WindowElementEventArgs e )
        {
            IWindowElement triggerHolder = e.Window;
            IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerHolder );
            foreach( IWindowElement window in attachedElements )
            {
                window.Hide();
            }
        }

        void WindowBinder_BeforeBinding( object sender, WindowBindingEventArgs e )
        {

        }

        void WindowBinder_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            if( e.BindingType == BindingEventType.Attach )
            {
                if( e.Binding.Position == BindingPosition.Top )
                {
                    double topSlave = e.Binding.Master.Top - e.Binding.Slave.Height;
                    double leftSlave = e.Binding.Master.Left;

                    e.Binding.Slave.Move( topSlave, leftSlave );
                    e.Binding.Slave.Resize( e.Binding.Master.Width, e.Binding.Slave.Height );
                }
            }
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
            WindowManager.WindowHidden += WindowManager_WindowHidden;
            WindowManager.WindowRestored += WindowManager_WindowRestored;

            WindowBinder.BeforeBinding += WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding += WindowBinder_AfterBinding;
        }

        public void Stop()
        {
            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowHidden -= WindowManager_WindowHidden;
            WindowManager.WindowRestored -= WindowManager_WindowRestored;

            WindowBinder.BeforeBinding -= WindowBinder_BeforeBinding;
            WindowBinder.AfterBinding -= WindowBinder_AfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
