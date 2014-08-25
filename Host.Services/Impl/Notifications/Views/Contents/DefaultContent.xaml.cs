#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host.Services\Impl\Notifications\Views\Contents\DefaultContent.xaml.cs) is part of CiviKey. 
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

using System.Windows.Controls;

namespace Host.Services
{
    /// <summary>
    /// Interaction logic for DefaultContent.xaml
    /// </summary>
    public partial class DefaultContent : UserControl
    {
        public DefaultContent(string title, string textContent)
        {
            InitializeComponent();

            _title.Text = title;
            _content.Text = textContent;
        }
    }
}
