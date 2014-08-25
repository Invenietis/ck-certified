#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseWatcher\MouseWatcher.cs) is part of CiviKey. 
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
using CommonServices;
using CK.Plugin;
using System.Diagnostics;
using System.Windows.Threading;
using CK.Plugin.Config;
using System.ComponentModel;

namespace MouseWatcher
{
    /// <summary>
    /// This plugin watches the MousePointer, and indicates whether is hasn't been moving since a certain period of time (that can be set via <see cref="TimeBeforeCountDownStarts"/>).
    /// it also provides the progress value (from 0 to 100) accordingly to the time set
    /// Provides only one instance of watchers, two plugins can't use it with different configurations, the last to be set is the one used.
    /// </summary>
    [Plugin( "{D4D8660C-973B-46d3-8414-6FCC02AD74F5}", PublicName = "Mouse Watcher", Version = "1.0.0", Categories = new string[] { "Advanced" } )]
    public class MouseWatcher : IPlugin, IMouseWatcher
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IPointerDeviceDriver MouseDriver { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        Stopwatch _progressStopwatch = new Stopwatch(); //Watch used for the progressbar
        DispatcherTimer _idleTimer = new DispatcherTimer(); //Watch used to launch the progressStopwatch
        DispatcherTimer _updateTimer = new DispatcherTimer(); //Used to keep the view updated

        private bool _isPaused = true;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;

                if ( value ) ResetAndStopAllWatches();
                else LaunchTimers();
            }
        }

        private int _countDownDuration;
        public int CountDownDuration
        {
            get { return _countDownDuration; }
            set
            {
                if ( value < 100 )
                    _countDownDuration = 100;
                else
                    _countDownDuration = value;
                if ( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( "CountDownDuration" ) );
            }
        }

        private int _timeBeforeCountDownStarts;
        public int TimeBeforeCountDownStarts
        {
            get { return _timeBeforeCountDownStarts; }
            set
            {
                if ( value < 100 )
                    _timeBeforeCountDownStarts = 100;
                else
                    _timeBeforeCountDownStarts = value;

                _idleTimer.Interval = new TimeSpan( 0, 0, 0, 0, TimeBeforeCountDownStarts );
                if ( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( "TimeBeforeCountDownStarts" ) );
            }
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if ( Config.User["CountDownDuration"] != null )
                CountDownDuration = (int)Config.User["CountDownDuration"];
            else CountDownDuration = 2000;

            if ( Config.User["TimeBeforeCountDownStarts"] != null )
                TimeBeforeCountDownStarts = (int)Config.User["TimeBeforeCountDownStarts"];
            else TimeBeforeCountDownStarts = 1500;

            Initialize();
        }

        public void Stop()
        {
            ResetAndStopAllWatches();
            MouseDriver.PointerMove -= new PointerDeviceEventHandler( OnPointerAction );
            MouseDriver.PointerButtonDown -= new PointerDeviceEventHandler( OnPointerAction );
            MouseDriver.PointerButtonUp -= new PointerDeviceEventHandler( OnPointerAction );
            _updateTimer.Tick -= new EventHandler( onTimerTick );
            _idleTimer.Tick -= new EventHandler( onIdleStopWatchTick );
        }

        public void Teardown()
        {

        }

        #endregion

        #region IMouseWatcher Members

        public event PropertyChangedEventHandler PropertyChanged;

        public event AutoClickCountDownEventHandler StartingCountDown;

        public event EventHandler LaunchClick;
        public event EventHandler ClickCanceled;
        public event EventHandler HasPaused;
        public event EventHandler HasResumed;

        public int ProgressValue
        {
            get { return (int)( (double)( (double)_progressStopwatch.Elapsed.TotalMilliseconds / (double)CountDownDuration ) * (double)100 ); }
        }

        #endregion

        private void Initialize()
        {
            //This timer is used to have the progressbar update accordingly to the elapsed time.
            //The point is to tick 100 times to get the 100% of the progress
            _updateTimer.Interval = new TimeSpan( 0, 0, 0, 0, CountDownDuration / 100 );
            _updateTimer.Tick += new EventHandler( onTimerTick );

            //This timer is used to spot when the user hasn't been moving for a while, and then start the progressStopwatch
            _idleTimer.Interval = new TimeSpan( 0, 0, 0, 0, TimeBeforeCountDownStarts );
            _idleTimer.Tick += new EventHandler( onIdleStopWatchTick );

            MouseDriver.PointerMove += OnPointerAction;
            MouseDriver.PointerButtonDown += OnPointerAction;
            MouseDriver.PointerButtonUp += OnPointerAction;
            //MouseDriver.PointerButtonDoubleClick += OnDoubleClick;
            MouseDriver.WheelAction += OnWheelAction;

            LaunchTimers();
        }

        private void OnWheelAction( object sender, WheelActionEventArgs e )
        {
        }

        private void onTimerTick( object sender, EventArgs e )
        {
            //The updateTimer has ticked, we refresh the view and check if the Progressvalue isn't already at 100
            if ( ProgressValue >= 100 )
            {
                ProgressCompleted();
            }
            else if ( ProgressValueChanged != null )
            {
                ProgressValueChanged( this, new AutoClickProgressValueChangedEventArgs( ProgressValue ) );
            }
        }

        private void ProgressCompleted()
        {
            LaunchTimers(); //Back to the first state

            if ( LaunchClick != null )
                LaunchClick( this, EventArgs.Empty );
        }

        private void onIdleStopWatchTick( object sender, EventArgs e )
        {
            //Entering second state
            StartProgressWatch();
        }

        private void OnPointerAction( object sender, EventArgs e )
        {
            //Back to the frst state : only idle is launched
            if ( !IsPaused )
            {
                LaunchTimers();

                if ( ClickCanceled != null )
                    ClickCanceled( this, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Sets the Watch to the second state : the ProgressWatch is launched while the idle one is stopped
        /// </summary>
        private void StartProgressWatch()
        {
            if ( StartingCountDown != null )
                StartingCountDown( this, new AutoClickCountDownEventArgs( CountDownDuration ) );

            _idleTimer.Stop();
            _updateTimer.Stop();
            _progressStopwatch.Reset();

            _progressStopwatch.Start();
            _updateTimer.Start();
        }

        /// <summary>
        /// Resets and stops all timers.
        /// </summary>
        public void ResetAndStopAllWatches()
        {
            _idleTimer.Stop();
            _progressStopwatch.Reset();
            _updateTimer.Stop();
        }

        /// <summary>
        /// Sets the process to the first state : only idleStopwatch is launched
        /// </summary>
        public void LaunchTimers()
        {
            _progressStopwatch.Reset();
            _updateTimer.Stop();
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        #region IMouseWatcher Members

        public event AutoClickProgressValueChangedEventHandler ProgressValueChanged;

        public void Pause()
        {
            ResetAndStopAllWatches();
            _isPaused = true;
            if ( HasPaused != null )
                HasPaused( this, EventArgs.Empty );
        }

        public void Resume()
        {
            LaunchTimers();
            _isPaused = false;
            if ( HasResumed != null )
                HasResumed( this, EventArgs.Empty );
        }

        #endregion
    }
}
