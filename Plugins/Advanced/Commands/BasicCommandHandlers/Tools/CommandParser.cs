#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\Commands\BasicCommandHandlers\Tools\CommandParser.cs) is part of CiviKey. 
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
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Define all necessary operations for a string parser.
    /// </summary>
    public sealed class CommandParser
    {
        /// <summary>
        /// Token's identifiers.
        /// </summary>
        public enum Token
        {
            None,
            Plus,
            Minus,
            IsolatedChar,
            Mult,
            Div,
            Number,
            OpenPar,
            ClosePar,
            Error,
            EndOfInput,
            Identifier,
            Equal,
            String
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="CommandParser"/> class.
        /// Create a parser object from a string. It initialize the parsing at the first character of the string.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        public CommandParser(string s)
            : this(s, 0)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandParser"/> class.
        /// Create a parser object from a string. It initialize the parsing at the index selected by the user.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="startIndex">Index of the first character for parsing.</param>
        public CommandParser(string s, int startIndex)
        {
            _curToken = Token.None;
            _toParse = s;
            _pos = startIndex;
            _buffer = new StringBuilder();
            GetNextToken();
        }

        /// <summary>
        /// Gets the current token of the string.
        /// </summary>
        public Token CurrentToken
        {
            get { return _curToken; }
        }

        /// <summary>
        /// Gets a numeric value created from the a string of digit.
        /// </summary>
        public double NumValue
        {
            get { return _numValue; }
        }

        /// <summary>
        /// Gets the identifier, use only if the current token is an indentifier.
        /// </summary>
        public string Identifier
        {
            get { return _strVal; }
        }

        /// <summary>
        /// Gets the string value, use only if the current tokent is a string.
        /// </summary>
        public string StringValue
        {
            get { return _strVal; }
        }

        /// <summary>
        /// Determines whether the current token is an identifier and returns it.
        /// </summary>
        /// <param name="identifier">Out : the identifier value.</param>
        /// <returns>Return true if the token is an identifier otherwise false.</returns>
        public bool IsIdentifier(out string identifier)
        {
            identifier = _strVal;
            if (_curToken == Token.Identifier)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether a string is the wanted identifier.
        /// </summary>
        /// <param name="identifier">The wanted identifier.</param>
        /// <returns>True if the identifiers match otherwise false.</returns>
        public bool MatchIdentifier(string identifier)
        {
            if (_curToken == Token.Identifier
                && CaseInsensitiveComparer.Default.Compare(identifier, _strVal) == 0)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether a char is the wanted IsolatedChar.
        /// </summary>
        /// <param name="c">The char to test</param>
		/// <returns>True if the char is the wanted IsolatedChar otherwise false.</returns>
        public bool MatchIsolatedChar( char c )
        {
            if ( _curToken == Token.IsolatedChar && c == _cIsolated )
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether the current togen is an IsolatedChar and return it.
        /// </summary>
        /// <param name="c">Out : the IsolatedChar value.</param>
        /// <returns>True if the current token is an IsolatedChar otherwise false.</returns>
        public bool IsIsolatedChar(out char c)
        {
            c = _cIsolated;
            if ( _curToken == Token.IsolatedChar )
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether the current token is a string and return it.
        /// </summary>
        /// <param name="s">Out : the string value.</param>
        /// <returns>True if the current token is a string otherwise false.</returns>
        public bool IsString(out string s)
        {
            s = _strVal;
            if (_curToken == Token.String)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether a token matches with the current one.
        /// </summary>
        /// <param name="t">The searched token.</param>
        /// <returns>True if both token matches, otherwise false.</returns>
        public bool Match(Token t)
        {
            if (_curToken == t)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine wether the current token is a number and return it.
        /// </summary>
        /// <param name="numValue">Out : the number value.</param>
        /// <returns>True wether the token is a number otherwise false.</returns>
        public bool IsNumber(out double numValue)
        {
            numValue = _numValue;
            if (_curToken == Token.Number)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the next token and determine its value.
        /// </summary>
        /// <returns>The next token's value.</returns>
        public Token GetNextToken()
        {
            if (IsEnd) return _curToken = Token.EndOfInput;
            char c = Read();
            while (Char.IsWhiteSpace(c))
            {
                if (IsEnd) return _curToken = Token.EndOfInput;
                c = Read();
            }
            switch (c)
            {
                case '+': _curToken = Token.Plus; break;
                case '-': _curToken = Token.Minus; break;
                case '*': _curToken = Token.Mult; break;
                case '/': _curToken = Token.Div; break;
                case '=': _curToken = Token.Equal; break;
                case '(': _curToken = Token.OpenPar; break;
                case ')': _curToken = Token.ClosePar; break;
                case '"':
                    {
                        _curToken = Token.String;
                        _buffer.Length = 0;
                        while (!IsEnd && (c = Read()) != '"')
                        {
                            if (c == '\\' && !IsEnd) c = Read();
                            _buffer.Append(c);
                        }
                        _strVal = _buffer.ToString();
                        break;
                    }
                default:
                    {
                        if (Char.IsDigit(c))
                        {
                            _curToken = Token.Number;
                            double val = (int)(c - '0');
                            while (!IsEnd && Char.IsDigit(c = Peek()))
                            {
                                val = val * 10 + (int)(c - '0');
                                Forward();
                            }
                            _numValue = val;
                        }
                        else if (Char.IsLetter(c) || c == '_')
                        {
                            _curToken = Token.Identifier;
                            _buffer.Length = 0;
                            _buffer.Append(c);
                            while (!IsEnd && (Char.IsLetterOrDigit(c = Peek())
                                    || c == '_'))
                            {
                                _buffer.Append(c);
                                Forward();
                            }
                            _strVal = _buffer.ToString();
                        }
                        else
                        {
                            _curToken = Token.IsolatedChar;
                            _cIsolated = c;
                        }
                        break;
                    }
            }
            return _curToken;
        }

        #region Implementation
        /// <summary>
        /// The string to parse.
        /// </summary>
        string _toParse;

        /// <summary>
        /// The current position in the string.
        /// </summary>
        int _pos;

        /// <summary>
        /// The current token found by the parser.
        /// </summary>
        Token _curToken;

        /// <summary>
        /// The last numeric value found by the parser.
        /// </summary>
        double _numValue;

        /// <summary>
        /// The last string found by the parser.
        /// </summary>
        string _strVal;

        /// <summary>
        /// The last isolated char found by the parser.
        /// </summary>
        char _cIsolated;

        /// <summary>
        /// Buffer used to build a string value or an identifier.
        /// </summary>
        StringBuilder _buffer;

        /// <summary>
        /// Peek the current character in the string to parse.
        /// </summary>
        /// <returns>The current character.</returns>
        char Peek()
        {
            return _toParse[_pos];
        }

        /// <summary>
        /// Peek the current character and go to the next.
        /// </summary>
        /// <returns>The current character.</returns>
        char Read()
        {
            return _toParse[_pos++];
        }

        /// <summary>
        /// Reach the next character.
        /// </summary>
        void Forward()
        {
            ++_pos;
        }

        /// <summary>
		/// Gets a value indicating whether wether the parser reached the end of the string to parse.
        /// </summary>
        bool IsEnd
        {
            get { return _pos >= _toParse.Length; }
        }

        #endregion

        #region AutoTest

#if DEBUG
        static bool _autoTest = AutoTest();

        static void CheckIdentifier(string s, int idPos, string identifier)
        {
            CommandParser p = new CommandParser(s);
            while (idPos-- > 0) p.GetNextToken();
            Debug.Assert(p.CurrentToken == CommandParser.Token.Identifier
                && p.Identifier == identifier);
        }

        static void CheckString(string s, int idPos, string strVal)
        {
            CommandParser p = new CommandParser(s);
            while (idPos-- > 0) p.GetNextToken();
            Debug.Assert(p.CurrentToken == CommandParser.Token.String
                && p.StringValue == strVal);
        }

        static void CkeckNotIdentifier(string s, int idNotPos)
        {
            CommandParser p = new CommandParser(s);
            while (idNotPos-- > 0) p.GetNextToken();
            Debug.Assert(p.CurrentToken != CommandParser.Token.Identifier);
        }

        static void ParseAll(string s)
        {
            CommandParser p = new CommandParser(s);
            while (p.CurrentToken != CommandParser.Token.EndOfInput) p.GetNextToken();
        }

        static bool AutoTest()
        {
            CheckIdentifier("Toto", 0, "Toto");
            CheckIdentifier("_Toto+7", 0, "_Toto");
            CheckIdentifier("_Toto", 0, "_Toto");
            CheckIdentifier("   _T12o_to    ", 0, "_T12o_to");
            CheckIdentifier(" 129 + 233  _1234 llom   ", 3, "_1234");
            CheckIdentifier(" 129 + 233  _1234 llom   ", 4, "llom");
            CkeckNotIdentifier(" 129 + 233  _1234 llom   ", 2);
            CheckString("\"val\" titi", 0, "val");
            CheckString("a \"string val\" titi", 1, "string val");
            CheckIdentifier("a \"string val\" titi", 2, "titi");
            CheckString("123 k \"kjkj h\\\"jejhe\" ", 2, "kjkj h\"jejhe");
            CheckString("123 k \"\" ", 2, "");
            ParseAll("a b 223 \"");
            CheckString("123 k \"", 2, "");
            CheckString("123 k \"\\\"", 2, "\"");
            CheckString("123 k \"\\\"a", 2, "\"a");
            return true;
        }
#endif

        #endregion

    }

}
