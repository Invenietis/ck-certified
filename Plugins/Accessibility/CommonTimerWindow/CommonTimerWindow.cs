using System;
using System.Windows.Media.Animation;
using CK.Plugin;
using CommonServices;
using CK.Core;

namespace CK.Plugins.CommonTimerWindow
{
    [Plugin( CommonTimerWindow.PluginIdString, PublicName = "Common timer window", Version = CommonTimerWindow.PluginIdVersion,
     Categories = new string[] { "Visual" },
     Description="Simple user interface to show the events of the CommonTimer")]
    public class CommonTimerWindow : IPlugin
    {
        const string PluginIdString = "{62C9697A-95A1-475e-AFDE-1094B018382A}";
        const string PluginIdVersion = "1.0.0";
        public static readonly IVersionedUniqueId PluginId = new SimpleVersionedUniqueId( PluginIdString, PluginIdVersion );

        ICommonTimer _timer;
        TimerView _window;
        int ticks;

        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public ICommonTimer Timer
        {
            get { return _timer; }
            set { _timer = value; }
        }

        int Multiple
        {
            get
            {
                if (_window != null)
                    return Convert.ToInt32(_window.Slider.Value);
                return 3;
            }
        }

        private Storyboard HeartBeatAnimation { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _window = new TimerView();
            _window.ModifyButtonP.Click += new System.Windows.RoutedEventHandler( ModifyButtonP_Click );
            _window.ModifyButtonM.Click += new System.Windows.RoutedEventHandler( ModifyButtonM_Click );

            HeartBeatAnimation = (Storyboard)_window.FindResource( "HeartBeat" );

            return true;
        }

        public void Start()
        {
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.IntervalChanged += new EventHandler(Timer_IntervalChanged);

            _window.TimerFrequency.Text = Timer.Interval.ToString();
            _window.Show();
        }

        public void Stop()
        {
            Timer.Tick -= new EventHandler(Timer_Tick);
            if (_window != null)
                _window.Close();
        }

        public void Teardown()
        {
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            ticks++;
            if (ticks >= Multiple)
            {
                HeartBeatAnimation.Begin();
                ticks = 0;
            }
        }

        void Timer_IntervalChanged(object sender, EventArgs e)
        {
            if (_window != null)
                _window.TimerFrequency.Text = Timer.Interval.ToString();
        }

        void ModifyButtonP_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Timer.Interval += 500;
        }

        void ModifyButtonM_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if( Timer.Interval >= 200 )
                Timer.Interval -= 200;
            else
            {
                Timer.Interval = 1;
            }
        }
    }
}
