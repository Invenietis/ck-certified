using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace KeyScroller
{
    public class TreeWalker : ITreeWalker
    {
        Stack<IHighlightableElement> _stack; 

        public TreeWalker()
        {
            _stack = new Stack<IHighlightableElement>();
        }

        ICKReadOnlyList<IHighlightableElement> GetSibblings()
        {
            return Peek() != null ? Peek().Children : null;
        }

        IHighlightableElement Peek()
        {
            return _stack.Count > 0 ? _stack.Peek() : null;
        }
        
        #region ITreeWalker Members

        public IHighlightableElement Current
        {
            get;
            private set;
        }

        public bool MoveNext()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            if( sibblings == null || sibblings.Count == 1 ) //false if there is no parent or there are no sibblings at all
                return false;

            int idx = sibblings.IndexOf( Current );

            if( idx == -1 ) throw new InvalidOperationException("Something goeas wrong : the current element is not contains by its parent");

            //The current child is the last one
            if( idx + 1 >= sibblings.Count ) return false;

            Current = sibblings.ElementAt( idx + 1 );
            return true;
        }

        public void MoveFirst()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            Current = sibblings == null ? Current : sibblings[0];
        }

        public void MoveLast()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            Current = sibblings == null ? Current : sibblings[sibblings.Count - 1];
        }

        public bool EnterChild()
        {
            if( Current.Children.Count == 0 ) return false;
            
            _stack.Push( Current );
            Current = Current.Children[0];
            return true;
        }

        public bool UpToParent()
        {
            if( Peek() == null ) return false;

            Current = _stack.Pop();
            return true;
        }

        public void GoTo( HighlightModel.IHighlightableElement element )
        {
            if( element == null ) throw new ArgumentNullException( "element" );
            _stack.Clear();
            Current = element;
        }

        #endregion
    }
}
