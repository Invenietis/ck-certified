using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;

namespace Scroller
{
    public class TreeWalker : ITreeWalker
    {
        protected IHighlightableElement Root;

        public Stack<IHighlightableElement> Parents { get; private set; }

        public TreeWalker( IHighlightableElement root )
        {
            Parents = new Stack<IHighlightableElement>();
            Root = root;
        }

        public virtual ICKReadOnlyList<IHighlightableElement> Sibblings
        {
            get { return Peek() != null ? Peek().Children : CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
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
            if( Sibblings.Count <= 1 ) //false if there are no sibblings at all
                return false;

            int idx = Sibblings.IndexOf( Current );

            if( idx < 0 )
            {
                //13/05/2014 : Modules can be unregistered through an interface. There is a possibility of concurrency actions when a move next is called on an element that has just been unregistered (and the information hasn't been propagated yet)
                //Therefor, we cannot take this as a bug
                //The Debug.Assert makes sure that the root of the element (the detached current) is no longer registered
                //throw new InvalidOperationException("Something is wrong : the current element is not contained in its parent !");
#if DEBUG
                if( Parents.Count > 0 )
                {
                    while( Parents.Count > 1 ) Parents.Pop();
                    Debug.Assert( !Root.Children.Contains( Parents.Single() ) );
                }
#endif
                GoToAbsoluteRoot();

            }

            //The current child is the last one
            if( idx + 1 >= Sibblings.Count ) return false;

            Current = Sibblings.ElementAt( idx + 1 );
            return true;
        }

        public virtual void MoveFirst()
        {
            Current = Sibblings.Count == 0 ? Current : Sibblings[0];
        }

        public virtual void MoveLast()
        {
            Current = Sibblings.Count == 0 ? Current : Sibblings[Sibblings.Count - 1];
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

            if( element != Root ) Parents.Push( Root );

            Current = element;
        }

        public void GoToAbsoluteRoot()
        {
            GoTo( Root );
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
