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
using System;

namespace SimpleSkin.ViewModels
{
    internal class VMContextSimple : VMContext<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
    {
        IPluginConfigAccessor _config;
        public IPluginConfigAccessor Config
        {
            get
            {
                if( _config == null ) _config = Context.GetService<IPluginConfigAccessor>( true );
                return _config;
            }
        }

        public VMContextSimple( IContext ctx, IKeyboardContext kbctx )
            : base( ctx, kbctx.Keyboards.Context )
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
    }
}
