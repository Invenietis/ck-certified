#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.Controls\ImageButton.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CK.WPF.Controls
{
    public class ImageButton : Image
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ImageButton ), new FrameworkPropertyMetadata( typeof( ImageButton ) ) );
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ImageButton));

        public object CommandParameter
        {
            get { return (object)GetValue( CommandParameterProperty ); }
            set { SetValue( CommandParameterProperty, value ); }
        }
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register( "CommandParameter", typeof( object ), typeof( ImageButton ), new UIPropertyMetadata( null ) );

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            if( Command != null && Command.CanExecute( CommandParameter ) )
                Command.Execute( CommandParameter );
            base.OnMouseLeftButtonDown( e );
        }
    }
}
