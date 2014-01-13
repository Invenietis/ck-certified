using CK.Windows;

namespace CK.WordPredictor.UI
{
    /// <summary>
    /// Interaction logic for PredictionTextArea.xaml
    /// </summary>
    public partial class TextualContextPreviewWindow : CKWindow
    {
        public TextualContextPreviewWindow( ViewModels.TextualContextPreviewViewModel vm )
        {
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
