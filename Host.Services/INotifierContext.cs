#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\INotifierContext.cs) is part of CiviKey. 
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
using System.Windows.Controls.Primitives;

namespace Host.Services
{
    public class NotificationArgs : EventArgs
    {
        public CKNotification Content { get; private set; }

        public PopupAnimation Animation { get; private set; }

        public int? Duration { get; private set; }
        
        public NotificationArgs( CKNotification content, PopupAnimation animation, int? duration )
        {
            Content = content;
            Animation = animation;
            Duration = duration;
        }
    }

    public interface INotifierContext
    {
        event EventHandler<NotificationArgs> Show;

        event EventHandler Hide;

        void FireHide();
    }
}
