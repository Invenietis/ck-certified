using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using CK.WindowManager.Model;
using System.Windows;
using System.Windows.Threading;

namespace CK.Certified.WindowManager.Test
{
    [TestFixture]
    public class WindowManagerTest
    {
        [Test]
        public void WindowManagerApiTest()
        {
            IWindowManager windowManager = new WindowManager();
            WindowElement A = new WindowElement( "A", new Window() );
            WindowElement B = new WindowElement( "B", new Window() );
            WindowElement C = new WindowElement( "C", new Window() );
            WindowElement D = new WindowElement( "D", new Window() );

            A.WindowManager = B.WindowManager = C.WindowManager = D.WindowManager = windowManager;

            // B is linked to A C D
            //  A has 1 binding: AB. 
            //  B has 3 bindings: BA BC BD. 
            //  C has 1 binding: CB.
            //  D has 1 binding: DB.
            //  E has no binding.
            //  
            //   A-B-C  
            //     |    E
            //     D

            // The listener plugin listen the window location change event directly from the WindowManager.
            // When a Window from A B C or D change the location, all the window must move together.
            Listener listener = new Listener();
            listener.WindowManager = windowManager;

            // The listener listen any window movement
            listener.WindowManager.WindowMoved += ( sender, e ) =>
            {

            };


            WindowElementBinder binder = new WindowElementBinder();
            binder.WindowManager = windowManager;
            binder.WindowManager.WindowMoved += ( sender, e ) =>
            {
                // Whenever a window moved, ask if there is a window near from this
            };
            binder.BeforeBinding += ( sender, e ) =>
            {
            };
            binder.AfterBinding += ( sender, e ) =>
            {

            };
        }

        class WindowElementBinder : IWindowBinder
        {
            IDictionary<IWindowElement,List<IBinding>> _bindings;

            public IWindowManager WindowManager { get; set; }

            public event EventHandler<WindowBindingEventArgs> BeforeBinding;

            public event EventHandler<WindowBindedEventArgs> AfterBinding;

            public IList<IWindowElement> GetAttachedElements( IWindowElement referential )
            {
                List<IBinding> bindings = _bindings[referential];
                IList<IWindowElement> list = new List<IWindowElement>();
                GetAttachedElements( referential, bindings, list );

                return list;
            }

            void GetAttachedElements( IWindowElement referential, List<IBinding> bindings, IList<IWindowElement> attached )
            {
                if( bindings == null || bindings.Count == 0 ) return;

                for( int i = 0; i < bindings.Count; ++i )
                {
                    IBinding binding = bindings[i];
                    if( binding.First != referential )
                    {
                        GetAttachedElements( binding.First, _bindings[binding.First], attached );
                        attached.Add( binding.First );
                    }
                    if( binding.Second != referential )
                    {
                        GetAttachedElements( binding.Second, _bindings[binding.Second], attached );
                        attached.Add( binding.Second );
                    }
                }
            }


            public void Attach( IWindowElement first, IWindowElement second )
            {
                var binding = new SimpleBinding { First = first, Second = second };

                var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
                if( BeforeBinding != null ) BeforeBinding( this, evt );

                if( evt.Canceled == false )
                {
                    Link( first, binding );
                    Link( second, binding );

                    var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Attach };
                    if( AfterBinding != null ) AfterBinding( this, evtAfter );
                }
            }

            private void Link( IWindowElement window, SimpleBinding binding )
            {
                var bindings = _bindings[window];
                if( bindings == null )
                {
                    bindings = new List<IBinding>();
                    _bindings.Add( window, bindings );
                }
                bindings.Add( binding );
            }

            public void Detach( IBinding binding )
            {
                var evt = new WindowBindingEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                if( BeforeBinding != null ) BeforeBinding( this, evt );

                if( evt.Canceled == false )
                {
                    var bindingsA = _bindings[binding.First];
                    if( bindingsA != null ) bindingsA.Remove( binding );

                    var bindingsB = _bindings[binding.First];
                    if( bindingsB != null ) bindingsB.Remove( binding );

                    var evtAfter = new WindowBindedEventArgs { Binding = binding, BindingType = BindingEventType.Detach };
                    if( AfterBinding != null ) AfterBinding( this, evtAfter );
                }
            }

            class SimpleBinding : IBinding
            {
                public IWindowElement First { get; set; }

                public IWindowElement Second { get; set; }

                public override bool Equals( object obj )
                {
                    var binding = obj as IBinding;
                    if( binding != null )
                    {
                        return ReferenceEquals( binding.First, First ) && ReferenceEquals( binding.Second, Second );
                    }
                    return false;
                }

                public override int GetHashCode()
                {
                    return First.GetHashCode() ^ Second.GetHashCode();
                }
            }
        }

        class Listener
        {
            public IWindowManager WindowManager { get; set; }

            public IWindowBinder WindowBinder { get; set; }

            public Listener()
            {
                WindowManager.WindowResized += WindowManager_WindowResized;
                WindowManager.WindowMoved += WindowManager_WindowMoved;
            }

