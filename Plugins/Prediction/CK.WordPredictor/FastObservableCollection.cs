#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\FastObservableCollection.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CK.Core;

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
        /// This method is thread safe.
        /// </summary>
        /// <param name="items">The source collection.</param>
        public void ReplaceItems( ICKReadOnlyList<T> items )
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
        public void RemoveItems( ICKReadOnlyList<T> items )
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
