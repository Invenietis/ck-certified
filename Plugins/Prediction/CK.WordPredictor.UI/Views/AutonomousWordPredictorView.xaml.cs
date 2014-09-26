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
using CK.Windows;
using CK.Windows.Core;
using SimpleSkin.Helpers;

namespace CK.WordPredictor.UI.Views
{
    /// <summary>
    /// Interaction logic for AutonomousWordPredictorView.xaml
    /// </summary>
    public partial class AutonomousWordPredictorView : CKNoFocusWindow
    {
        public AutonomousWordPredictorView( NoFocusManager noFocusManager )
            : base( noFocusManager )
        {
            InitializeComponent();
        }

        protected override bool IsDraggableVisual( DependencyObject visualElement )
        {
            FrameworkElement border = visualElement as FrameworkElement;
            //Allows drag and drop when the background is set
            if( border != null && border.Name == "InsideBorder" ) return true;
            if( DraggableVisualAttachedProperty.GetDraggableVisual( visualElement ) ) return true;
            var parent = VisualTreeHelper.GetParent( visualElement );
            return parent is AutonomousWordPredictorView || base.IsDraggableVisual( visualElement );
        }

        private void predicbutton_Click( object sender, RoutedEventArgs e )
        {
            Console.WriteLine("");
        }

        private void CKNoFocusWindow_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            Console.WriteLine( "" );
        }

        private void Viewbox_MouseEnter( object sender, MouseEventArgs e )
        {
            Console.WriteLine( "" );
        }

        protected override bool EnableHitTestElementController( DependencyObject visualElement, Point p, int currentHTCode, out IHitTestElementController specialElement )
        {
            specialElement = visualElement as IHitTestElementController;
            return specialElement != null;
        }
    }
}
