#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Services\CommonServices\Accessibility\IMouseWatcher.cs) is part of CiviKey. 
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
using System.ComponentModel;
using CK.Plugin;

namespace CommonServices
{
    public interface IMouseWatcher : IDynamicService
    {
        int TimeBeforeCountDownStarts { get; set; }
        int CountDownDuration { get; set; }

        /// <summary>
        /// Event fired when a property on the IMouseWatcher implementation changes.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event fired when the MouseWatcher starts to count.
        /// Its eventargs contain the time from which the autoclick counts down.
        /// </summary>
        event AutoClickCountDownEventHandler StartingCountDown;

        /// <summary>
        /// Event fired when the MouseWatcher
        /// </summary>
        event AutoClickProgressValueChangedEventHandler ProgressValueChanged;

        /// <summary>
        /// Event fired when the MouseWatcher has decided that a click should be launched (depends on the implementation)
        /// </summary>
        event EventHandler LaunchClick;

        /// <summary>
        /// Event fired when the MouseWatcher has decided that the click should be cancelled (depends on the implementation)
        /// </summary>
        event EventHandler ClickCanceled;
        
        /// <summary>
        /// Gets the progressValue of the countdown (in %)
        /// </summary>
        int ProgressValue { get; }        

        /// <summary>
        /// Event fired when the MouseWatcher has been paused (via the Pause method)
        /// </summary>
        event EventHandler HasPaused;

        /// <summary>
        /// Event fired when the MouseWatcher is not paused anymore (can be done via the Resume method)
        /// </summary>
        event EventHandler HasResumed;

        /// <summary>
        /// Pauses the MouseWatcher (ProgressValue is set to 0)
        /// </summary>
        void Pause();

        /// <summary>
        /// Has the MouseWatcher start over.
        /// </summary>
        void Resume();
    }

    public delegate void AutoClickCountDownEventHandler( object sender, AutoClickCountDownEventArgs e );

    public class AutoClickCountDownEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the time (in milliseconds) of the countdown, before the autoclick asks for a click to be launched.
        /// </summary>
        public int TimeBeforeClick { get; private set; }

        public AutoClickCountDownEventArgs(int timeBeforeClick)
        {
            TimeBeforeClick = timeBeforeClick;
        }
    }

    public delegate void AutoClickProgressValueChangedEventHandler( object sender, AutoClickProgressValueChangedEventArgs e );

    public class AutoClickProgressValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ProgressValue of the MouseWatcher
        /// </summary>
        public int ProgressValue { get; private set; }

        public AutoClickProgressValueChangedEventArgs( int progressValue )
        {
            ProgressValue = progressValue;
        }
    }
}
