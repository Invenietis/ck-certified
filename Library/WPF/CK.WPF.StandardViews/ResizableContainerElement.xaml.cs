using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Windows.Core;
using CK.Windows.Interop;

namespace CK.WPF.StandardViews
{
    public partial class ResizableContainerElement : UserControl, IHitTestElementController
    {
        public static readonly DependencyProperty ResizeTypeProperty = DependencyProperty.Register( "ResizeType", typeof( string ), typeof( ResizableContainerElement ) );
        public string ResizeType
        {
            get { return GetValue( ResizeTypeProperty ).ToString(); }
            set { SetValue( ResizeTypeProperty, value ); }
        }

        #region IHitTestElementController Members

        public int GetHitTestResult( Point p, int currentResult, DependencyObject hitObject )
        {
            switch( ResizeType )
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

        protected override System.Windows.Media.HitTestResult HitTestCore( System.Windows.Media.PointHitTestParameters hitTestParameters )
        {
            return new PointHitTestResult( this, hitTestParameters.HitPoint );
        }
    }
}
