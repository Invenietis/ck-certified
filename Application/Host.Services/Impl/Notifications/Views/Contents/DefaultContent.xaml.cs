using System.Windows.Controls;

namespace Host.Services
{
    /// <summary>
    /// Interaction logic for DefaultContent.xaml
    /// </summary>
    public partial class DefaultContent : UserControl
    {
        public DefaultContent(string title, string textContent)
        {
            InitializeComponent();

            _title.Text = title;
            _content.Text = textContent;
        }
    }
}
