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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace CK.Plugins.ObjectExplorer
{
    public class FolderConverter : IMultiValueConverter
    {
        public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string folder = parameter as string ?? "";
            List<string> folders = folder.Split( ',' ).Select( f => f.Trim() ).ToList();

            //this is the collection that gets all top level items
            List<object> items = new List<object>();

            for( int i = 0; i < values.Length; i++ )
            {
                //make sure were working with collections from here...
                IEnumerable childs = values[i] as IEnumerable ?? new List<object> { values[i] };
                if( childs.GetEnumerator().MoveNext() )
                {

                    string folderName = folders[i];
                    if( folderName != String.Empty )
                    {
                        //create folder item and assign childs
                        VMIFolder folderItem = new VMIFolder( childs, folderName );
                        items.Add( folderItem );
                    }
                    else
                    {
                        //if no folder name was specified, move the item directly to the root item
                        foreach( var child in childs ) { items.Add( child ); }
                    }
                }
            }
            return items;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}
