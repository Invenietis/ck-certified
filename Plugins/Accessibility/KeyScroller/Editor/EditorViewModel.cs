#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\Editor\EditorViewModel.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using Caliburn.Micro;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using Scroller.Resources;

namespace Scroller.Editor
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

            _currentIndexStrategy = StrategyBridge.AvailableStrategies.IndexOf( _scrollConfig.User.GetOrSet( "Strategy", "ZoneScrollingStrategy" ) );
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
                foreach( string name in StrategyBridge.AvailableStrategies )
                {
                    yield return R.ResourceManager.GetString(name);
                }
            }
        }

        public bool IsTurboStrategy
        {
            get { return _currentIndexStrategy == -1 ? false : StrategyBridge.AvailableStrategies[_currentIndexStrategy] == "TurboScrollingStrategy"; }
        }

        public int CurrentIndexStrategy
        {
            set
            {
                _scrollConfig.User["Strategy"] = StrategyBridge.AvailableStrategies[value];
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
                      
                        return string.Format( Scroller.Resources.R.Listening, keyName.ToString() );
                    }
                    else if( selectedKey.Source == TriggerDevice.Pointer )
                        return string.Format( Scroller.Resources.R.PointerListening, MouseClicFromCode( selectedKey.KeyCode ) );
                }
                return Scroller.Resources.R.NothingSelected;
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
