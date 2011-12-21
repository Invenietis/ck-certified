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

namespace Host.Services
{
    /// <summary>
    /// Interaction logic for NotificationWrapper.xaml
    /// </summary>
    public partial class NotificationWrapper : CKNotification
    {
        public NotificationWrapper() : base(null,null)
        {
            InitializeComponent();
        }

        public NotificationWrapper(UIElement content, string imagePath)
            : base(content,imagePath)
        {
            InitializeComponent();
        }
    }
}
