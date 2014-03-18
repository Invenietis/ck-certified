using System;
using System.Windows.Media;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using CK.Core;
using System.Diagnostics;

namespace MouseRadar
{
    [Plugin( MouseRadarPlugin.PluginIdString,
           PublicName = MouseRadarPlugin.PluginPublicName,
           Version = MouseRadarPlugin.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadarPlugin : IPlugin, IHighlightableElement
    {
        internal const string PluginIdString = "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Mouse Radar";
        bool _yield;
        public bool IsActive { get { return _radar.CurrentStep != RadarStep.Paused; } }

        float _blurredOpacity;
        float _focusedOpacity;

        Radar _radar;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [ConfigurationAccessor( MouseRadarPlugin.PluginIdString )]
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHighlighterService> Highlighter { get; set; }

        void Pause()
        {
            Console.WriteLine( "Pause" );

            _radar.Pause();
        }

        void Resume()
        {
            Console.WriteLine( "Resume" );

            ActionType = ActionType.StayOnTheSameLocked;
            Debug.Assert( _radar.CurrentStep == RadarStep.Paused );
            _radar.ToNextStep();
        }

        void Blur()
        {
            _radar.ViewModel.Opacity = _blurredOpacity;
        }

        void Focus()
        {
            _radar.ViewModel.Opacity = _focusedOpacity;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _radar = new Radar( MouseDriver.Service );

            _focusedOpacity = (float)( Configuration.User.GetOrSet( "Opacity", 100 ) ) / 100f;
            _blurredOpacity = .1f;

            _radar.ViewModel.Opacity = _blurredOpacity;

            _radar.ViewModel.RadarSize = Configuration.User.GetOrSet( "RadarSize", 50 ) * 2;
            _radar.ViewModel.ArrowColor = new SolidColorBrush( Configuration.User.GetOrSet( "ArrowColor", Color.FromRgb( 0, 0, 0 ) ) );
            _radar.ViewModel.CircleColor = new SolidColorBrush( Configuration.User.GetOrSet( "CircleColor", Color.FromRgb( 0, 0, 0 ) ) );

            _radar.RotationSpeed = Configuration.User.GetOrSet( "RotationSpeed", 1 );
            _radar.TranslationSpeed = Configuration.User.GetOrSet( "TranslationSpeed", 1 );

            ActionType = HighlightModel.ActionType.StayOnTheSameLocked;

            Highlighter.Service.RegisterTree( "MouseRadarPlugin", this );

            _radar.Show();
            _radar.Initialize();
            Pause();
            Configuration.ConfigChanged += OnConfigChanged;
            _radar.RotationDelayExpired += (o, e) =>
            {
                _yield = true;
            };
        }

        public void Stop()
        {
            _radar.Dispose();
            if( Highlighter.Status.IsStartingOrStarted )
                Highlighter.Service.UnregisterTree( "MouseRadarPlugin", this );
        }

        public void Teardown()
        {

        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            switch( e.Key )
            {
                case "Opacity":
                    _radar.ViewModel.Opacity = (int)e.Value / 100f;
                    break;
                case "RadarSize":
                    _radar.ViewModel.RadarSize = (int)e.Value * 2;
                    _radar.Height = _radar.ViewModel.WindowSize;
                    _radar.Width = _radar.ViewModel.WindowSize;
                    _radar.UpdateLocation( MouseDriver.Service.CurrentPointerXLocation, MouseDriver.Service.CurrentPointerYLocation );
                    break;
                case "RotationSpeed":
                    _radar.RotationSpeed = (int)e.Value;
                    break;
                case "TranslationSpeed":
                    _radar.TranslationSpeed = (int)e.Value;
                    break;
                case "ArrowColor":
                    _radar.ViewModel.SetArrowColor( (Color)e.Value );
                    break;
                case "CircleColor":
                    _radar.ViewModel.SetCircleColor( (Color)e.Value );
                    break;
            }
        }

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
            get { return _radar.ViewModel.WindowSize; }
        }

        public int Height
        {
            get { return _radar.ViewModel.WindowSize; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        public ActionType ActionType { get; set; }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( beginScrollingInfo.PreviousElement != this ) //otherwise we should already be focused
                Focus();
            else //The scroller is actually scrolling on this element, and hooked by the StayOnTheSameLocked, we relay the scroller's tick to the radar.
                _radar.Tick( beginScrollingInfo );

            if (_yield)
            {
                //Once arrived at the end of the last lap, we release the scroller.
                scrollingDirective.NextActionType = ActionType = ActionType.AbsoluteRoot;
                scrollingDirective.ActionTime = ActionTime.Immediate;
            }

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( endScrollingInfo.ElementToBeHighlighted != this ) //if the next element to highlight is the element itself, we should not change anything
                Blur();

            //If the scroller was released (see BeginHighlight), we can pause the radar (we are no longer scrolling on it) 
            if( ActionType != ActionType.StayOnTheSameLocked && IsActive )
            {
                Pause();
                _yield = false;
            }

            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            _radar.ToNextStep();

            //In any case, when we trigger the input when on the radar, we set the NextType as ActionType.StayOnTheSameLocked.
            scrollingDirective.NextActionType = ActionType = ActionType.StayOnTheSameLocked;
            scrollingDirective.ActionTime = ActionTime.Immediate;

            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return _radar.CurrentStep == RadarStep.Paused; }//if the radar is not paused, it is scrolling, so we actually are NOT on the root, we are on a virtual step that is child of the root.
        }

        #endregion

    }
}
