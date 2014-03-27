using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HighlightModel;

namespace KeyScroller
{
    public interface ITreeWalker
    {
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
    }
}
