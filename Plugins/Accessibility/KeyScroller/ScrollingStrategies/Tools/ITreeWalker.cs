﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace Scroller
{
    public interface ITreeWalker
    {
        /// <summary>
        /// Get the current parents stack
        /// </summary>
        Stack<IHighlightableElement> Parents { get; }

        /// <summary>
        /// Get the sibblings of the current element
        /// </summary>
        ICKReadOnlyList<IHighlightableElement> Sibblings { get; }

        /// <summary>
        /// Get the current element of the walker
        /// </summary>
        IHighlightableElement Current { get; }

        /// <summary>
        /// Move the walker to the next sibbling.
        /// </summary>
        /// <returns>false if there no next sibbling</returns>
        bool MoveNext();

        /// <summary>
        /// Move the walker to the first sibbling
        /// </summary>
        /// <returns></returns>
        void MoveFirst();

        /// <summary>
        /// Move the walker to the last sibbling
        /// </summary>
        /// <returns></returns>
        void MoveLast();

        /// <summary>
        /// Move the walker to the first child
        /// </summary>
        /// <returns>false if there no children</returns>
        bool EnterChild();

        /// <summary>
        /// Move the walker to the parent
        /// </summary>
        /// <returns>false if there no parent</returns>
        bool UpToParent();

        /// <summary>
        /// Move the walker to the given element
        /// </summary>
        /// <param name="element"></param>
        void GoTo(IHighlightableElement element);

        /// <summary>
        /// Move the walker to the absolute root
        /// </summary>
        void GoToAbsoluteRoot();

        /// <summary>
        /// move the cursor to the relative root (the root modul)
        /// </summary>
        void GoToRelativeRoot();
    }
}