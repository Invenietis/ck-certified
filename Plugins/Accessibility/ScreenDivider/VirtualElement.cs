using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HighlightModel;
using KeyScroller;
using CK.Core;
namespace ScreenDivider
{
    public class VirtualElement : IActionnableElement
    {
        public VirtualElement()
        {
            ActionType = ActionType.Normal;
        }
        #region IActionnableElement Members

        public ActionType ActionType { get; set; }

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
