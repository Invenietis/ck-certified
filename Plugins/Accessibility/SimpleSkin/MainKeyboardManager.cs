#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\SimpleSkin.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using Host.Services;
using SimpleSkin.ViewModels;
using CK.Windows;
using CK.Windows.Helpers;
using System.Linq;
using CommonServices.Accessibility;
using System.Diagnostics;
using CK.Plugins.SendInputDriver;
using System.IO;
using Help.Services;
using HighlightModel;

namespace SimpleSkin
{

    [Plugin( PluginIdString,
        PublicName = PluginPublicName,
        Version = PluginIdVersion,
        Categories = new[] { "Visual", "Accessibility" } )]
    public class MainKeyboardManager : IPlugin, IHaveDefaultHelp
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );
        const string PluginIdString = "{36C4764A-111C-45e4-83D6-E38FC1DF5979}";
        readonly Guid PluginGuid = new Guid( PluginIdString );
        const string PluginPublicName = "MainKeyboardManager";
        const string PluginIdVersion = "1.6.0";

        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService]
        public INotificationService Notification { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        #region IPlugin Implementation

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            ChangeActiveCurrentKeyboardStatus( true );
            RegisterEvents();
        }

        public void Stop()
        {
            Context.ServiceContainer.Remove( typeof( IPluginConfigAccessor ) );

            UnregisterEvents();

            ChangeActiveCurrentKeyboardStatus( false );
        }

        public void Teardown()
        {
        }

        #region ToolMethods

        void ChangeActiveCurrentKeyboardStatus( bool active )
        {
            if( KeyboardContext.Status == InternalRunningStatus.Started && KeyboardContext.Service.Keyboards.Count > 0 )
            {
                //temporary for prediction
                if( KeyboardContext.Service.CurrentKeyboard.Name == "Prediction" ) return;
                if( !active ) KeyboardContext.Service.Keyboards.KeyboardDeactivated -= OnKeyboardDeactivated;
                KeyboardContext.Service.CurrentKeyboard.IsActive = active;
                if( active ) KeyboardContext.Service.Keyboards.KeyboardDeactivated += OnKeyboardDeactivated;
            }

        }

        void OnKeyboardDeactivated( object sender, KeyboardEventArgs e )
        {
            if( e.Keyboard == KeyboardContext.Service.CurrentKeyboard )
            {
                Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( PluginIdString ), ConfigUserAction.Stopped );
                Context.PluginRunner.Apply();
                return;
            }
        }

        #endregion

        #endregion

        #region OnXXXX

        private void RegisterEvents()
        {
            KeyboardContext.Service.CurrentKeyboardChanging += new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            KeyboardContext.Service.CurrentKeyboardChanged += new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        }

        private void UnregisterEvents()
        {
            KeyboardContext.Service.CurrentKeyboardChanging -= new EventHandler<CurrentKeyboardChangingEventArgs>( OnCurrentKeyboardChanging );
            KeyboardContext.Service.CurrentKeyboardChanged -= new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
        }

        void OnCurrentKeyboardChanging( object sender, CurrentKeyboardChangingEventArgs e )
        {
            if( e.Current != null ) ChangeActiveCurrentKeyboardStatus( false );
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( e.Current != null ) ChangeActiveCurrentKeyboardStatus(true);
        }

        #endregion

        #region IHaveDefaultHelp Members

        public Stream GetDefaultHelp()
        {
            return typeof( MainKeyboardManager ).Assembly.GetManifestResourceStream( "SimpleSkin.Res.helpcontent.zip" );
        }

        #endregion
    }


}
