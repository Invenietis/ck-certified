#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\CK.WindowManager.Model\PreviewBindingInfo.cs) is part of CiviKey. 
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
using System.Windows;
using CK.Windows;

namespace CK.WindowManager.Model
{
    public class PreviewBindingInfo
    {
        public IBinding Binding;
        public CKWindow Window;

        public BindingPosition Position
        {
            get { return Binding != null ? Binding.Position : BindingPosition.None; }
        }

        public bool HasPreview
        {
            get { return Position != BindingPosition.None; }
        }

        public void Shutdown( ITopMostService topMostService )
        {
            if( HasPreview )
            {
                Binding = null;
                if( Window != null ) Window.Dispatcher.BeginInvoke( new Action( () =>
                {
                    Window.Hide();
                    topMostService.UnregisterTopMostElement( Window );
                } ) );
            }
        }

        public void Display( IBinding binding, ITopMostService topMostService )
        {
            if( binding == null ) return;

            if( Window == null )
            {
                Window = new CKWindow();
                Window.Dispatcher.BeginInvoke( new Action( () =>
                {
                    Window.Opacity = .8;
                    Window.Background = new System.Windows.Media.SolidColorBrush( System.Windows.Media.Color.FromRgb( 152, 120, 152 ) );
                    Window.ResizeMode = ResizeMode.NoResize;
                    Window.WindowStyle = WindowStyle.None;
                    Window.ShowInTaskbar = false;
                    Window.ShowActivated = false;
                } ) );
            }
            Binding = binding;

            Rect r = Binding.GetWindowArea();
            if( r != Rect.Empty )
            {
                Window.Dispatcher.BeginInvoke( new Action( () =>
                {
                    Window.Left = r.Left;
                    Window.Top = r.Top;
                    Window.Width = r.Width;
                    Window.Height = r.Height;
                    Window.Show();

                    topMostService.RegisterTopMostElement( "1", Window );
                } ) );
            }
        }

        public bool IsPreviewOf( IBinding newBinding )
        {
            if( newBinding == null ) throw new ArgumentNullException( "binding" );

            if( Binding == null ) return false;
            return newBinding.Position == Binding.Position && Binding.Target == newBinding.Target;
        }
    }

}
