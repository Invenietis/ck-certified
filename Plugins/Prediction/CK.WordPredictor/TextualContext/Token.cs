using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.WordPredictor.Model;

namespace CK.WordPredictor
{
    internal class Token : IToken
    {
        public Token( string v )
        {
            Value = v;
        }
        public string Value { get; set; }
    }
}
