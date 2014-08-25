#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\UpdateChecker\View\NewerVersionViewModel.cs) is part of CiviKey. 
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
using System.IO;
using System.Windows;
using System.Windows.Input;
using CK.Windows.App;
using MarkdownSharp;

namespace UpdateChecker.View
{
    public class NewerVersionViewModel
    {
        public NewerVersionViewModel( string releasenotes, string newVersion, Action<bool> CloseView, Action<string> SetBrowserContent )
        {
            Title = R.UpdateAvailableTitle;

            OkCommand = new VMCommand( () => CloseView( true ) );
            CancelCommand = new VMCommand( () => CloseView( false ) );

            Header = string.Format( R.UpdateAvailableContent, newVersion );

            if( !string.IsNullOrEmpty( releasenotes ) )
            {
                string cssSnipet = string.Empty;
                using( var rs = typeof( NewerVersionViewModel ).Assembly.GetManifestResourceStream( "UpdateChecker.View.Resources.MarkdownCssSnipet.html" ) )
                using( var rdr = new StreamReader( rs ) )
                    cssSnipet = rdr.ReadToEnd();

                ReleaseNotes = string.Concat( cssSnipet, new Markdown( new MarkdownOptions { AutoHyperlink = false } ).Transform( releasenotes ) );

                SetBrowserContent( ReleaseNotes );
            }
        }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string ReleaseNotes { get; private set; }
        public Visibility BrowserVisibility { get { return string.IsNullOrEmpty( ReleaseNotes ) ? Visibility.Collapsed : Visibility.Visible; } }
        public string Yes { get { return R.Yes; } }
        public string No { get { return R.No; } }
    }
}
