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
