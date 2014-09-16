#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WordPredictor.Model\ITextualContextService.cs) is part of CiviKey. 
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
using System.Collections.Specialized;
using CK.Core;
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
        /// <summary>
        /// This event is fired when the textual context is going to change
        /// </summary>
        event EventHandler TextualContextChanging;

        /// <summary>
        /// This event is fired when the textual context has changed.
        /// </summary>
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