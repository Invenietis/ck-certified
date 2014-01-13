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
