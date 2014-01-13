#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMContextSimple.cs) is part of CiviKey. 
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

using CK.Keyboard.Model;
using CK.Plugin.Config;
using System;

namespace SimpleSkin.ViewModels.Versionning
{
    internal static class V150To160
    {
        internal static void EnsureKeyVersion( IPluginConfigAccessor simpleSkinConfigAccessor, IKey k )
        {
            if( simpleSkinConfigAccessor[k] != null )
            {
                object o = simpleSkinConfigAccessor[k.CurrentLayout.LayoutKeyModes.FindBest( k.Context.EmptyMode )]["[PluginDataVersion]"];
                if( o != null )
                {
                    Version v;
                    if( Version.TryParse( o.ToString(), out v ) )
                    {
                        if( v < new Version( "1.6.0" ) )
                        {
                            Key150To160( simpleSkinConfigAccessor, k );
                        }
                    }
                }
            }
        }

        private static void Key150To160( IPluginConfigAccessor simpleSkinConfigAccessor, IKey k )
        {
            //Prior to SimpleSkin plugin v1.6.0 the images were stored in the plugin datas of the layoutkeymodes
            //Since this version, the images are on the keymodes
            foreach( var layoutKeyMode in k.CurrentLayout.LayoutKeyModes )
            {
                PropertyMigrationLayoutKeyModeToKeyMode( simpleSkinConfigAccessor, layoutKeyMode, "Image" );
                PropertyMigrationLayoutKeyModeToKeyMode( simpleSkinConfigAccessor, layoutKeyMode, "DisplayType" );
            }
        }

        private static void PropertyMigrationLayoutKeyModeToKeyMode( IPluginConfigAccessor simpleSkinConfigAccessor, ILayoutKeyMode lkm, string propertyName )
        {
            object obj = simpleSkinConfigAccessor[lkm][propertyName];
            simpleSkinConfigAccessor[lkm].Remove( propertyName );
            if( obj != null )
            {
                IKeyboardMode mode = lkm.Mode;
                IKeyMode bestMatch = lkm.Key.KeyModes.FindBest( mode );
                if( bestMatch.Mode.ContainsAll( mode ) && mode.ContainsAll( bestMatch.Mode ) )
                {
                    simpleSkinConfigAccessor[bestMatch].Set( propertyName, obj );
                }
            }
        }
    }
}
