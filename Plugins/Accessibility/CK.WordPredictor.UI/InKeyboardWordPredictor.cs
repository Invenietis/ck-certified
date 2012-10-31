using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI
{
    [Plugin( "{1756C34D-EF4F-45DA-9224-1232E96964D2}", PublicName = "CK.Wordpredictor.UI | InKeyboard" )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [RequiredService]
        public IKeyboardContext Context { get; set; }

        //[DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        //public IService<ITextualContextService> TextualContextService { get; set; }

        [RequiredService]
        public IWordPredictorService WordPredictorService { get; set; }


        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            int keyHeight = 50;
            int keySpace = 5;
            IKeyboard kb = UpdateAzertyKeyboard( keyHeight, keySpace );
            if( kb != null )
            {
                if( kb.Zones["Prediction"] == null )
                {
                    IZone zone = kb.Zones.Create( "Prediction" );
                    IKeyMode keyMode = zone.Keys.Create().KeyModes.Create( kb.CurrentMode );
                    keyMode.DownLabel = "test";
                    keyMode.UpLabel = "test";
                }
            }
        }

        private IKeyboard UpdateAzertyKeyboard( int keyHeight, int keySpace )
        {
            IKeyboard azertyKeyboard = Context.Keyboards["Azerty"];
            if( azertyKeyboard != null )
            {
                //azertyKeyboard.CurrentLayout.LayoutZones.SelectMany( t => t.LayoutKeys ).ToList().ForEach( ( l ) =>
                //{
                //    foreach( var mode in l.LayoutKeyModes )
                //    {
                //        mode.Y = mode.Y + keyHeight + keySpace;
                //    }
                //} );
                //azertyKeyboard.CurrentLayout.H = Context.CurrentKeyboard.CurrentLayout.H + keyHeight + keySpace;
                return azertyKeyboard;
            }

            return null;
        }

        public void Stop()
        {
            int keyHeight = -50;
            int keySpace = -5;
            IKeyboard kb = UpdateAzertyKeyboard( keyHeight, keySpace );
            if( kb != null )
            {
                IZone zone = kb.Zones["Prediction"];
                if( zone != null ) zone.Destroy();
            }
        }

        public void Teardown()
        {
        }
    }
}
