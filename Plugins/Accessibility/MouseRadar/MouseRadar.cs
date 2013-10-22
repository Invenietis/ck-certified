using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

namespace MouseRadar
{
    [Plugin( MouseRadar.PluginIdString,
           PublicName = MouseRadar.PluginPublicName,
           Version = MouseRadar.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadar :  IPlugin
    {
        internal const string PluginIdString = "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Radar";
        
        Radar _radar;
        
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }

        [ConfigurationAccessor( MouseRadar.PluginIdString )]
        public IPluginConfigAccessor Configuration { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {   
            return true;
        }

        void OnConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            switch( e.Key )
            {
                case "Opacity" :
                    _radar.Model.Opacity = (int) e.Value / 100f;
                    break;
                case "RadarSize":
                    _radar.Model.RadarSize = (int)e.Value * 2;
                    _radar.Height = _radar.Model.WindowSize;
                    _radar.Width = _radar.Model.WindowSize;
                    _radar.UpdateLocation( MouseDriver.Service.CurrentPointerXLocation, MouseDriver.Service.CurrentPointerYLocation );
                    break;
                case "RotationSpeed":
                    _radar.RotationSpeed = (int)e.Value;
                    break;
                case "TranslationSpeed":
                    _radar.TranslationSpeed = (int)e.Value;
                    break;
                case "ArrowColor":
                    _radar.Model.SetArrowColor((Color)e.Value);
                    break;
                case "CircleColor":
                    _radar.Model.SetCircleColor((Color)e.Value );
                    break;
            }
        }

        public void Start()
        {
            Configuration.ConfigChanged += OnConfigChanged;
            _radar = new Radar( MouseDriver.Service );

            _radar.Model.Opacity = (float) Configuration.User.GetOrSet("Opacity", 100) / 100f;
            _radar.Model.RadarSize = Configuration.User.GetOrSet( "RadarSize", 50 ) * 2;
            _radar.Model.ArrowColor = new SolidColorBrush(Configuration.User.GetOrSet("ArrowColor", Color.FromRgb(0, 0, 0)));
            _radar.Model.CircleColor = new SolidColorBrush( Configuration.User.GetOrSet( "CircleColor", Color.FromRgb( 0, 0, 0 ) ) );

            _radar.RotationSpeed = Configuration.User.GetOrSet( "RotationSpeed", 1 );
            _radar.TranslationSpeed = Configuration.User.GetOrSet( "TranslationSpeed", 1 );
            ExternalInput.Service.Triggered += TranslateRadar;
            //MouseDriver.Service.PointerButtonUp += TranslateRadar;

            _radar.ScreenBoundCollide += ( o, e ) => {
                
                switch( e.ScreenBound )
                {
                    case ScreenBound.Left :
                        _radar.Model.AngleMin = 270;
                        _radar.Model.AngleMax = 90;
                        break;
                    case ScreenBound.Top:
                        _radar.Model.AngleMin = 0;
                        _radar.Model.AngleMax = 180;
                        break;
                    case ScreenBound.Right:
                        _radar.Model.AngleMin = 90;
                        _radar.Model.AngleMax = 270;
                        break;
                    case ScreenBound.Bottom:
                        _radar.Model.AngleMin = 180;
                        _radar.Model.AngleMax = 360;
                        break;
                    default :
                        _radar.Model.AngleMin = 0;
                        _radar.Model.AngleMax = 360;
                        break;
                }
                if( e.ScreenBound != ScreenBound.None ) _radar.StopTranslation();
            };
            _radar.Show();
            _radar.Launch();
        }

        void TranslateRadar(object o, EventArgs e)
        {
            if(_radar.IsTranslating()) _radar.StopTranslation();
            else _radar.StartTranslation();
        }

        public void Stop()
        {
            _radar.Dispose();
            MouseDriver.Service.PointerButtonUp -= TranslateRadar;
            ExternalInput.Service.Triggered -= TranslateRadar;
        }

        public void Teardown()
        {

        }

        #endregion
    }
}
