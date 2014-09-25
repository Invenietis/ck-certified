#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\TextualContext\TextualContextCommandHandler.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{B2A76BF2-E9D2-4B0B-ABD4-270958E17DA0}", PublicName = "TextualContext - Command Handler", Categories = new string[] { "Prediction" } )]
    public class TextualContextCommandHandler : BasicCommandHandler, ICommandTextualContextService
    {
        public const string CMDClearTextualContext = "clearTextualContext";

        public event EventHandler TextualContextClear;

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.Contains( CMDClearTextualContext ) )
            {
                ClearTextualContext();
            }
        }

        public void ClearTextualContext()
        {
            if( TextualContextClear != null )
                TextualContextClear( this, EventArgs.Empty );
        }
    }
}
