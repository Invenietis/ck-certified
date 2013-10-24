using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using HighlightModel;
using KeyScroller;

namespace MouseRadar
{
    public enum RootState
    {
        None = 0,
        Highlighted,
        Selected,
    }

    public class RootElement : IHighlightableElement
    {
        List<IHighlightableElement> _children;
        ICKReadOnlyList<IHighlightableElement> _sealedChildren;
        
        public RootState State { get; private set; }

        public RootElement()
        {
            _children = new List<IHighlightableElement>();
            State = RootState.Highlighted;
            _sealedChildren = new CKReadOnlyListMono<IHighlightableElement>( new ChildElement() );
        }

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return _sealedChildren; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        #endregion
    }

    public class ChildElement : IActionnableElement
    {
        #region IActionnableElement Members

        public ActionType ActionType
        {
            get
            {
                return ActionType.StayOnTheSame;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }

        #endregion
    }
}
