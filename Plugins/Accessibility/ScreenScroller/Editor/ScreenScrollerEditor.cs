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
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;

namespace ScreenScroller.Editor
{

    [Plugin( ScreenScrollerEditor.PluginIdString,
           PublicName = ScreenScrollerEditor.PluginPublicName,
           Version = ScreenScrollerEditor.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScreenScrollerEditor : IPlugin
    {
        internal const string PluginIdString = "{652CFF65-5CF7-4FE9-8FF5-45C5E2A942E6}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Screen scroller Editor";

        EditorViewModel _editor;
        EditorView _window;

        [ConfigurationAccessor( ScreenScrollerPlugin.PluginIdString )]
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
