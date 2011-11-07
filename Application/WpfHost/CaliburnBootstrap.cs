using System.Linq;
using Caliburn.Micro;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using CK.Plugin;

namespace Host
{
    public class CaliburnBootstrap : Bootstrapper
    {
        public CaliburnBootstrap()
            : base()
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

        protected override void Configure()
        {
            base.Configure();
            Caliburn.Micro.LogManager.GetLog = t => new CaliburnAdapter( CivikeyStandardHost.Instance.Context.LogCenter, Common.Logging.LogManager.GetLogger( t ) );
        }

        class CaliburnAdapter : Caliburn.Micro.ILog
        {
            Common.Logging.ILog _common;
            CK.Plugin.ILogCenter _center;

            public CaliburnAdapter( CK.Plugin.ILogCenter center, Common.Logging.ILog common )
            {
                _center = center;
                _common = common;
            }

            public void Error( System.Exception exception )
            {
                _center.ExternalLogError( exception, null, "Logged from Caliburn", null );
            }

            public void Info( string format, params object[] args )
            {
                _common.InfoFormat( format, args );
            }

            public void Warn( string format, params object[] args )
            {
                _common.WarnFormat( format, args );
            }

        }


        protected override void DisplayRootView()
        {
            IWindowManager manager = new WindowManager();
            manager.Show( new Host.AppViewModel(), null );
        }

    }
}