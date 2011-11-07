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
    /// Interaction logic for ExceptionContent.xaml
    /// </summary>
    public partial class ExceptionContent : UserControl
    {
        Exception _ex;

        public ExceptionContent( Exception ex )
        {
            InitializeComponent();
            _ex = ex;
            _message.Text = ex.Message;
        }

        void CopyException( object sender, RoutedEventArgs e )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( "Message : " );
            builder.Append( _ex.Message + '\n' );
            builder.Append( "Stack : " );
            builder.Append( _ex.StackTrace + '\n' );
            builder.Append( "HelpLink : " );
            builder.Append( _ex.HelpLink + '\n' );

            Clipboard.SetText( builder.ToString() );
        }
    }
}
