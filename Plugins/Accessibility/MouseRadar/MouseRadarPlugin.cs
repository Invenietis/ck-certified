#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseRadar\MouseRadarPlugin.cs) is part of CiviKey. 
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
using System.Windows.Media;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using CK.Core;
using System.Diagnostics;
using CK.WindowManager.Model;
using MouseRadar.Resources;

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

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<ITopMostService> TopMostService { get; set; }

        void Pause()
        {
            _radar.Pause();
        }

        void Resume()
        {
            ActionType = ActionType.StayOnTheSame;
            Debug.Assert( _radar.CurrentStep == RadarStep.Paused );
            _radar.ToNextStep();
        }

        void Blur()
        {
            if( _radar.ViewModel.Opacity != _blurredOpacity )
                _radar.ViewModel.Opacity = _blurredOpacity;
        }

        void Focus()
        {
            if( _radar.ViewModel.Opacity != _focusedOpacity )
                _radar.ViewModel.Opacity = _focusedOpacity;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _radar = new Radar( MouseDriver.Service );

            _focusedOpacity = (float)(Configuration.User.GetOrSet( "Opacity", 100 )) / 100f;
            _blurredOpacity = .1f;

            _radar.ViewModel.Opacity = _blurredOpacity;

            _radar.ViewModel.RadarSize = Configuration.User.GetOrSet( "RadarSize", 50 ) * 2;
            _radar.ViewModel.ArrowColor = new SolidColorBrush( Configuration.User.GetOrSet( "ArrowColor", Color.FromRgb( 0, 0, 0 ) ) );
            _radar.ViewModel.CircleColor = new SolidColorBrush( Configuration.User.GetOrSet( "CircleColor", Color.FromRgb( 0, 0, 0 ) ) );

            _radar.RotationSpeed = Configuration.User.GetOrSet( "RotationSpeed", 3 );
            _radar.TranslationSpeed = Configuration.User.GetOrSet( "TranslationSpeed", 6 );

            ActionType = HighlightModel.ActionType.StayOnTheSame;

            Highlighter.Service.RegisterTree( "MouseRadarPlugin", R.RadarDisplayName, this );

            _radar.Show();
            _radar.Initialize();
            Pause();
            Configuration.ConfigChanged += OnConfigChanged;
            _radar.RotationDelayExpired += ( o, e ) =>
            {
                _yield = true;
            };

            InitializeTopMost();
        }

        public void Stop()
        {
            UninitializeTopMost();
            Highlighter.Service.UnregisterTree( "MouseRadarPlugin", this );
            _radar.Dispose();
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
            if( _radar._timerRotate.IsEnabled )
                Debug.Assert( _radar.CurrentStep == RadarStep.Rotating && !_radar._timerTranslate.IsEnabled );
            else if( _radar._timerTranslate.IsEnabled )
                Debug.Assert( _radar.CurrentStep == RadarStep.Translating && !_radar._timerRotate.IsEnabled );

            //When begin highlight is triggered, we have three cases : 
            // - we are begin scrolled on, because the scroller is scrolling on the module level. In this case we focus the radar to show the user that the radar is currently being scrolled on.
            // - we are already focused (the current action is stayonthesamelocked and beginScrollingInfo.PreviousElement == this). In this case we do nothing but tell the radar to check that its tick is still in sync with the configuration.
            // - we are paused and the radar is the only element in the scrolling tree (the current action is Normal and beginScrollingInfo.PreviousElement == this) : we do the same as the previous case. 

            Focus();

            //The scroller is actually scrolling on this element, and hooked by the StayOnTheSameLocked, or we are the only element in the scrolling tree : we relay the scroller's tick to the radar.
            if( beginScrollingInfo.PreviousElement == this )
                _radar.Tick( beginScrollingInfo );

            if( _yield )
            {
                //Once the DelayRadar has ticked, we release the scroller.
                scrollingDirective.NextActionType = ActionType = ActionType.GoToAbsoluteRoot;
                scrollingDirective.ActionTime = ActionTime.Immediate;
            }

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( _radar._timerRotate.IsEnabled )
                Debug.Assert( _radar.CurrentStep == RadarStep.Rotating && !_radar._timerTranslate.IsEnabled );
            else if( _radar._timerTranslate.IsEnabled )
                Debug.Assert( _radar.CurrentStep == RadarStep.Translating && !_radar._timerRotate.IsEnabled );

            if( endScrollingInfo.ElementToBeHighlighted != this ) //if the next element to highlight is the element itself, we should not change anything
                Blur();

            //If the scroller was released (see BeginHighlight), we can pause the radar (we are no longer scrolling on it) 
            if( ActionType != ActionType.StayOnTheSame && IsActive )
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
            scrollingDirective.NextActionType = ActionType = ActionType.StayOnTheSame;
            scrollingDirective.ActionTime = ActionTime.Immediate;


            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return _radar.CurrentStep == RadarStep.Paused; }//if the radar is not paused, it is scrolling, so we actually are NOT on the root, we are on a virtual step that is child of the root.
        }

        #endregion

        #region ITopMostService Members

        void InitializeTopMost()
        {
            RegisterTopMost();
            TopMostService.ServiceStatusChanged += OnTopMostServiceStatusChanged;
        }
        void UninitializeTopMost()
        {
            TopMostService.ServiceStatusChanged -= OnTopMostServiceStatusChanged;
            UnregisterTopMost();
        }

        void RegisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted ) TopMostService.Service.RegisterTopMostElement( "200", _radar );
        }

        void UnregisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted ) TopMostService.Service.UnregisterTopMostElement( _radar );
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                TopMostService.Service.RegisterTopMostElement( "200", _radar );
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                TopMostService.Service.UnregisterTopMostElement( _radar );
            }
        }

        #endregion

    }
}
