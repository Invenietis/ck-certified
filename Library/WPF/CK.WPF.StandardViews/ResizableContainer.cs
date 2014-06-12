using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CK.Windows.Core;
using CK.Windows.Interop;

namespace CK.WPF.StandardViews
{
    public class ResizableContainer : Control, ISpecialElementHitTest
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register( "Content", typeof( object ), typeof( ResizableContainer ) );
        public object Content
        {
            get { return GetValue( ContentProperty ); }
            set { SetValue( ContentProperty, value ); }
        }

        #region ISpecialElementHitTest Members

        public int GetHitTestResult( Point p, int currentResult, DependencyObject hitObject )
        {
            var rect = hitObject as Rectangle; 
            switch( rect.Name )
            {
                case "top":
                    return Win.HTTOP;
                case "bottom":
                    return Win.HTBOTTOM;
                case "left":
                    return Win.HTLEFT;
                case "right":
                    return Win.HTRIGHT;
                case "topLeft":
                    return Win.HTTOPLEFT;
                case "topRight":
                    return Win.HTTOPRIGHT;
                case "bottomLeft":
                    return Win.HTBOTTOMLEFT;
                case "bottomRight":
                    return Win.HTBOTTOMRIGHT;
            }
            return currentResult;
        }

        #endregion
    }
}
