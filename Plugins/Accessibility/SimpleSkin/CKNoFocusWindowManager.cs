using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using CK.Windows;

namespace SimpleSkin
{
    public class CKNoFocusWindowManager
    {
        Dispatcher _innerDispatcher;

        private void FindOrCreateThread()
        {
            if( _innerDispatcher == null )
            {
                WPFThread secondThread = new WPFThread( "CiviKey NoFocusWindow Thread" );
                _innerDispatcher = secondThread.Dispatcher;
            }
        }

        /// <summary>
        /// Gets the dispatcher of the NoFocusWindow Thread.
        /// </summary>
        public Dispatcher NoFocusWindowThreadDispatcher
        {
            get
            {
                if( _innerDispatcher == null )
                {
                    FindOrCreateThread();
                }

                return _innerDispatcher;
            }
        }

        /// <summary>
        /// Shuts down the underlying Thread. 
        /// Should ONLY be called when CiviKey is shutting down.
        /// </summary>
        public void Shutdown()
        {
            _innerDispatcher.BeginInvokeShutdown( DispatcherPriority.ApplicationIdle );
        }

        /// <summary>
        /// Create a CKWindow in the CKWindow Thread.
        /// Invokes the function set as parameter in the CKWindow Thread
        /// </summary>
        /// <typeparam name="T">The type of the window, must inherit from <see cref="CKWindow"/></typeparam>
        /// <param name="createFunction">The function that launches the "new" of the window</param>
        /// <returns>An object of the type set as parameter, created in the CKWindow Thread</returns>
        public T CreateNoFocusWindow<T>( Func<T> createFunction )
            where T : CKWindow
        {
            T window = default( T );
            NoFocusWindowThreadDispatcher.Invoke( (Action)( () =>
            {
                window = createFunction.Invoke();
            } )
            , null );

            return window;
        }

        class WPFThread
        {
            public readonly Dispatcher Dispatcher;
            readonly object _lock;

            public WPFThread( string name )
            {
                _lock = new object();
                Thread t = new Thread( StartDispatcher );
                t.Name = name;
                t.SetApartmentState( ApartmentState.STA );
                lock( _lock )
                {
                    t.Start();
                    Monitor.Wait( _lock );
                }
                Dispatcher = Dispatcher.FromThread( t );
            }

            void StartDispatcher()
            {
                // This creates the Dispatcher and pushes the job.
                Dispatcher.CurrentDispatcher.BeginInvoke( (System.Action)DispatcherStarted, null );
                // Initializes a SynchronizationContext (for tasks ot other components that would require one). 
                SynchronizationContext.SetSynchronizationContext( new DispatcherSynchronizationContext( Dispatcher.CurrentDispatcher ) );
                Dispatcher.Run();
            }

            void DispatcherStarted()
            {
                lock( _lock )
                {
                    Monitor.Pulse( _lock );
                }
            }
        }

    }
}
