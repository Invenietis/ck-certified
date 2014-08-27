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
        }

        protected override bool IsDraggableVisual( DependencyObject visualElement )
        {
            return true;
        }
    }
}