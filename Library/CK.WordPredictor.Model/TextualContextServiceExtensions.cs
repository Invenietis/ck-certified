#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\TextualContextServiceExtensions.cs) is part of CiviKey. 
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
using System.Linq;

namespace CK.WordPredictor.Model
{
    public static class TextualContextServiceExtensions
    {
        public static string GetTextualContext( this ITextualContextService textualService )
        {
            string rawContext;
            CaretPosition currentPosition = textualService.CurrentPosition;
            if( textualService.Tokens.Count > 1 )
            {
                int tokensToTake =  textualService.CurrentTokenIndex;
                if( currentPosition == CaretPosition.OutsideToken )
                {
                    tokensToTake = textualService.Tokens.Count;
                }

                if( currentPosition == CaretPosition.EndToken )
                {
                    tokensToTake++;
                }

                rawContext = String.Join( " ", textualService.Tokens.Take( tokensToTake ).Select( t => t.Value ) );

                if( currentPosition == CaretPosition.InsideToken )
                {
                    rawContext += " " + textualService.CurrentToken.Value.Substring( 0, textualService.CaretOffset );
                }
                // If we are on the beginning of a word then add a space separator to the raw context
                if( currentPosition == CaretPosition.StartToken )
                {
                    rawContext += " ";
                }
                // If we are outside of a token (after a space separator) add a space separator to the raw context
                if( currentPosition == CaretPosition.OutsideToken )
                {
                    rawContext += " ";
                }
                return rawContext;
            }
            if( textualService.Tokens.Count == 1 )
            {
                if( currentPosition == CaretPosition.InsideToken )
                {
                    return textualService.CurrentToken.Value.Substring( 0, textualService.CaretOffset );
                }

                if( currentPosition == CaretPosition.OutsideToken )
                {
                    return textualService.Tokens[0].Value + " ";
                }

                return textualService.CurrentToken.Value;
            }

            return String.Empty;
        }
    }
}
