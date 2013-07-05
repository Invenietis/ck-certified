#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\ShowNotificationBehavior.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System;

namespace Host.Services
{
    public static class ShowNotificationBehavior
    {
        public static INotifierContext GetNotificationContext( DependencyObject obj )
        {
            return (INotifierContext)obj.GetValue( NotificationContextProperty );
        }

        public static void SetNotificationContext( DependencyObject obj, INotifierContext value )
        {
            obj.SetValue( NotificationContextProperty, value );
        }

        // Using a DependencyProperty as the backing store for NotificationContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationContextProperty =
        DependencyProperty.RegisterAttached(
            "NotificationContext",
            typeof( INotifierContext ),
            typeof( ShowNotificationBehavior ),
            new FrameworkPropertyMetadata( OnContextChanged )
        );

        static void OnContextChanged( DependencyObject depObj, DependencyPropertyChangedEventArgs e )
        {
            var notifier = GetNotificationContext( depObj );
            notifier.Show += ( o, args ) =>
            {
                var tb = depObj as TaskbarIcon;

                if( tb != null && !tb.IsDisposed )
                {
                    tb.ShowCustomBalloon( args.Content, args.Animation, args.Duration );
                }

            };
            notifier.Hide += ( o, args ) =>
            {
                var tb = depObj as TaskbarIcon;
                if( tb != null )
                {
                    tb.Dispatcher.BeginInvoke( (Action)( () => tb.CloseBalloon() ), null );
                }
            };
        }
    }
}
