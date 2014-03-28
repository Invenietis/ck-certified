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
        IHighlightableElement _root;

        public Stack<IHighlightableElement> Parents { get; private set; } 

        public Walker(IHighlightableElement root)
        {
            Parents = new Stack<IHighlightableElement>();
            _root = root;
        }

        protected virtual ICKReadOnlyList<IHighlightableElement> GetSibblings()
        {
            return Peek() != null ? Peek().Children : null;
        }

        protected virtual IHighlightableElement Peek()
        {
            return Parents.Count > 0 ? Parents.Peek() : null;
        }
        
        #region ITreeWalker Members

        public IHighlightableElement Current
        {
            get;
            protected set;
        }

        public virtual bool MoveNext()
        {
            ICKReadOnlyList<IHighlightableElement> sibblings = GetSibblings();
            if( sibblings == null || sibblings.Count == 1 ) //false if there is no parent or there are no sibblings at all
                return false;

            int idx = sibblings.IndexOf( Current );

            if( idx < 0 ) throw new InvalidOperationException("Something goes wrong : the current element is not contained by its parent !");

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

            Parents.Push( Current );
            Current = Current.Children[0];
            return true;
        }

        public virtual bool UpToParent()
        {
            if( Peek() == null ) return false;

            Current = Parents.Pop();
            return true;
        }

        public virtual void GoTo( HighlightModel.IHighlightableElement element )
        {
            if( element == null ) throw new ArgumentNullException( "element" );
            Parents.Clear();

            if(element != _root) Parents.Push( _root );

            Current = element;
        }

        public void GoToAbsoluteRoot()
        {
            GoTo( _root );
        }

        public void GoToRelativeRoot()
        {
            while( Parents.Count > 1 )
            {
                Current = Parents.Pop();
            }

            //Check if that the founded element in the parent stack is really a root element. Get the absolute root if not.
            if( !Current.IsHighlightableTreeRoot )
                GoToAbsoluteRoot();
        }
        #endregion
    }
}
