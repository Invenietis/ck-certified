using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Predictor.Model
{
    /// <summary>
    /// A token describes a group of symbols.
    /// </summary>
    public interface IToken
    {
        string Value { get; }
    }
}
