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

namespace BasicScroll.Editor
{
    class EditorViewModel : Screen
    {
        IPluginConfigAccessor _scrollConfig;
        IPluginConfigAccessor _keyboardTriggerConfig;
        IKeyboardDriver _keyboardHook;

        public EditorViewModel( IPluginConfigAccessor scrollConfig, IPluginConfigAccessor keyboardTriggerConfig, IKeyboardDriver keyboardHook )
        {
            _scrollConfig = scrollConfig;
            _keyboardTriggerConfig = keyboardTriggerConfig;

            _keyboardHook = keyboardHook;

            this.DisplayName = "Scroll editor";
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

        bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
                NotifyOfPropertyChange( () => IsRecording );
                if( _isRecording ) _keyboardHook.RegisterCancellableKey( -1 );
                else _keyboardHook.UnregisterCancellableKey( -1 );
            }
        }

        public string SelectedKey
        {
            get
            {
                var selectedKey = _keyboardTriggerConfig.User["TriggerKeyCode"];
                if( selectedKey != null ) return string.Format( BasicScroll.Resources.R.Listening, selectedKey.ToString() );
                return BasicScroll.Resources.R.NothingSelected;
            }
        }

        string _formatSpeed = "{0} s";
        public string FormatedSpeed
        {
            get { return string.Format( _formatSpeed, Math.Round( Speed / 1000.0, 1 ).ToString() ); }
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

        public void OnKeyboardHookInvoked( object sender, KeyboardDriverEventArg args )
        {
            _keyboardTriggerConfig.User["TriggerKeyCode"] = args.KeyCode;
            NotifyOfPropertyChange( () => SelectedKey );
            IsRecording = false;
        }
    }
}
