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
using ScreenDivider.Events;
using ScreenDivider.ViewModels;

namespace ScreenDivider.Views.Controls
{
    /// <summary>
    /// Interaction logic for ArrowUserControl.xaml
    /// </summary>
    public partial class ArrowUserControl : UserControl
    {
        bool _isLast = false;

        public ArrowUserControl( ScrollingDirection direction )
        {
            if( _isLast && direction == ScrollingDirection.GoToParent ) direction = ScrollingDirection.GoToWindow;
            InitializeComponent();

            double angle = 0;
            switch( direction )
            {
                case ScrollingDirection.Left: angle = 180; break;
                case ScrollingDirection.Top: angle = 270; break;
                case ScrollingDirection.Bottom: angle = 90; break;
                case ScrollingDirection.GoToWindow:
                    Arrow.Source = new BitmapImage( new Uri( "pack://application:,,,/WPFTry;component/Resources/New_window.png" ) );
                    _isLast = false;
                    break;
                case ScrollingDirection.GoToParent:
                    Arrow.Source = new BitmapImage( new Uri( "pack://application:,,,/WPFTry;component/Resources/ArrowTurnLeftDown.png" ) );
                    break;
            }
            Arrow.RenderTransform = new RotateTransform( angle );
            Arrow.RenderTransformOrigin = new Point( 0.5, 0.5 );
        }

        public void GoToWindow( object sender, GoToWindowEventArgs args )
        {
            _isLast = true;
        }
    }
}
