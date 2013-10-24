#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\Views\CustomControls\ActionOnMouseEnterButton.cs) is part of CiviKey. 
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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace CK.Plugins.AutoClick.Views
{
    public class ActionOnMouseEnterButton : Button
    {
        static ActionOnMouseEnterButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionOnMouseEnterButton ), new FrameworkPropertyMetadata( typeof( ActionOnMouseEnterButton ) ) );
            IsPausedProperty = DependencyProperty.Register( "IsPaused", typeof( bool ), typeof( ActionOnMouseEnterButton ) ); 
        }
        
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register( "Selected", typeof( bool ), typeof( ActionOnMouseEnterButton ) );
        public bool Selected
        {
            get { return (bool)GetValue( SelectedProperty ); }
            set { SetValue( SelectedProperty, value ); }
        }

        public static DependencyProperty IsPausedProperty;
        public bool IsPaused
        {
            get { return (bool)GetValue( IsPausedProperty ); }
            set { SetValue( IsPausedProperty, value ); }
        }

        public ICommand MouseEnterCommand
        {
            get { return (ICommand)GetValue( MouseEnterCommandProperty ); }
            set 
            {
                SetValue( MouseEnterCommandProperty, value ); 
            }
        }
        public static readonly DependencyProperty MouseEnterCommandProperty =
        DependencyProperty.Register( "MouseEnterCommand", typeof( ICommand ), typeof( ActionOnMouseEnterButton ) );

        //TODO : Replace by a Behavior.
        protected override void OnMouseEnter( MouseEventArgs e )
        {
            FireCommand( MouseEnterCommand );
        }
        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( true ) )
                    command.Execute( this );
            }
        }     
    }
}
