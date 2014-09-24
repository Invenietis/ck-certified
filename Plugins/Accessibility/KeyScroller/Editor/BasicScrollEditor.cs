#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\Editor\BasicScrollEditor.cs) is part of CiviKey. 
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
using Caliburn.Micro;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

namespace Scroller.Editor
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class BasicScrollEditor : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{48D3977C-EC26-48EF-8E47-806E11A1C041}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Scroller editor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        EditorViewModel _editor;

        [RequiredService]
        public IContext Context { get; set; }

        [ConfigurationAccessor( "{84DF23DC-C95A-40ED-9F60-F39CD350E79A}" )] //ScrollerPlugin
        public IPluginConfigAccessor BasicScrollConfiguration { get; set; }

        [ConfigurationAccessor( "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}" )]
        public IPluginConfigAccessor KeyboardTriggerConfiguration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITriggerService TriggerService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerInput { get; set; }
        
        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IWindowManager wnd = IoC.Get<WindowManager>();
            _editor = new EditorViewModel( BasicScrollConfiguration, KeyboardTriggerConfiguration, TriggerService ) { Context = Context };
            wnd.ShowWindow( _editor );
        }

        public void Stop()
        {
            _editor.Stopping = true;
            _editor.TryClose();
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
