#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\Impl\Notifications\Views\Contents\ExceptionContent.xaml.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
