#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.StandardViews\StdKeyView.cs) is part of CiviKey. 
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
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;

namespace CK.WPF.StandardViews
{
    [ContentProperty( "UpLabel" )]
    public class StdKeyView : ButtonBase
    {
        static StdKeyView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( StdKeyView ), new FrameworkPropertyMetadata( typeof( StdKeyView ) ) );
        }

        public StdKeyView()
        {
            Focusable = false;
        }

        public int X
        {
            get { return (int)GetValue( XProperty ); }
            set { SetValue( XProperty, value ); }
        }
        public static readonly DependencyProperty XProperty =
        DependencyProperty.Register( "X", typeof( int ), typeof( StdKeyView ) );

        public int Y
        {
            get { return (int)GetValue( YProperty ); }
            set { SetValue( YProperty, value ); }
        }
        public static readonly DependencyProperty YProperty =
        DependencyProperty.Register( "Y", typeof( int ), typeof( StdKeyView ) );

        public ICommand Description
        {
            get { return (ICommand)GetValue( DescriptionProperty ); }
            set { SetValue( DescriptionProperty, value ); }
        }
        public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register( "Description", typeof( string ), typeof( StdKeyView ) );

        public int ZIndex
        {
            get { return (int)GetValue( ZIndexProperty ); }
            set { SetValue( ZIndexProperty, value ); }
        }
        public static readonly DependencyProperty ZIndexProperty =
        DependencyProperty.Register( "ZIndex", typeof( int ), typeof( StdKeyView ) );

        public ICommand KeyDownCommand
        {
            get { return (ICommand)GetValue( KeyDownCommandProperty ); }
            set { SetValue( KeyDownCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyDownCommandProperty = 
        DependencyProperty.Register( "KeyDownCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public ICommand KeyUpCommand
        {
            get { return (ICommand)GetValue( KeyUpCommandProperty ); }
            set { SetValue( KeyUpCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyUpCommandProperty = 
        DependencyProperty.Register( "KeyUpCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public ICommand KeyPressedCommand
        {
            get { return (ICommand)GetValue( KeyPressedCommandProperty ); }
            set { SetValue( KeyPressedCommandProperty, value ); }
        }
        public static readonly DependencyProperty KeyPressedCommandProperty = 
        DependencyProperty.Register( "KeyPressedCommand", typeof( ICommand ), typeof( StdKeyView ) );

        public FrameworkElement CustomContent
        {
            get { return (Image)GetValue( CustomContentProperty ); }
            set { SetValue( CustomContentProperty, value ); }
        }
        public static readonly DependencyProperty CustomContentProperty = 
        DependencyProperty.Register( "CustomContent", typeof( FrameworkElement ), typeof( StdKeyView ) );

        public bool ShowLabel
        {
            get { return (bool)GetValue( ShowLabelProperty ); }
            set { SetValue( ShowLabelProperty, value ); }
        }
        public static readonly DependencyProperty ShowLabelProperty = 
        DependencyProperty.Register( "ShowLabel", typeof( bool ), typeof( StdKeyView ), new PropertyMetadata( true ) );

        public bool ShowImage
        {
            get { return (bool)GetValue( ShowImageProperty ); }
            set { SetValue( ShowImageProperty, value ); }
        }
        public static readonly DependencyProperty ShowImageProperty =
        DependencyProperty.Register( "ShowImage", typeof( bool ), typeof( StdKeyView ), new PropertyMetadata( true ) );


        public string UpLabel
        {
            get { return (string)GetValue( UpLabelProperty ); }
            set { SetValue( UpLabelProperty, value ); }
        }
        public static readonly DependencyProperty UpLabelProperty = 
        DependencyProperty.Register( "UpLabel", typeof( string ), typeof( StdKeyView ) );

        public string DownLabel
        {
            get { return (string)GetValue( DownLabelProperty ); }
            set { SetValue( DownLabelProperty, value ); }
        }
        public static readonly DependencyProperty DownLabelProperty = 
        DependencyProperty.Register( "DownLabel", typeof( string ), typeof( StdKeyView ) );

        public Brush HoverBackground
        {
            get { return (Brush)GetValue( HoverBackgroundProperty ); }
            set { SetValue( HoverBackgroundProperty, value ); }
        }
        public static readonly DependencyProperty HoverBackgroundProperty = 
        DependencyProperty.Register( "HoverBackground", typeof( Brush ), typeof( StdKeyView ) );

        public Brush PressedBackground
        {
            get { return (Brush)GetValue( PressedBackgroundProperty ); }
            set { SetValue( PressedBackgroundProperty, value ); }
        }
        public static readonly DependencyProperty PressedBackgroundProperty = 
        DependencyProperty.Register( "PressedBackground", typeof( Brush ), typeof( StdKeyView ) );

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue( TextDecorationsProperty ); }
            set { SetValue( TextDecorationsProperty, value ); }
        }
        public static readonly DependencyProperty TextDecorationsProperty = 
        DependencyProperty.Register( "TextDecorations", typeof( TextDecorationCollection ), typeof( StdKeyView ) );

        protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            FireCommand( KeyDownCommand );
            base.OnMouseLeftButtonDown( e );
        }

        protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            FireCommand( KeyUpCommand );
            base.OnMouseLeftButtonUp( e );
        }

        void FireCommand( ICommand command )
        {
            if( command != null )
            {
                if( command.CanExecute( CommandParameter ) )
                    command.Execute( CommandParameter );
            }
        }
    }
}