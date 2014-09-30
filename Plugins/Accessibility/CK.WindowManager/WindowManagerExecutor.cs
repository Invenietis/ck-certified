#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\CK.WindowManager\WindowManagerExecutor.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin;
using CK.WindowManager.Model;
using CK.Windows;

namespace CK.WindowManager
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Accessibility" } )]
    public class WindowManagerExecutor : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{B91D6A8D-2294-4BAA-AD31-AC1F296D82C4}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "CK.WindowManager.Executor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        PreviewBindingInfo _placeholder;
        ActivityMonitor _logger;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowManager WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWindowBinder WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITopMostService TopMostService { get; set; }

        public WindowManagerExecutor()
        {
            _placeholder = new PreviewBindingInfo();
            _logger = new ActivityMonitor();
            //_logger.Tap.Register( new ActivityLoggerConsoleSink() );
        }

        void OnWindowManagerWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );
            IWindowElement triggerHolder = e.Window;
            // Gets all windows attached to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );

            if( binding != null )
            {
                //temporary
                ResizingWindow( binding );
                PlacingWindow( binding );
            }
        }

        void OnWindowManagerWindowResized( object sender, WindowElementResizeEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            IWindowElement triggerHolder = e.Window;
            // Gets all windows attached to the given window
            ISpatialBinding binding = WindowBinder.GetBinding( triggerHolder );
            if( binding != null )
            {
                if(  e.DeltaHeight != 0 || e.DeltaWidth != 0 )
                {
                    ResizingWindow( binding );
                    PlacingWindow( binding );
                }
            }
        }

        void PlacingWindow( ISpatialBinding binding, BindingPosition masterPosition = BindingPosition.None )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            IWindowElement reference = binding.Window;
            IWindowElement slave = null;

            if( masterPosition != BindingPosition.Top && binding.Top != null )
            {
                slave = binding.Top.Window;
                WindowManager.Move( binding.Top.Window, reference.Top - slave.Height, reference.Left );
                PlacingWindow( binding.Top, BindingPosition.Bottom );
            }
            if( masterPosition != BindingPosition.Bottom && binding.Bottom != null )
            {
                slave = binding.Bottom.Window;
                WindowManager.Move( binding.Bottom.Window, reference.Top + reference.Height, reference.Left );
                PlacingWindow( binding.Bottom, BindingPosition.Top );
            }
            if( masterPosition != BindingPosition.Left && binding.Left != null )
            {
                slave = binding.Left.Window;
                WindowManager.Move( binding.Left.Window, reference.Top, reference.Left - slave.Width );
                PlacingWindow( binding.Left, BindingPosition.Right );
            }
            if( masterPosition != BindingPosition.Right && binding.Right != null )
            {
                slave = binding.Right.Window;
                WindowManager.Move( binding.Right.Window, reference.Top, reference.Left + reference.Width );
                PlacingWindow( binding.Right, BindingPosition.Left );
            }
        }

        void ResizingWindow( ISpatialBinding binding, BindingPosition masterPosition = BindingPosition.None )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            IWindowElement reference = binding.Window;
            IWindowElement slave = null;

            if( masterPosition != BindingPosition.Top && binding.Top != null )
            {
                slave = binding.Top.Window;
                if( reference.Width != slave.Width )
                {
                    WindowManager.Resize( slave, reference.Width, slave.Height );
                    ResizingWindow( binding.Top, BindingPosition.Bottom );
                }
                WindowManager.Move( slave, reference.Top - slave.Height, reference.Left );
            }
            if( masterPosition != BindingPosition.Bottom && binding.Bottom != null )
            {
                slave = binding.Bottom.Window;
                if( reference.Width != slave.Width )
                {
                    WindowManager.Resize( slave, reference.Width, slave.Height );
                    ResizingWindow( binding.Bottom, BindingPosition.Top );
                }
                WindowManager.Move( slave, reference.Top + reference.Height, reference.Left );
            }
            if( masterPosition != BindingPosition.Left && binding.Left != null )
            {
                slave = binding.Left.Window;
                if( reference.Height != slave.Height )
                {
                    WindowManager.Resize( slave, slave.Width, reference.Height );
                    ResizingWindow( binding.Left, BindingPosition.Right );
                }
                WindowManager.Move( slave, reference.Top, reference.Left - slave.Width );
            }
            if( masterPosition != BindingPosition.Right && binding.Right != null )
            {
                slave = binding.Right.Window;
                if( reference.Height != slave.Height )
                {
                    WindowManager.Resize( slave, slave.Width, reference.Height );
                    ResizingWindow( binding.Right, BindingPosition.Left );
                }
                WindowManager.Move( slave, reference.Top, reference.Left + reference.Width );
            }
        }

        void OnPreviewBinding( object sender, WindowBindedEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == NoFocusManager.Default.ExternalDispatcher, "This method should only be called by the ExternalThread." );

            if( e.BindingType == BindingEventType.Attach )
            {
                if( !_placeholder.IsPreviewOf( e.Binding ) ) _placeholder.Display( e.Binding, TopMostService );
            }
            else _placeholder.Shutdown( TopMostService );
        }

        void OnBeforeBinding( object sender, WindowBindingEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the Application Thread." );

            if( e.BindingType == BindingEventType.Attach )
            {
                Rect r = e.Binding.GetWindowArea();
                if( r != Rect.Empty )
                {
                    //Console.WriteLine( "OnBeforeBinding ! Origin : {0}", e.Binding.Origin.Name );
                    var move = WindowManager.Move( e.Binding.Origin, r.Top, r.Left );
                    var resize = WindowManager.Resize( e.Binding.Origin, r.Width, r.Height );
                    move.Broadcast();
                    resize.Broadcast();
                }
            }
        }

        void OnAfterBinding( object sender, WindowBindedEventArgs e )
        {
            if(e.BindingType == BindingEventType.Attach)
            {
                _placeholder.Shutdown( TopMostService );
            }
        }

        void OnWindowRestored( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                // The Window that moves first
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Restore();
                }
            }
        }

        void OnWindowMinimized( object sender, WindowElementEventArgs e )
        {
            Debug.Assert( Dispatcher.CurrentDispatcher == Application.Current.Dispatcher, "This method should only be called by the ExternalThread." );

            ISpatialBinding binding = WindowBinder.GetBinding( e.Window );
            if( binding != null )
            {
                foreach( ISpatialBinding descendant in binding.AllDescendants() )
                {
                    descendant.Window.Minimize();
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
            WindowManager.WindowMinimized += OnWindowMinimized;
            WindowManager.WindowRestored += OnWindowRestored;

            WindowBinder.PreviewBinding += OnPreviewBinding;
            WindowBinder.BeforeBinding += OnBeforeBinding;
            WindowBinder.AfterBinding += OnAfterBinding;
        }

        public void Stop()
        {

            WindowManager.WindowResized -= OnWindowManagerWindowResized;
            WindowManager.WindowMoved -= OnWindowManagerWindowMoved;
            WindowManager.WindowMinimized -= OnWindowMinimized;
            WindowManager.WindowRestored -= OnWindowRestored;

            WindowBinder.PreviewBinding -= OnPreviewBinding;
            WindowBinder.BeforeBinding -= OnBeforeBinding;
            WindowBinder.AfterBinding -= OnAfterBinding;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
