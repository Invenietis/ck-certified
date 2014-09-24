#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\MouseRadar\Editor\MouseRadarEditor.cs) is part of CiviKey. 
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

namespace MouseRadar.Editor
{

    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadarEditor : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{275B0E68-B880-463A-96E5-342C8E31E229}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Radar Editor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        EditorViewModel _editor;

        [ConfigurationAccessor( "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}" )] //MouseRadarPlugin
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardDriver KeyboardHook { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerInput { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IWindowManager wnd = IoC.Get<WindowManager>();
            _editor = new EditorViewModel( Configuration ) { Context = Context };
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
