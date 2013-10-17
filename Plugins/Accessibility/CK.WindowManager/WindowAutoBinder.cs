using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Plugin;
using CK.WindowManager.Model;
using CommonServices;
using System.Diagnostics;

namespace CK.WindowManager
{
    [Plugin( "{B63BB144-1C13-4A3B-93BD-AC5233F4F18E}", PublicName = "CK.WindowManager.AutoBinder" )]
    public class WindowAutoBinder : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IPointerDeviceDriver PointerDeviceDriver { get; set; }

        IDictionary<IWindowElement, Rect> _rect;
        bool _binding = false;

        public float AttractionRadius = 50;

        bool _intersectionDetected = false;
        bool _previewingBinding;
        IBindResult _bindResult;

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            // Do not act when a binding is occuring.
            if( _binding == true ) return;

            _intersectionDetected = false;

            ISpatialBinding binding = WindowBinder.Service.GetBinding( e.Window );
            if( binding == null ) return;

            IReadOnlyList<IWindowElement> registeredElements = WindowManager.Service.WindowElements;
            ICollection<IWindowElement> boundWindows = binding.AllDescendants().ToArray();

            Rect rect = _rect[e.Window] = new Rect( e.Window.Left, e.Window.Top, e.Window.Width, e.Window.Height );
            Rect enlargedRectangle = Rect.Inflate( rect, AttractionRadius, AttractionRadius );

            int i = 0;
            IWindowElement otherWindow = null;

            while( i < registeredElements.Count && _intersectionDetected == false )
            {
                otherWindow = registeredElements[i];
                // If in all registered windows a window intersect with the one that moved
                if( otherWindow != e.Window && !boundWindows.Contains( otherWindow ) )
                {
                    rect = _rect[otherWindow];
                    _intersectionDetected = rect.IntersectsWith( enlargedRectangle );
                }
                i++;
            }

            if( _intersectionDetected )
            {
                Debug.Assert( otherWindow != null );
                // Determines the intersection
                Rect overlapRectangle = Rect.Intersect( enlargedRectangle, rect );
                BindingPosition pos = BindingPosition.None;

                if( overlapRectangle.Bottom == enlargedRectangle.Bottom && overlapRectangle.Height != enlargedRectangle.Height ) pos = BindingPosition.Top;
                else if( overlapRectangle.Top == enlargedRectangle.Top && overlapRectangle.Height != enlargedRectangle.Height ) pos = BindingPosition.Bottom;
                else if( overlapRectangle.Right == enlargedRectangle.Right && overlapRectangle.Width != enlargedRectangle.Width ) pos = BindingPosition.Left;
                else if( overlapRectangle.Left == enlargedRectangle.Left && overlapRectangle.Width != enlargedRectangle.Width ) pos = BindingPosition.Right;

                _bindResult = WindowBinder.Service.PreviewBind( otherWindow, e.Window, pos );
            }
            else _bindResult = WindowBinder.Service.PreviewUnbind( otherWindow, e.Window );
        }

        void WindowBinder_PreviewBinding( object sender, WindowBindedEventArgs e )
        {
            _previewingBinding = e.BindingType == BindingEventType.Attach && _previewingBinding == false;
        }

        private void PointerDeviceDriver_PointerButtonUp( object sender, PointerDeviceEventArgs e )
        {
            if( _intersectionDetected && _bindResult != null )
            {
                _bindResult.Seal();
                _bindResult = null;
                _intersectionDetected = false;
            }
        }

        void Service_BeforeBinding( object sender, WindowBindingEventArgs e )
        {
            _binding = true;
        }

        void Service_AfterBinding( object sender, WindowBindedEventArgs e )
        {
            _binding = false;
        }

        void Service_Registered( object sender, WindowElementEventArgs e )
        {
            RegisterWindow( e.Window );
        }

        private void RegisterWindow( IWindowElement window )
        {
            if( !_rect.ContainsKey( window ) )
            {
                var rect = new Rect( window.Left, window.Top, window.Width, window.Height );
                _rect.Add( window, rect );
            }
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _rect = new Dictionary<IWindowElement, Rect>();
            return true;
        }

        public void Start()
        {
            WindowBinder.Service.PreviewBinding += WindowBinder_PreviewBinding;
            WindowBinder.Service.BeforeBinding += Service_BeforeBinding;
            WindowBinder.Service.AfterBinding += Service_AfterBinding;

            WindowManager.Service.Registered += Service_Registered;
            WindowManager.Service.WindowMoved += OnWindowMoved;

            PointerDeviceDriver.PointerButtonUp += PointerDeviceDriver_PointerButtonUp;

            foreach( IWindowElement e in WindowManager.Service.WindowElements ) RegisterWindow( e );
        }

        public void Stop()
        {
            PointerDeviceDriver.PointerButtonUp -= PointerDeviceDriver_PointerButtonUp;

            WindowBinder.Service.PreviewBinding -= WindowBinder_PreviewBinding;
            WindowBinder.Service.AfterBinding -= Service_AfterBinding;
            WindowBinder.Service.BeforeBinding -= Service_BeforeBinding;

            WindowManager.Service.Registered -= Service_Registered;
            WindowManager.Service.WindowMoved -= OnWindowMoved;
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
