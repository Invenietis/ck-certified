using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
        }

        public void StopMonitoring()
        {
            _timer.Stop();
        }

        public void StartMonitoring()
        {
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Tick -= _onTimerTick;
            _onTimerTick = null;
        }
    }
}
