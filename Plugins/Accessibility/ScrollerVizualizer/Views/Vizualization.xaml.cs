using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Windows;

namespace ScrollerVizualizer
{
    /// <summary>
    /// Interaction logic for Vizualization.xaml
    /// </summary>
    public partial class Vizualization : CKWindow
    {
        public Vizualization(VizualizationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            this.Top = 0;
            Left = 0;
            Storyboard tIn = (Storyboard) this.Resources["toggleIn"];
            Storyboard tOut = (Storyboard) this.Resources["toggleOut"];
            StackPanel sp = (StackPanel) FindName( "subPanel" );
            
            //vm.ToggleIn += ( o, e ) => tIn.Begin();
            //vm.ToggleOut += ( o, e ) => tOut.Begin();
        }

        protected override void OnChildDesiredSizeChanged( UIElement child )
        {
            base.OnChildDesiredSizeChanged( child );
          
            
            Console.WriteLine( child.GetValue(FrameworkElement.NameProperty));       
        }

        protected override bool IsDraggableVisual( DependencyObject visualElement )
        {
            if( visualElement is ButtonBase || visualElement.FindParent( x => x is ButtonBase ) != null )
            {

                return false;
            }
            return true;
        }

        private void ToggleButton_Click( object sender, RoutedEventArgs e )
        {

        }
    }
}