using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CK.Windows;

namespace ScrollerVisualizer
{
    /// <summary>
    /// Interaction logic for Vizualization.xaml
    /// </summary>
    public partial class Visualization : CKWindow
    {
        public Visualization(VisualizationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            this.Top = 0;
            Left = 0;
            Storyboard tIn = (Storyboard) this.Resources["toggleIn"];
            Storyboard tOut = (Storyboard) this.Resources["toggleOut"];
            StackPanel sp = (StackPanel) FindName( "subPanel" );
            this.Name = "main";
            vm.ToggleIn += ( o, e ) => tIn.Begin();
            vm.ToggleOut += ( o, e ) => tOut.Begin();
        }

        protected override void OnChildDesiredSizeChanged( UIElement child )
        {
            base.OnChildDesiredSizeChanged( child );
          
            
            Console.WriteLine( child.GetValue(FrameworkElement.NameProperty));       
        }

        //protected override bool IsDraggableVisual( DependencyObject visualElement )
        //{
        //    if( visualElement is ButtonBase || visualElement.FindParent( x => x is ButtonBase ) != null )
        //    {

        //        return false;
        //    }
        //    return true;
        //}

        private void ToggleButton_Click( object sender, RoutedEventArgs e )
        {

        }
    }
}