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

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Core;
using CK.Context;

namespace SimpleSkin.ViewModels
{
    internal class VMContextSimple : VMContext<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple, VMKeyModeSimple, VMLayoutKeyModeSimple>
    {
        public VMContextSimple( IContext ctx, IKeyboardContext kbctx, IPluginConfigAccessor config )
            : base( ctx, kbctx.Keyboards.Context, config, config )
        {
        }

        protected override VMKeySimple CreateKey( IKey k )
        {
            return new VMKeySimple( this, k );
        }

        protected override VMZoneSimple CreateZone( IZone z )
        {
            return new VMZoneSimple( this, z );
        }

        protected override VMKeyboardSimple CreateKeyboard( IKeyboard kb )
        {
            return new VMKeyboardSimple( this, kb );
        }

        protected override VMKeyModeSimple CreateKeyMode( IKeyMode km )
        {
            return null;
            //return new VMKeyModeSimple(this, km);
        }

        protected override VMLayoutKeyModeSimple CreateLayoutKeyMode( ILayoutKeyMode lkm )
        {
            return null;
            //return new VMLayoutKeyModeSimple( this, lkm );
        }
    }
}
