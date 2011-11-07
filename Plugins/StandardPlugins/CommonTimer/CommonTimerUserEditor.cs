#region LGPL License
/*----------------------------------------------------------------------------
* This file (StandardPlugins\CommonTimer\CommonTimerUserEditor.cs) is part of CiviKey. 
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
* Copyright © 2007-2009, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CK.StandardPlugins.CommonTimer
{
	public partial class CommonTimerUserEditor : UserControl
	{
		CommonTimerEditor _editor;

		public CommonTimerUserEditor( CommonTimerEditor e )
		{
			InitializeComponent();

			_editor = e;

            nInteval.Value = _editor.Interval;
		}

		private void OnSave( object sender, EventArgs e )
		{
			// The user has changed the value of the interval
			// The editor will set the new configuration
            _editor.Interval = (int)nInteval.Value;
		}

	}
}
