#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Components\UserControls\ActionControl.cs) is part of CiviKey. 
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeyboardEditor.ViewModels
{
    public class ActionControl : Control
    {
        static ActionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( ActionControl ), new FrameworkPropertyMetadata( typeof( ActionControl ) ) );
        }

        public ICommand Action
        {
            get { return (ICommand)GetValue( ActionProperty ); }
            set { SetValue( ActionProperty, value ); }
        }
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register( "Action", typeof( ICommand ), typeof( ActionControl ) );

        public object ActionParameter
        {
            get { return (object)GetValue( ActionParameterProperty ); }
            set { SetValue( ActionParameterProperty, value ); }
        }
        public static readonly DependencyProperty ActionParameterProperty =
            DependencyProperty.Register( "ActionParameter", typeof( object ), typeof( ActionControl ) );


        public string Text
        {
            get { return (string)GetValue( TextProperty ); }
            set { SetValue( TextProperty, value ); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register( "Text", typeof( string ), typeof( ActionControl ) );

        public string TooltipText
        {
            get { return (string)GetValue( TooltipTextProperty ); }
            set { SetValue( TooltipTextProperty, value ); }
        }
        public static readonly DependencyProperty TooltipTextProperty =
            DependencyProperty.Register( "TooltipText", typeof( string ), typeof( ActionControl ) );

        public string ImageSource
        {
            get { return GetValue( ImageSourceProperty ).ToString(); }
            set { SetValue( ImageSourceProperty, value ); }
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register( "ImageSource", typeof( string ), typeof( ActionControl ) );
    }
}
