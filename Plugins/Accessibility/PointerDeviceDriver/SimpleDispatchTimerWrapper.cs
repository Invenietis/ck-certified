#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\PointerDeviceDriver\SimpleDispatchTimerWrapper.cs) is part of CiviKey. 
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
using System.Windows.Threading;

namespace PointerDeviceDriver
{
    /// <summary>
    /// Simple object that embeds a DispatchTimer that triggers a method given in the constructor
    /// </summary>
    public class SimpleDispatchTimerWrapper : IDisposable
    {
        DispatcherTimer _timer;
        EventHandler _onTimerTick;
        public SimpleDispatchTimerWrapper( TimeSpan timeCounter, EventHandler onTimerTick )
        {
            _onTimerTick = onTimerTick;

            _timer = new DispatcherTimer();
            _timer.Interval = timeCounter;
            _timer.Tick += _onTimerTick;

            _isRunning = false;
        }

        bool _isRunning;
        public bool IsRunning { get { return _isRunning; } }

        public void StopMonitoring()
        {
            _timer.Stop();
            _isRunning = false;
        }

        public void StartMonitoring()
        {
            _timer.Start();
            _isRunning = true;
        }

        public void Dispose()
        {
            _timer.Tick -= _onTimerTick;
            _onTimerTick = null;
        }
    }
}
