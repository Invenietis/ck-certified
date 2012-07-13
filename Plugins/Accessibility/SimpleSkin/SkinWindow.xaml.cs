using CK.Windows;

namespace SimpleSkin
{
    /// <summary>
    /// Logique d'interaction pour SkinWindow.xaml
    /// </summary>
    public partial class SkinWindow : NoFocusWindow
    {
        public SkinWindow( object dc )
        {
            this.DataContext = dc;
            InitializeComponent();
        } 
        
    }
}
