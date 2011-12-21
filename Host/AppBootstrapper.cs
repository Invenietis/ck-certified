using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows.Controls;

namespace Host
{
    internal class AppBootstrapper : Bootstrapper<AppViewModel>
    {

        public AppBootstrapper()
            :base()
        {
            //ViewLocator.LocateTypeForModelType = ( modelType, displayLocation, context ) =>
            //{
            //    var viewTypeName = modelType.FullName;

            //    viewTypeName = viewTypeName.Substring(
            //        0,
            //        viewTypeName.IndexOf( "`" ) < 0
            //            ? viewTypeName.Length
            //            : viewTypeName.IndexOf( "`" )
            //        );

            //    var viewTypeList = ViewLocator.TransformName( viewTypeName, context );
            //    Type viewType = viewTypeList.Select( ( v ) => modelType.Assembly.GetType( v ) ).FirstOrDefault();
            //    if( viewType == null )
            //    {
            //        viewType = (from assembly in AssemblySource.Instance
            //                    from type in assembly.GetExportedTypes()
            //                    where viewTypeList.Contains( type.FullName )
            //                    select type).FirstOrDefault();

            //        //if( viewType == null )
            //        //{
            //        //    Log.Warn( "View not found. Searched: {0}.", string.Join( ", ", viewTypeList.ToArray() ) );
            //        //}
            //    }
            //    return viewType;
            //};

            ViewLocator.LocateForModelType = ( modelType, displayLocation, context ) =>
            {
                var viewTypeName = modelType.FullName.Replace( "ViewModel", string.Empty ) + "View";
                if( context != null )
                {
                    viewTypeName = viewTypeName.Remove( viewTypeName.Length - 4, 4 );
                    viewTypeName = viewTypeName + "." + context;
                }

                Type viewType = modelType.Assembly.GetType( viewTypeName );
                if( viewType == null )
                {
                    viewType = (from assembly in AssemblySource.Instance
                                from type in assembly.GetExportedTypes()
                                where type.FullName == viewTypeName
                                select type).FirstOrDefault();
                }
                return viewType == null
                    ? new TextBlock { Text = string.Format( "{0} not found.", viewTypeName ) }
                    : ViewLocator.GetOrCreateViewType( viewType );
            };
        }
    }
}
