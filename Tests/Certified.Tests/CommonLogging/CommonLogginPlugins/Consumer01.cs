using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonLogginPlugins
{
    [Plugin( "{9BEFB310-558E-4de2-B245-62B7C805D49B}",
        Categories = new string[] { "Advanced" },
        PublicName = "Consumer01",
        Version = "1.0.0" )]
    public class Consumer01 : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService01 Service01 { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            Service01.EventError += new EventHandler<CustomEventArgs>( Service01_EventError );
            Service01.EventOK += new EventHandler<CustomEventArgs>( Service01_EventOK );

            Service01.LaunchEvents();
        }

        public void Stop()
        {
            
        }

        public void Teardown()
        {
          
        }

        #endregion

        void Service01_EventError( object sender, CustomEventArgs e )
        {
            Service01.MethodError( "MethodError's parameter" );
        }

        void Service01_EventOK( object sender, CustomEventArgs e )
        {
            Service01.MethodSimpleOK( "MethodError's parameter" );

            HashSet<CustomObject> hashset = new HashSet<CustomObject>();
            hashset.Add(new CustomObject( "CustomObject 2, from MethodComplexOK", 22 ));
            hashset.Add(new CustomObject( "CustomObject 3, from MethodComplexOK", 23 ));

            Service01.MethodComplexOK( new CustomObject( "CustomObject 1, from MethodComplexOK", 21 ), new List<string>() { "1", "2" }, hashset );
        }
    }
}
