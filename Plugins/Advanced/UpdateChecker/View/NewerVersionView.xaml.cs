using System.Windows;

namespace UpdateChecker.View
{
    /// <summary>
    /// Interaction logic for NewerVersionView.xaml
    /// </summary>
    public partial class NewerVersionView : Window
    {
        public NewerVersionView()
        {
            InitializeComponent();
        }

        public void SetBrowserContent( string content )
        {
            _browser.NavigateToString( content );
        }
    }
}
