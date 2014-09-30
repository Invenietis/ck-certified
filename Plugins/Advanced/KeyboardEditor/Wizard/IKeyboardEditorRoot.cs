#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\Wizard\IKeyboardEditorRoot.cs) is part of CiviKey. 
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

using CK.Context;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;
using KeyboardEditor.ViewModels;
using ProtocolManagerModel;

namespace KeyboardEditor
{
    public interface IKeyboardEditorRoot : IKeyboardBackupManager
    {
        /// <summary>
        /// Gets the Keyboardcontext service implementation
        /// Gives access to all the keyboards and their layouts, zones, keys etc..
        /// </summary>
        IService<IKeyboardContext> KeyboardContext { get; }

        /// <summary>
        /// Gets the Keyboardcontext service implementation
        /// Gives access to all the keyboards and their layouts, zones, keys etc..
        /// </summary>
        IService<IProtocolEditorsManager> ProtocolManagerService { get; }

        /// <summary>
        /// Gets the Context.
        /// Gives access to the application's inner management
        /// </summary>
        IContext Context { get; }

        /// <summary>
        /// Gets the <see cref="IPluginConfigAccessor"/> linked ot the configuration of the skin plugin
        /// </summary>
        IPluginConfigAccessor SkinConfiguration { get; }

        /// <summary>
        /// Gets the <see cref="IPluginConfigAccessor"/> linked ot the configuration of the keyboard editor
        /// </summary>
        IPluginConfigAccessor Config { get; }

        /// <summary>
        /// Gets the PointerDeviceDriver service , is used to hook mouse events to enable drag n drop for keys
        /// </summary>
        IService<IPointerDeviceDriver> PointerDeviceDriver { get; }

        /// <summary>
        /// Gets the Viewmodels of the keyboard that is being edited.
        /// Used to trigger Dispose on viewmodels when stopping the plugin
        /// </summary>
        VMContextEditable EditedContext { get; set; }

        void ShowHelp();

        /// <summary>
        /// Saves the Context and the UserConfiguration
        /// </summary>
        void Save();
    }
}
