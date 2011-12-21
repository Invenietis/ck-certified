using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

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
