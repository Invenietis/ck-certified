using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
