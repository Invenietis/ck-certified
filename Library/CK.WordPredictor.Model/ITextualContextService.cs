using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Specialized;
using System.ComponentModel;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public enum CaretPosition
    {
        OutsideToken,
        InsideToken,
        StartToken,
        EndToken
    };

    public interface ITokenCollection : INotifyCollectionChanged, ICKReadOnlyList<IToken>
    {
    }

    public interface ITextualContextService : CK.Core.IFluentInterface, IDynamicService
    {
        event EventHandler TextualContextChanging;

        event EventHandler TextualContextChanged;

        /// <summary>
        /// Gets an observable list of <see cref="IToken"/> that surrounds the caret (the current insertion point).
        /// </summary>
        ITokenCollection Tokens { get; }

        /// <summary>
        /// Gets the index in <see cref="Tokens"/> of the token where the caret is.
        /// </summary>
        int CurrentTokenIndex { get; }

        /// <summary>
        /// Gets the current token. Null if <see cref="CurrentPosition"/> is <see cref="CaretPosition.OutsideToken"/>.
        /// </summary>
        IToken CurrentToken { get; }

        /// <summary>
        /// Index of the caret inside the <see cref="CurrentToken"/>. It is 0 when <see cref="CurrentPosition"/> is <see cref="CaretPosition.StartToken"/>, 
        /// it is equal to the token length when <see cref="CurrentPosition"/> is <see cref="CaretPosition.EndToken"/> and is 
        /// equal to -1 when <see cref="CurrentPosition"/> is <see cref="CaretPosition.OutsideToken"/>.
        /// </summary>
        int CaretOffset { get; }

        /// <summary>
        /// </summary>
        CaretPosition CurrentPosition { get; }
    }
}