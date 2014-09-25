#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\WordPredicted\WordPredictedService.cs) is part of CiviKey. 
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
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{669622D4-4E7E-4CCE-96B1-6189DC5CD5D6}", PublicName = "WordPredictedService", Categories = new string[] { "Advanced", "Prediction" } )]
    public class WordPredictedService : BasicCommandHandler, IWordPredictedService
    {
        public const string CMDSendPredictedWord = "sendPredictedWord";

        public event EventHandler<WordPredictionSuccessfulEventArgs> WordPredictionSuccessful;

        public void WordHasBeenChosen( string word )
        {
            if( WordPredictionSuccessful != null )
                WordPredictionSuccessful( this, new WordPredictionSuccessfulEventArgs( word ) );
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.StartsWith( CMDSendPredictedWord + ":" ) )
            {
                WordHasBeenChosen( e.Command.Substring( CMDSendPredictedWord.Length + 1 ) );
            }
        }
    }
}
