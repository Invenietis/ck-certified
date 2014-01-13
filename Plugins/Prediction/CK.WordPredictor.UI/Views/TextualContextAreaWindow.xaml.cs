using CK.Windows;

namespace CK.WordPredictor.UI
{
    /// <summary>
    /// Interaction logic for PredictionTextArea.xaml
    /// </summary>
    public partial class TextualContextAreaWindow : CKWindow
    {
        public TextualContextAreaWindow( ViewModels.TextualContextAreaViewModel vm )
        {
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