            void WindowManager_WindowMoved( object sender, WindowElementLocationEventArgs e )
            {
                IWindowElement triggerWindow = e.Window;
                // The Window that moves first
                Assert.That( triggerWindow, Is.Not.Null );

                // Gets all windows attach to the given window
                IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerWindow );
                foreach( IWindowElement window in attachedElements )
                {
                    Delegate action = new Action( () => window.Move( window.Top + e.DeltaTop, window.Left + e.DeltaLeft ) );
                    Dispatcher.CurrentDispatcher.BeginInvoke( action, DispatcherPriority.Render );
                }
            }

            void WindowManager_WindowResized( object sender, WindowElementResizeEventArgs e )
            {
                IWindowElement triggerWindow = e.Window;
                // The Window that moves first
                Assert.That( triggerWindow, Is.Not.Null );

                // Gets all windows attach to the given window
                IEnumerable<IWindowElement> attachedElements = WindowBinder.GetAttachedElements( triggerWindow );
                foreach( IWindowElement window in attachedElements )
                {
                    Delegate action = new Action( () => window.Resize( window.Width + e.DeltaWidth, window.Height + e.DeltaHeight ) );
                    Dispatcher.CurrentDispatcher.BeginInvoke( action, DispatcherPriority.Render );
                }
            }
        }

        class WindowManager : IWindowManager
        {
            class WindowElementData
            {
                public IWindowElement Window { get; set; }

                public double Top { get; set; }

                public double Left { get; set; }

                public double Width { get; set; }

                public double Height { get; set; }
            }

            IDictionary<IWindowElement, WindowElementData> _dic = new Dictionary<IWindowElement, WindowElementData>();

            public event EventHandler<WindowElementEventArgs> WindowHidden;

            public event EventHandler<WindowElementEventArgs> WindowRestored;

            public event EventHandler<WindowElementLocationEventArgs> WindowMoved;

            public event EventHandler<WindowElementResizeEventArgs> WindowResized;

            public void Register( IWindowElement window )
            {
                _dic.Add( window, new WindowElementData
                                    {
                                        Window = window,
                                        Height = window.Height,
                                        Width = window.Width,
                                        Left = window.Left,
                                        Top = window.Top
                                    } );

                window.Hidden += window_Hidden;
                window.Restored += window_Restored;
                window.LocationChanged += window_LocationChanged;
                window.SizeChanged += window_SizeChanged;
            }

            void window_Restored( object sender, EventArgs e )
            {
                if( WindowRestored != null )
                    WindowRestored( sender, new WindowElementEventArgs( sender as IWindowElement ) );
            }

            void window_SizeChanged( object sender, EventArgs e )
            {
                IWindowElement window = sender as IWindowElement;
                WindowElementData data = _dic[window];

                double deltaWidth = window.Width - data.Width;
                double deltaHeight = window.Height - data.Height;

                var evt = new WindowElementResizeEventArgs( window, deltaWidth, deltaHeight );
                if( WindowResized != null )
                    WindowResized( sender, evt );

                data.Width = window.Width;
                data.Height = window.Height;
            }

            void window_LocationChanged( object sender, EventArgs e )
            {
                IWindowElement window = sender as IWindowElement;
                WindowElementData data = _dic[window];

                double deltaTop = window.Top - data.Top;
                double deltaLeft = window.Left - data.Left;

                var evt = new WindowElementLocationEventArgs( window, deltaTop, deltaLeft );
                if( WindowMoved != null )
                    WindowMoved( sender, evt );

                data.Top = window.Top;
                data.Left = window.Left;
            }

            void window_Hidden( object sender, EventArgs e )
            {
                if( WindowHidden != null )
                    WindowHidden( sender, new WindowElementEventArgs( sender as IWindowElement ) );
            }

            public void Unregister( IWindowElement window )
            {
                window.Hidden -= window_Hidden;
                window.Restored -= window_Restored;
                window.LocationChanged -= window_LocationChanged;
                window.SizeChanged -= window_SizeChanged;
                _dic.Remove( window );
            }

        }

        class WindowElement : IWindowElement
        {
            Window _w;
            string _name;

            public IWindowManager WindowManager { get; set; }

            public event EventHandler LocationChanged;

            public event EventHandler SizeChanged;

            public event EventHandler Hidden;

            public event EventHandler Restored;

            public WindowElement( string name, Window w )
            {
                _name = name;
                _w = w;
                _w.LocationChanged += _w_LocationChanged;
                _w.SizeChanged += _w_SizeChanged;

                WindowManager.Register( this );
            }

            void _w_SizeChanged( object sender, SizeChangedEventArgs e )
            {
                if( SizeChanged != null )
                    SizeChanged( sender, EventArgs.Empty );
            }

            void _w_LocationChanged( object sender, EventArgs e )
            {
                if( LocationChanged != null )
                    LocationChanged( sender, e );
            }

            string IWindowElement.Name
            {
                get { return _name; }
            }

            double IWindowElement.Left
            {
                get { return _w.Left; }
            }

            double IWindowElement.Top
            {
                get { return _w.Top; }
            }

            double IWindowElement.Width
            {
                get { return _w.Width; }
            }

            double IWindowElement.Height
            {
                get { return _w.Height; }
            }

            void IWindowElement.Move( double top, double left )
            {
                _w.Top = top;
                _w.Left = left;
            }

            void IWindowElement.Resize( double width, double height )
            {
                _w.Width = width;
                _w.Height = height;
            }
        }

    }
}
