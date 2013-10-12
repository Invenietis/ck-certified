using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using CK.Plugin;
using CK.WindowManager.Model;

namespace CK.WindowManager
{
    [Plugin( "{B63BB144-1C13-4A3B-93BD-AC5233F4F18E}" )]
    public class WindowAutoBinder : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowManager> WindowManager { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IWindowBinder> WindowBinder { get; set; }

        IDictionary<IWindowElement, RectangleF> _rect;
        bool _binding = false;

        public float AttractionRadius = 50;

        void OnWindowMoved( object sender, WindowElementLocationEventArgs e )
        {
            // Do not act when a binding is occuring.
            if( _binding == true ) return;

            IReadOnlyList<IWindowElement> registeredElements = WindowManager.Service.WindowElements;
            ISpatialBinding binding = WindowBinder.Service.GetBinding( e.Window );

            ICollection<IWindowElement> boundWindows = binding.AllDescendants().ToArray();

            RectangleF rect = _rect[e.Window] = new RectangleF( (float)e.Window.Left, (float)e.Window.Top, (float)e.Window.Width, (float)e.Window.Height );
            RectangleF enlargedRectangle = RectangleF.Inflate( rect, AttractionRadius, AttractionRadius );

            int i = 0;
            IWindowElement otherWindow = null;
            bool intersectionDetected = false;

            while( i < registeredElements.Count && intersectionDetected == false )
            {
                otherWindow = registeredElements[i];
                // If in all registered windows a window intersect with the one that moved
                if( otherWindow != e.Window && !boundWindows.Contains( otherWindow ) )
                {
                    rect = _rect[otherWindow];
                    intersectionDetected = rect.IntersectsWith( enlargedRectangle );
                }
                i++;
            }

            if( intersectionDetected )
            {
                // Determines the intersection
                RectangleF overlapRectangle = RectangleF.Intersect( enlargedRectangle, rect );
                BindingPosition pos = BindingPosition.None;

                if( overlapRectangle.Bottom == enlargedRectangle.Bottom ) pos = BindingPosition.Top;
                else if( overlapRectangle.Top == enlargedRectangle.Top ) pos = BindingPosition.Bottom;
                else if( overlapRectangle.Right == enlargedRectangle.Right ) pos = BindingPosition.Left;
                else if( overlapRectangle.Left == enlargedRectangle.Left ) pos = BindingPosition.Right;

                if( pos != BindingPosition.None )
                    WindowBinder.Service.Attach( otherWindow, e.Window, pos );
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
            var window = e.Window;
            RegisterWindow( window );
        }

        private void RegisterWindow( IWindowElement window )
        {
            var rect = new RectangleF( (float)window.Left, (float)window.Top, (float)window.Width, (float)window.Height );
            _rect.Add( window, rect );
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _rect = new Dictionary<IWindowElement, RectangleF>();
            return true;
        }

        public void Start()
        {
            WindowBinder.Service.BeforeBinding += Service_BeforeBinding;
            WindowBinder.Service.AfterBinding += Service_AfterBinding;

            WindowManager.Service.Registered += Service_Registered;
            WindowManager.Service.WindowMoved += OnWindowMoved;

            foreach( IWindowElement e in WindowManager.Service.WindowElements ) RegisterWindow( e );
        }

        public void Stop()
        {
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
