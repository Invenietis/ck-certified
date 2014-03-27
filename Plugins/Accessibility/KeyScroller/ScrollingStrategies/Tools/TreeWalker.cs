using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace KeyScroller
{
    public class Walker : ITreeWalker
    {
        Stack<IHighlightableElement> _stack; 

        public Walker()
        {
            _stack = new Stack<IHighlightableElement>();
        }

        protected virtual ICKReadOnlyList<IHighlightableElement> GetSibblings()
        {
            return Peek() != null ? Peek().Children : null;
        }

        protected virtual IHighlightableElement Peek()
        {
            return _stack.Count > 0 ? _stack.Peek() : null;
        }
        
        #region ITreeWalker Members

        public IHighlightableElement Current
        {
            get;
            private set;
        }

        public virtual bool MoveNext()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            if( sibblings == null || sibblings.Count == 1 ) //false if there is no parent or there are no sibblings at all
                return false;

            int idx = sibblings.IndexOf( Current );

            if( idx < 0 ) throw new InvalidOperationException("Something goeas wrong : the current element is not contains by its parent");

            //The current child is the last one
            if( idx + 1 >= sibblings.Count ) return false;

            Current = sibblings.ElementAt( idx + 1 );
            return true;
        }

        public virtual void MoveFirst()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            Current = sibblings == null ? Current : sibblings[0];
        }

        public virtual void MoveLast()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            Current = sibblings == null ? Current : sibblings[sibblings.Count - 1];
        }

        public virtual bool EnterChild()
        {
            if( Current.Children.Count == 0 ) return false;
            
            _stack.Push( Current );
            Current = Current.Children[0];
            return true;
        }

        public virtual bool UpToParent()
        {
            if( Peek() == null ) return false;

            Current = _stack.Pop();
            return true;
        }

        public virtual void GoTo( HighlightModel.IHighlightableElement element )
        {
            if( element == null ) throw new ArgumentNullException( "element" );
            _stack.Clear();
            Current = element;
        }
        #endregion
    }
}
