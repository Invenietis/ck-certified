using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using CommonServices;
using System.Windows.Input;
using CK.WPF.ViewModel;
using System.Diagnostics;
using KeyScroller.Resources;
using System.Globalization;

namespace KeyScroller.Editor
{
    class EditorViewModel : Screen
    {
        IPluginConfigAccessor _scrollConfig;
        IPluginConfigAccessor _keyboardTriggerConfig;
        IKeyboardDriver _keyboardHook;
        IPointerDeviceDriver _pointerHook;

        int _currentIndexStrategy;
        public EditorViewModel( IPluginConfigAccessor scrollConfig, IPluginConfigAccessor keyboardTriggerConfig, IKeyboardDriver keyboardHook, IPointerDeviceDriver pointerHook )
        {
            _scrollConfig = scrollConfig;
            _keyboardTriggerConfig = keyboardTriggerConfig;

            _keyboardHook = keyboardHook;
            _pointerHook = pointerHook;
            _currentIndexStrategy = KeyScrollerPlugin.AvailableStrategies.IndexOf(_scrollConfig.User.GetOrSet( "Strategy", "BasicScrollingStrategy" ));
            this.DisplayName = R.ScrollEditor;
        }

        public IContext Context { get; set; }

        public bool Stopping { get; set; }

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
                foreach( string name in KeyScrollerPlugin.AvailableStrategies )
                {
                    yield return R.ResourceManager.GetString(name);
                }
            }
        }

        public bool IsTurboStrategy
        {
            get { return KeyScrollerPlugin.AvailableStrategies[_currentIndexStrategy] == "TurboScrollingStrategy"; }
        }

        public int CurrentIndexStrategy
        {
            set
            {
                _scrollConfig.User["Strategy"] = KeyScrollerPlugin.AvailableStrategies[value];
                _currentIndexStrategy = value;
                NotifyOfPropertyChange( () => CurrentIndexStrategy );
                NotifyOfPropertyChange( () => IsTurboStrategy );
            }
            get { return _currentIndexStrategy; }
        }

        bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                NotifyOfPropertyChange( () => IsRecording );
                if( _isRecording )
                {
                    _keyboardHook.RegisterCancellableKey( -1 );
                    _pointerHook.PointerButtonDown += OnPointerButtonDown;
                }
                else
                {
                    _keyboardHook.UnregisterCancellableKey( -1 );
                    _pointerHook.PointerButtonDown -= OnPointerButtonDown;
                }
            }
        }


        public string SelectedKey
        {
            get
            {
                var selectedKey = _keyboardTriggerConfig.User.GetOrSet( "TriggerCode", 122 );
                var triggerDevice = _keyboardTriggerConfig.User.GetOrSet( "TriggerDevice", TriggerDevice.Keyboard );

                if( selectedKey != null && triggerDevice != null )
                {
                    TriggerDevice device = TriggerDevice.None;
                    Enum.TryParse<TriggerDevice>( triggerDevice.ToString(), out device );

                    if( device == TriggerDevice.Keyboard )
                    {
                        System.Windows.Forms.Keys keyName;
                        if( Enum.TryParse<System.Windows.Forms.Keys>( selectedKey.ToString(), out keyName ) )
                            return string.Format( KeyScroller.Resources.R.Listening, keyName.ToString() );
                        return string.Format( KeyScroller.Resources.R.Listening, selectedKey.ToString() );
                    }
                    else if( device == TriggerDevice.Pointer )
                        return string.Format( KeyScroller.Resources.R.PointerListening, MouseClicFromCode( Int32.Parse( selectedKey.ToString() ) ) );
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


        void OnPointerButtonDown( object sender, PointerDeviceEventArgs e )
        {
            _keyboardTriggerConfig.User.Set( "TriggerCode", MouseCodeFromButtonInfo( e.ButtonInfo, e.ExtraInfo ) );
            _keyboardTriggerConfig.User.Set( "TriggerDevice", TriggerDevice.Pointer );
            NotifyOfPropertyChange( () => SelectedKey );
            if( IsRecording ) IsRecording = false;
        }

        public void OnKeyboardHookInvoked( object sender, KeyboardDriverEventArg args )
        {
            _keyboardTriggerConfig.User.Set( "TriggerCode", args.KeyCode );
            _keyboardTriggerConfig.User.Set( "TriggerDevice", TriggerDevice.Keyboard );
            NotifyOfPropertyChange( () => SelectedKey );
            if( IsRecording ) IsRecording = false;
        }

        //JL : duplicate with MouseCodeFromButtonInfo in InputTrigger. Where can it be put to avoid duplication ?
        public int MouseCodeFromButtonInfo( ButtonInfo buttonInfo, string extraInfo )
        {
            if( buttonInfo == ButtonInfo.DefaultButton )
            {
                return 1;
            }

            if( buttonInfo == ButtonInfo.XButton )
            {
                if( extraInfo == "Right" )
                    return 2;

                if( extraInfo == "Middle" )
                    return 3;
            }

            throw new Exception( String.Format( "The specified buttonInfo is incorrect. (ButtonInfo : {0}, ExtraInfo : {1}) ", buttonInfo.ToString(), extraInfo ) );
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
