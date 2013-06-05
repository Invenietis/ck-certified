using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SimpleSkin
{
    /// <summary>
    /// A wrapper around a Dispatcher.
    /// Used to hide the BeingInvokeShutDown method.
    /// </summary>
    public class DispatcherWrapper : ISimpleDispatcher
    {
        Dispatcher _dispatcher;
        public DispatcherWrapper( Dispatcher dispatcher )
        {
            _dispatcher = dispatcher;
        }
        public System.Windows.Threading.DispatcherOperation BeginInvoke( Delegate method, params object[] args )
        {
            return _dispatcher.BeginInvoke( method, args );
        }

        public System.Windows.Threading.DispatcherOperation BeginInvoke( System.Windows.Threading.DispatcherPriority priority, Delegate method )
        {
            return _dispatcher.BeginInvoke( priority, method );
        }

        public System.Windows.Threading.DispatcherOperation BeginInvoke( Delegate method, System.Windows.Threading.DispatcherPriority priority, params object[] args )
        {
            return _dispatcher.BeginInvoke( method, priority, args );
        }

        public System.Windows.Threading.DispatcherOperation BeginInvoke( System.Windows.Threading.DispatcherPriority priority, Delegate method, object arg )
        {
            return _dispatcher.BeginInvoke( priority, method, arg );
        }

        public System.Windows.Threading.DispatcherOperation BeginInvoke( System.Windows.Threading.DispatcherPriority priority, Delegate method, object arg, params object[] args )
        {
            return _dispatcher.BeginInvoke( priority, method, arg, args );
        }

        public object Invoke( Delegate method, params object[] args )
        {
            return _dispatcher.Invoke( method, args );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, Delegate method )
        {
            return _dispatcher.Invoke( priority, method );
        }

        public object Invoke( Delegate method, System.Windows.Threading.DispatcherPriority priority, params object[] args )
        {
            return _dispatcher.Invoke( method, priority, args );
        }

        public object Invoke( Delegate method, TimeSpan timeout, params object[] args )
        {
            return _dispatcher.Invoke( method, timeout, args );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, Delegate method, object arg )
        {
            return _dispatcher.Invoke( priority, method, arg );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, TimeSpan timeout, Delegate method )
        {
            return _dispatcher.Invoke( priority, timeout, method );
        }

        public object Invoke( Delegate method, TimeSpan timeout, System.Windows.Threading.DispatcherPriority priority, params object[] args )
        {
            return _dispatcher.Invoke( method, timeout, priority, args );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, Delegate method, object arg, params object[] args )
        {
            return _dispatcher.Invoke( priority, method, arg, args );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg )
        {
            return _dispatcher.Invoke( priority, timeout, method, arg );
        }

        public object Invoke( System.Windows.Threading.DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg, params object[] args )
        {
            return _dispatcher.Invoke( priority, timeout, method, arg, args );
        }
    }
}
