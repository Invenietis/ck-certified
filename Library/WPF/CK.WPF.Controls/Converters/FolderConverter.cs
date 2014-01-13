#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Advanced\ObjectExplorer\UI\Converters\FolderConverter.cs) is part of CiviKey. 
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
using System.Windows.Data;
using System.Collections;
using System.Globalization;

namespace CK.WPF.Controls
{
    public class FolderConverter : IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            //get folder name listing...
            string folder = parameter as string ?? "";
            var folders = folder.Split( ',' ).Select( f => f.Trim() ).ToList();
            //...and make sure there are no missing entries
            while( values.Length > folders.Count ) folders.Add( String.Empty );

            //this is the collection that gets all top level items
            List<object> items = new List<object>();

            for( int i = 0; i < values.Length; i++ )
            {
                //make sure were working with collections from here...
                IEnumerable childs = values[i] as IEnumerable ?? new List<object> { values[i] };

                string folderName = folders[i];
                if( folderName != String.Empty )
                {
                    //create folder item and assign childs
                    FolderItem folderItem = new FolderItem { Name = folderName, Items = childs };
                    items.Add( folderItem );
                }
                else
                {
                    //if no folder name was specified, move the item directly to the root item
                    foreach( var child in childs ) { items.Add( child ); }
                }
            }

            return items;
        }


        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException( "Cannot perform reverse-conversion" );
        }
    }

    /// <summary>
    /// Provides a virtual folder data structure for arbitrary
    /// child items.
    /// </summary>
    public class FolderItem : VMBase
    {
        #region Name

        /// <summary>
        /// The name that can be displayed or used as an
        /// ID to perform more complex styling.
        /// </summary>
        private string name;


        /// <summary>
        /// The name that can be displayed or used as an
        /// ID to perform more complex styling.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                //ignore if values are equal
                if( value == name ) return;

                name = value;
                OnPropertyChanged( "Name" );
            }
        }

        public bool IsExpanded { get { return true; } set { } }
        //public bool IsSelected { get { return false; } set { } }
        public bool IsSelected { get; set; }

        #endregion

        #region Items

        /// <summary>
        /// The child items of the folder.
        /// </summary>
        private IEnumerable items;


        /// <summary>
        /// The child items of the folder.
        /// </summary>
        public IEnumerable Items
        {
            get { return items; }
            set
            {
                //ignore if values are equal
                if( value == items ) return;

                items = value;
                OnPropertyChanged( "Items" );
            }
        }

        #endregion


        public FolderItem()
        {
        }

        /// <summary>
        /// This method is invoked by WPF to render the object if
        /// no data template is available.
        /// </summary>
        /// <returns>Returns the value of the <see cref="Name"/>
        /// property.</returns>
        public override string ToString()
        {
            return string.Format( "{0}: {1}", GetType().Name, Name );
        }

    }
}
