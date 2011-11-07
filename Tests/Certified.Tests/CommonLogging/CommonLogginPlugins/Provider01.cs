using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Threading;

namespace CommonLogginPlugins
{
    [Plugin( "{99C25400-A002-4e0d-955B-3F4CC0C25E1D}",
        Categories = new string[] { "Advanced" },
        PublicName = "Provider01",
        Version = "1.0.0" )]
    public class Provider01 : IPlugin, IService01
    {

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
          
        }

        public void Stop()
        {
            
        }

        public void Teardown()
        {
          
        }

        #endregion

        #region IService04 Members

        public void LaunchEvents()
        {
            if( EventOK != null )
                EventOK( this, new CustomEventArgs( "The EventOK name", 25 ) );

            if( EventError != null )
                EventError( this, new CustomEventArgs( "The EventError's name", 22 ) );   
        }

        public string MethodComplexOK( CustomObject c, List<string> stringList, HashSet<CustomObject> hash )
        {
            return "My MethodComplexOK's ReturnValue";
        }

        public string MethodSimpleOK( string name )
        {
            return "My MethodSimpleOK's ReturnValue";
        }

        public string MethodError( string name )
        {
            throw new NotImplementedException( "My MethodError's Exception", new Exception( "My Inner Exception" ) );
            return "My MethodError's ReturnValue";
        }

        public event EventHandler<CustomEventArgs>  EventOK;

        public event EventHandler<CustomEventArgs>  EventError;

        #endregion
    }
}
