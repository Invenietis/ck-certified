#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ContextEditor\KeyboardEdition\Views\Keyboard\KeyboardEditionUserControl.xaml.cs) is part of CiviKey. 
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

namespace KeyboardEditor.s
{
    /// <summary>
    /// Interaction logic for KeyboardEditionView.xaml
    /// </summary>
    public partial class KeyboardEditionUserControl : UserControl
    {
        public KeyboardEditionUserControl()
        {
            InitializeComponent();
        }

        private void OnNewZoneClicked( object sender, RoutedEventArgs e )
        {
            ToStep2();
        }

        private void OnConfirmCreateNewZoneClicked( object sender, RoutedEventArgs e )
        {
            ToStep1();
        }

        private void ToStep1()
        {
            zoneCreationPanel.Visibility = System.Windows.Visibility.Collapsed;
            zoneCreationMainButton.Visibility = System.Windows.Visibility.Visible;

        }

        private void ToStep2()
        {
            zoneCreationTextBox.Text = "";
            zoneCreationPanel.Visibility = System.Windows.Visibility.Visible;
            zoneCreationMainButton.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
