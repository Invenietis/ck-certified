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
using CommonServices.Accessibility;
using HighlightModel;
using CK.Core;
using KeyScroller;

namespace MouseRadar
{
    [Plugin( MouseRadar.PluginIdString,
           PublicName = MouseRadar.PluginPublicName,
           Version = MouseRadar.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadar : IPlugin, IHighlightableElement
    {
        internal const string PluginIdString = "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Mouse Radar";
        public bool IsActive { get; private set; }
        public float _blurOpacity = .1f;

        ICKReadOnlyList<IHighlightableElement> _child;
        Radar _radar;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }

        [ConfigurationAccessor( MouseRadar.PluginIdString )]
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHighlighterService> Highlighter { get; set; }

        void Pause()
        {
            Console.WriteLine( "Pause" );

            _radar.StopRotation();
            _radar.StopTranslation( false );
            _radar.Model.LapCount = 0;
            IsActive = false;
        }
        void Resume()
        {
            Console.WriteLine( "Resume" );
            ActionType = ActionType.StayOnTheSameForever;
            _radar.StartRotation();
            IsActive = true;
        }
        void Focus()
        {
            _radar.Model.Opacity = (float)( Configuration.User.GetOrSet( "Opacity", 100 ) ) / 100f;
        }

        void Blur()
        {
            _radar.Model.Opacity = _blurOpacity;
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _child = CKReadOnlyListEmpty<IHighlightableElement>.Empty;
            return true;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            switch( e.Key )
            {
                case "Opacity":
                    _radar.Model.Opacity = (int)e.Value / 100f;
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
                    _radar.Model.SetArrowColor( (Color)e.Value );
                    break;
                case "CircleColor":
                    _radar.Model.SetCircleColor( (Color)e.Value );
                    break;
            }
        }

        public void Start()
        {
            _radar = new Radar( MouseDriver.Service );

            _radar.Model.Opacity = (float)( Configuration.User.GetOrSet( "Opacity", 100 ) ) / 100f - .5f;
            _radar.Model.RadarSize = Configuration.User.GetOrSet( "RadarSize", 50 ) * 2;
            _radar.Model.ArrowColor = new SolidColorBrush( Configuration.User.GetOrSet( "ArrowColor", Color.FromRgb( 0, 0, 0 ) ) );
            _radar.Model.CircleColor = new SolidColorBrush( Configuration.User.GetOrSet( "CircleColor", Color.FromRgb( 0, 0, 0 ) ) );

            _radar.RotationSpeed = Configuration.User.GetOrSet( "RotationSpeed", 1 );
            _radar.TranslationSpeed = Configuration.User.GetOrSet( "TranslationSpeed", 1 );

            //ExternalInput.Service.RegisterFor( ExternalInput.Service.DefaultTrigger, TranslateRadar );
            Highlighter.Service.RegisterTree( this );

            _radar.ScreenBoundCollide += ( o, e ) =>
            {

                switch( e.ScreenBound )
                {
                    case ScreenBound.Left:
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
                    default:
                        _radar.Model.AngleMin = 0;
                        _radar.Model.AngleMax = 360;
                        break;
                }
                if( e.ScreenBound != ScreenBound.None ) _radar.StopTranslation();
            };
            _radar.Show();
            _radar.Launch();
            IsActive = true;
            Pause();
            Configuration.ConfigChanged += OnConfigChanged;
        }

        void TranslateRadar()
        {
            if( !IsActive ) return;
            if( _radar.IsTranslating() ) _radar.StopTranslation();
            else _radar.StartTranslation();
            _radar.Model.LapCount = 0;
        }

        public void Stop()
        {
            _radar.Dispose();
            // ExternalInput.Service.Unregister(ExternalInput.Service.DefaultTrigger ,TranslateRadar);
        }

        public void Teardown()
        {

        }

        #endregion

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get { return (int)_radar.Left; }
        }

        public int Y
        {
            get { return (int)_radar.Top; }
        }

        public int Width
        {
            get { return _radar.Model.WindowSize; }
        }

        public int Height
        {
            get { return _radar.Model.WindowSize; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        #endregion

        #region IActionnableElement Members

        public ActionType ActionType { get; set; }

        #endregion

        public ScrollingDirective BeginHighlight( ScrollingInfo scrollingInfo )
        {
            Focus();

            if( _radar.Model.LapCount >= 3 )
            {
                ActionType = ActionType.Normal;
            }

            return null;
        }

        public ScrollingDirective EndHighlight( ScrollingInfo scrollingInfo )
        {
            Blur();

            if( ActionType != ActionType.StayOnTheSameForever ) Pause();

            return null;
        }

        public ScrollingDirective SelectElement()
        {
            if( IsActive ) TranslateRadar();
            else Resume();

            return new ScrollingDirective( ActionType );
        }
    }
}
