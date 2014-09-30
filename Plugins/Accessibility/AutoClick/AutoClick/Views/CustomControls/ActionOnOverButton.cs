#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\AutoClick\AutoClick\Views\CustomControls\ActionOnOverButton.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CK.Plugins.AutoClick.Views
{
    /// <summary>
    ///  Button that will send its MouseEnterCommand command when the mouse stays over it for 0.5 second
    /// </summary>
    public class ActionOnOverButton : Button
    {
        DispatcherTimer _overTimer;

        static ActionOnOverButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionOnOverButton ), new FrameworkPropertyMetadata( typeof( ActionOnOverButton ) ) );
            MouseEnterCommandProperty = DependencyProperty.Register( "MouseEnterCommand", typeof( ICommand ), typeof( ActionOnOverButton ) );
        }           

        static DependencyProperty MouseEnterCommandProperty;
        public ICommand MouseEnterCommand
        {
            get { return (ICommand)GetValue( MouseEnterCommandProperty ); }
            set { SetValue( MouseEnterCommandProperty, value ); }
        }

        protected override void OnMouseEnter( MouseEventArgs e )
        {
            if( _overTimer == null )
            {
                _overTimer = new DispatcherTimer();
                _overTimer.Interval = new TimeSpan( 0, 0, 0, 0, 500 );
                _overTimer.Tick += new EventHandler( OnTick );
            }
            _overTimer.Start();
            FireCommand( MouseEnterCommand );
            base.OnMouseEnter( e );
        }

        protected override void OnMouseLeave( MouseEventArgs e )
        {
            _overTimer.Stop();
            base.OnMouseLeave( e );
        }

        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( true ) )
                    command.Execute( this );
            }
        }

        private void OnTick( object sender, EventArgs e )
        {
            FireCommand( MouseEnterCommand );
        }
    }
}
