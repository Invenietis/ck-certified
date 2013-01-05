#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMKeyEditable.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using System.Windows.Controls;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using Microsoft.Win32;
using System.Windows.Input;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using CommonServices;

namespace ContextEditor.ViewModels
{
    public partial class VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
    {
        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout.Current; }
        }

        #region Key Image management

        public bool ShowLabel
        {
            get { return LayoutKeyMode.GetPropertyValue<bool>( Context.Config, "ShowLabel", true ); }
        }

        //ICommand _removeImageCommand;
        //public ICommand RemoveImageCommand
        //{
        //    get
        //    {
        //        if( _removeImageCommand == null )
        //        {
        //            _removeImageCommand = new VMCommand( () => Context.SkinConfiguration[Model.CurrentLayout.Current].Remove( "Image" ) );
        //        }
        //        return _removeImageCommand;
        //    }
        //}

        //ICommand _browseCommand;
        //public ICommand BrowseCommand
        //{
        //    get
        //    {
        //        if( _browseCommand == null )
        //        {
        //            _browseCommand = new VMCommand<VMKeyEditable>( ( k ) =>
        //            {
        //                var fd = new OpenFileDialog();
        //                fd.DefaultExt = ".png";
        //                if( fd.ShowDialog() == true )
        //                {
        //                    if( !String.IsNullOrWhiteSpace( fd.FileName ) && File.Exists( fd.FileName ) && EnsureIsImage( Path.GetExtension( fd.FileName ) ) )
        //                    {
        //                        using( Stream str = fd.OpenFile() )
        //                        {
        //                            byte[] bytes = new byte[str.Length];
        //                            str.Read( bytes, 0, Convert.ToInt32( str.Length ) );
        //                            string encodedImage = Convert.ToBase64String( bytes, Base64FormattingOptions.None );

        //                            Context.SkinConfiguration[Model.CurrentLayout.Current].GetOrSet( "Image", encodedImage );
        //                        }
        //                    }
        //                }
        //            } );
        //        }
        //        return _browseCommand;
        //    }
        //}

        //private bool EnsureIsImage( string extension )
        //{
        //    return String.Compare( extension, ".jpeg", StringComparison.CurrentCultureIgnoreCase ) == 0
        //        || String.Compare( extension, ".jpg", StringComparison.CurrentCultureIgnoreCase ) == 0
        //        || String.Compare( extension, ".png", StringComparison.CurrentCultureIgnoreCase ) == 0
        //        || String.Compare( extension, ".bmp", StringComparison.CurrentCultureIgnoreCase ) == 0;
        //}

        #endregion

        /// <summary>
        /// Gets the opacity of the key.
        /// When the key has <see cref="IsBeingEdited"/> set to false, the opacity is lowered.
        /// returns 1 otherwise.
        /// </summary>
        public double Opacity
        {
            get { return IsBeingEdited ? 1 : 0.3; }
        }

        double _zIndex = 50;
        public double ZIndex 
        { 
            get { return _zIndex; } 
            set 
            { 
                _zIndex = value; 
                OnPropertyChanged( "ZIndex" ); 
            }
        }
    }
}
