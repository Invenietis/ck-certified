using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CK.WordPredictor
{
    public class FastObservableCollection<T> : ObservableCollection<T>
    {
        static object _lock = new object();

        /// <summary>
        /// This private variable holds the flag to
        /// turn on and off the collection changed notification.
        /// </summary>
        private bool suspendCollectionChangeNotification;

        /// <summary>
        /// Initializes a new instance of the FastObservableCollection class.
        /// </summary>
        public FastObservableCollection( List<T> internalList )
            : base( internalList )
        {
            this.suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// This event is overriden CollectionChanged event of the observable collection.
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// This method adds the given generic list of items
        /// as a range into current collection by casting them as type T.
        /// It then notifies once after all items are added.
        /// </summary>
        /// <param name="items">The source collection.</param>
        public void ReplaceItems( IReadOnlyList<T> items )
        {
            lock( _lock )
            {
                this.SuspendCollectionChangeNotification();
                try
                {
                    base.Clear();

                    foreach( var i in items )
                    {
                        base.Add( i );
                    }
                }
                finally
                {
                    this.NotifyChanges();
                }
            }
        }

        /// <summary>
        /// Raises collection change event.
        /// </summary>
        public void NotifyChanges()
        {
            this.ResumeCollectionChangeNotification();
            var arg
             = new NotifyCollectionChangedEventArgs
                      ( NotifyCollectionChangedAction.Reset );
            this.OnCollectionChanged( arg );
        }

        /// <summary>
        /// This method removes the given generic list of items as a range
        /// into current collection by casting them as type T.
        /// It then notifies once after all items are removed.
        /// </summary>
        /// <param name="items">The source collection.</param>
        public void RemoveItems( IReadOnlyList<T> items )
        {
            this.SuspendCollectionChangeNotification();
            try
            {
                foreach( var i in items )
                {
                    Remove( i );
                }
            }
            catch( Exception ex )
            {
                throw new InvalidCastException(
                   "Please check the type of items getting removed.", ex );
            }
            finally
            {
                this.NotifyChanges();
            }
        }

        /// <summary>
        /// Resumes collection changed notification.
        /// </summary>
        public void ResumeCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Suspends collection changed notification.
        /// </summary>
        public void SuspendCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = true;
        }

        /// <summary>
        /// This collection changed event performs thread safe event raising.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            // Recommended is to avoid reentry 
            // in collection changed event while collection
            // is getting changed on other thread.
            using( BlockReentrancy() )
            {
                if( !this.suspendCollectionChangeNotification )
                {
                    NotifyCollectionChangedEventHandler eventHandler = 
                      this.CollectionChanged;
                    if( eventHandler == null )
                    {
                        return;
                    }

                    // Walk thru invocation list.
                    Delegate[] delegates = eventHandler.GetInvocationList();

                    foreach( NotifyCollectionChangedEventHandler handler in delegates )
                    {
                        //// If the subscriber is a DispatcherObject and different thread.
                        //DispatcherObject dispatcherObject
                        // = handler.Target as DispatcherObject;

                        //if( dispatcherObject != null
                        //       && !dispatcherObject.CheckAccess() )
                        //{
                        //    // Invoke handler in the target dispatcher's thread... 
                        //    // asynchronously for better responsiveness.
                        //    dispatcherObject.Dispatcher.BeginInvoke
                        //          ( DispatcherPriority.DataBind, handler, this, e );
                        //}
                        //else
                        {
                            // Execute handler as is.
                            handler( this, e );
                        }
                    }
                }
            }
        }
    }
}
