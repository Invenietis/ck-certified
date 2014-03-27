using System;
using System.Collections.Generic;
using Caliburn.Micro;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using CommonServices;
using KeyScroller.Resources;

namespace KeyScroller.Editor
{
    class EditorViewModel : Screen
    {
        IPluginConfigAccessor _scrollConfig;
        IPluginConfigAccessor _keyboardTriggerConfig;
        ITriggerService _triggerService;

        int _currentIndexStrategy;
        public EditorViewModel( IPluginConfigAccessor scrollConfig, IPluginConfigAccessor keyboardTriggerConfig, ITriggerService triggerService )
        {
            _scrollConfig = scrollConfig;
            _keyboardTriggerConfig = keyboardTriggerConfig;
            _triggerService = triggerService;

            _currentIndexStrategy = StrategyAttribute.AvailableStrategies.IndexOf( _scrollConfig.User.GetOrSet( "Strategy", "ZoneScrollingStrategy" ) );
            this.DisplayName = R.ScrollEditor;
        }

        public IContext Context { get; set; }

        public bool Stopping { get; set; }
        private bool _isRecording;

        public bool IsRecording 
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                
                if( value )
                {
                    _triggerService.InputListener.Record( ( ITrigger trigger ) => {
                        _scrollConfig.User["Trigger"] = trigger;
                        IsRecording = false;
                        
                        NotifyOfPropertyChange( () => SelectedKey );
                        NotifyOfPropertyChange( () => IsRecording );
                    } );
                }
            }
        }

        public int Speed
        {
            get { return _scrollConfig.User.GetOrSet( "Speed", 1000 ); }
            set
            {
                _scrollConfig.User["Speed"] = value;
                NotifyOfPropertyChange( () => Speed );
                NotifyOfPropertyChange( () => FormatedSpeed );
            }
        }

        public int TurboSpeed
        {
            get { return _scrollConfig.User.GetOrSet( "TurboSpeed", 100 ); }
            set
            {
                _scrollConfig.User["TurboSpeed"] = value;
                NotifyOfPropertyChange( () => TurboSpeed );
                NotifyOfPropertyChange( () => FormatedTurboSpeed );
            }
        }
        
        public IEnumerable<string> AvailableStrategies
        {
            get
            {
                foreach( string name in StrategyAttribute.AvailableStrategies )
                {
                    yield return R.ResourceManager.GetString(name);
                }
            }
        }

        public bool IsTurboStrategy
        {
            get { return _currentIndexStrategy == -1 ? false : StrategyAttribute.AvailableStrategies[_currentIndexStrategy] == "TurboScrollingStrategy"; }
        }

        public int CurrentIndexStrategy
        {
            set
            {
                _scrollConfig.User["Strategy"] = StrategyAttribute.AvailableStrategies[value];
                _currentIndexStrategy = value;
                NotifyOfPropertyChange( () => CurrentIndexStrategy );
                NotifyOfPropertyChange( () => IsTurboStrategy );
            }
            get { return _currentIndexStrategy; }
        }

        public string SelectedKey
        {
            get
            {
                var selectedKey = _scrollConfig.User.GetOrSet( "Trigger",  _triggerService.DefaultTrigger );

                if( selectedKey != null ) 
                {
                    if( selectedKey.Source == TriggerDevice.Keyboard )
                    {
                        System.Windows.Forms.Keys keyName = (System.Windows.Forms.Keys) selectedKey.KeyCode;
                      
                        return string.Format( KeyScroller.Resources.R.Listening, keyName.ToString() );
                    }
                    else if( selectedKey.Source == TriggerDevice.Pointer )
                        return string.Format( KeyScroller.Resources.R.PointerListening, MouseClicFromCode( selectedKey.KeyCode ) );
                }
                return KeyScroller.Resources.R.NothingSelected;
            }
        }

        string _formatSpeed = "{0} s";
        public string FormatedSpeed
        {
            get { return string.Format( _formatSpeed, Math.Round( Speed / 1000.0, 1 ).ToString() ); }
        }
        string _formatTurboSpeed = "{0} ms";
        public string FormatedTurboSpeed
        {
            get { return string.Format( _formatTurboSpeed, TurboSpeed ); }
        }

        protected override void OnDeactivate( bool close )
        {
            if( close && !Stopping )
            {
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( BasicScrollEditor.PluginIdString ), ConfigUserAction.Stopped );
                Context.GetService<ISimplePluginRunner>( true ).Apply();
            }
            base.OnDeactivate( close );
        }


        string MouseClicFromCode( int code )
        {
            switch( code )
            {
                case 1:
                    return R.LeftClick;
                case 2:
                    return R.RightClick;
                case 3:
                    return R.MiddleClick;
            }

            return String.Empty;
        }
    }
}
