using System.Windows;
using System.Windows.Controls;

namespace KeyboardEditor.s
{
    /// <summary>
    /// Interaction logic for KeyboardEditionView.xaml
    /// </summary>
    public partial class KeyboardEditionUserControl : UserControl
    {
        public KeyboardEditionUserControl()
        {
            InitializeComponent();
        }

        private void OnNewZoneClicked( object sender, RoutedEventArgs e )
        {
            ToStep2();
        }

        private void OnConfirmCreateNewZoneClicked( object sender, RoutedEventArgs e )
        {
            ToStep1();
        }

        private void ToStep1()
        {
            zoneCreationPanel.Visibility = System.Windows.Visibility.Collapsed;
            zoneCreationMainButton.Visibility = System.Windows.Visibility.Visible;

        }

        private void ToStep2()
        {
            zoneCreationTextBox.Text = "";
            zoneCreationPanel.Visibility = System.Windows.Visibility.Visible;
            zoneCreationMainButton.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
