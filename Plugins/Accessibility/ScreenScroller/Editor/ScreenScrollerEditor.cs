#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\ScreenScroller\Editor\ScreenScrollerEditor.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;
using CK.Context;
using CK.Core;

namespace ScreenScroller.Editor
{

    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScreenScrollerEditor : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{652CFF65-5CF7-4FE9-8FF5-45C5E2A942E6}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Screen Scroller Editor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        EditorViewModel _editor;
        EditorView _window;

        [ConfigurationAccessor( "{AE25D80B-B927-487E-9274-48362AF95FC0}" )] //ScreenScrollerPlugin
        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _editor = new EditorViewModel( Configuration, Context );
            _window = new EditorView() { DataContext = _editor };
            _window.Show();
        }

        public void Stop()
        {
            _window.Close();
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
