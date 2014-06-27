#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\INotificationService.cs) is part of CiviKey. 
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

using System;

namespace Host.Services
{
    public enum NotificationTypes
    {
        None,
        Message,
        Ok,
        Warning,
        Error
    }

    public interface INotificationService
    {
        IDisposable ShowNotification( Guid pluginId, string title, string content, int duration );

        IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, Action<object, EventArgs> imageClickAction );

        IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notifType );

        IDisposable ShowNotification( Guid pluginId, string title, string content, int duration, NotificationTypes notifType, Action<object, EventArgs> imageClickAction );

//        IDisposable ShowNotification( Guid pluginId, UIElement content, int duration, NotificationTypes notifType, Action<object, EventArgs> imageClickAction );

        void ShowNotification( Guid pluginId, Exception ex );
    }
}
