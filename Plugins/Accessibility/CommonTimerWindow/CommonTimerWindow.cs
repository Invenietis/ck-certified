#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\CommonTimerWindow\CommonTimerWindow.cs) is part of CiviKey. 
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
using System.Windows.Media.Animation;
using CK.Plugin;
using CommonServices;
using CK.Core;

namespace CK.Plugins.CommonTimerWindow
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Visual" },
     Description="Simple user interface to show the events of the CommonTimer")]
    public class CommonTimerWindow : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{62C9697A-95A1-475e-AFDE-1094B018382A}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Common Timer Window";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

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
