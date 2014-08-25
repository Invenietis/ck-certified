#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\AppRecovery\ApplicationCrashedEventArgs.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace CK.AppRecovery
{
    public class ApplicationCrashedEventArgs : EventArgs, IDisposable
    {
        static object _lock = new Object();
        static ApplicationCrashedEventArgs _oneCrashOnly;

        public CrashLogWriter CrashLog { get; private set; }

        ApplicationCrashedEventArgs( CrashLogWriter w )
        {
            Debug.Assert( w != null );
            CrashLog = w;
            w.WriteProperty( "CurrentCrashCount", ApplicationRecovery.CurrentCrashCount );
        }

        void IDisposable.Dispose()
        {
            CrashLog.Dispose();
        }

        static internal ApplicationCrashedEventArgs InitOnce()
        {
            lock( _lock )
            {
                if( _oneCrashOnly != null ) return null;
                return _oneCrashOnly = new ApplicationCrashedEventArgs( CrashLogManager.CreateNewCrashLog() );
            }
        }

    }

}
