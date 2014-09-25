#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\AttachedProperties\ListBoxAttachedProperties.cs) is part of CiviKey. 
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
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CK.Plugins.ObjectExplorer
{
    public class ListBoxAttachedProperties : DependencyObject
    {
        #region AutoScrollToCurrentItem

        #region Properties

        public static readonly DependencyProperty AutoScrollToCurrentItemProperty = DependencyProperty.RegisterAttached( "AutoScrollToCurrentItem", typeof( bool ), typeof( ListBoxAttachedProperties ), new UIPropertyMetadata( default( bool ), OnAutoScrollToCurrentItemChanged ) );

        /// <summary>
        /// Returns the value of the AutoScrollToCurrentItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToCurrentItem( DependencyObject obj )
        {
            return (bool)obj.GetValue( AutoScrollToCurrentItemProperty );
        }

        /// <summary>
        /// Sets the value of the AutoScrollToCurrentItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToCurrentItemProperty</param>
        public static void SetAutoScrollToCurrentItem( DependencyObject obj, bool value )
        {
            obj.SetValue( AutoScrollToCurrentItemProperty, value );
        }

        #endregion

        #region Events

        /// <summary>
        /// This method will be called when the AutoScrollToCurrentItem
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToCurrentItemChanged( DependencyObject s, DependencyPropertyChangedEventArgs e )
        {
            var listBox = s as ListBox;
            if( listBox != null )
            {
                var listBoxItems = listBox.Items;
                if( listBoxItems != null )
                {
                    var newValue = (bool)e.NewValue;

                    var autoScrollToCurrentItemWorker = new EventHandler( ( s1, e2 ) => OnAutoScrollToCurrentItem( listBox, listBox.Items.CurrentPosition ) );

                    if( newValue )
                        listBoxItems.CurrentChanged += autoScrollToCurrentItemWorker;
                    else
                        listBoxItems.CurrentChanged -= autoScrollToCurrentItemWorker;
                }
            }
        }

        /// <summary>
        /// This method will be called when the ListBox should
        /// be scrolled to the given index
        /// </summary>
        /// <param name="listBox">The ListBox which should be scrolled</param>
        /// <param name="index">The index of the item to which it should be scrolled</param>
        public static void OnAutoScrollToCurrentItem( ListBox listBox, int index )
        {
            if( listBox != null && listBox.Items != null && listBox.Items.Count > index && index >= 0 )
                listBox.ScrollIntoView( listBox.Items[index] );
        }

        #endregion

        #endregion

        #region AutoScrollToNewItem

        #region Properties

        public static readonly DependencyProperty AutoScrollToNewItemProperty = DependencyProperty.RegisterAttached( "AutoScrollToNewItem", typeof( bool ), typeof( ListBoxAttachedProperties ), new UIPropertyMetadata( default( bool ), OnAutoScrollToNewItemChanged ) );

        /// <summary>
        /// Returns the value of the AutoScrollToNewItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToNewItem( DependencyObject obj )
        {
            return (bool)obj.GetValue( AutoScrollToNewItemProperty );
        }

        /// <summary>
        /// Sets the value of the AutoScrollToCurrentItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToNewItemProperty</param>
        public static void SetAutoScrollToNewItem( DependencyObject obj, bool value )
        {
            obj.SetValue( AutoScrollToNewItemProperty, value );
        }

        #endregion

        #region Events

        /// <summary>
        /// This method will be called when the AutoScrollToNewItem
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToNewItemChanged( DependencyObject s, DependencyPropertyChangedEventArgs e )
            {
                var listBox = s as ListBox;
                if( listBox != null )
                {
                    var listBoxItems = listBox.Items;
                    if( listBoxItems != null )
                    {
                        var newValue = (bool)e.NewValue;

                        var autoScrollToNewItemWorker = new NotifyCollectionChangedEventHandler( ( s1, e2 ) => OnAutoScrollToNewItem( listBox ) );
                        INotifyCollectionChanged collection = listBox.Items as INotifyCollectionChanged;
                        if(collection != null)
                        {
                            if( newValue )
                                collection.CollectionChanged += autoScrollToNewItemWorker;
                            else
                                collection.CollectionChanged -= autoScrollToNewItemWorker;
                        }
                    }
                }
            }

        /// <summary>
        /// This method will be called when the ListBox should
        /// be scrolled to the end
        /// </summary>
        /// <param name="listBox">The ListBox which should be scrolled</param>
        public static void OnAutoScrollToNewItem( ListBox listBox )
        {
            if( listBox != null && listBox.Items != null && listBox.Items.Count > 0 && listBox.SelectedItem == listBox.Items[0] )
                listBox.ScrollIntoView( listBox.Items[listBox.Items.Count - 1] );
        }

        #endregion
        #endregion
    }

}
