using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;

namespace CK.WordPredictor.UI
{
    [Plugin( "{F3417218-816C-4838-A998-F6CE399AA85A}", PublicName = "KeyboardWordPredictor" )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [RequiredService]
        public IKeyboardContext Context { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            int keyHeight = 50;
            int keySpace = 5;
            SetWidthForAzertyKeyboard( keyHeight, keySpace );
        }

        private void SetWidthForAzertyKeyboard( int keyHeight, int keySpace )
        {
            IKeyboard azertyKeyboard = Context.Keyboards["Azerty"];
            if( azertyKeyboard != null )
            {
                azertyKeyboard.CurrentLayout.LayoutZones.SelectMany( t => t.LayoutKeys ).ToList().ForEach( ( l ) =>
                {
                    foreach( var mode in l.LayoutKeyModes )
                    {
                        mode.X = mode.X + keyHeight + keySpace;
                    }
                } );
                azertyKeyboard.CurrentLayout.H = Context.CurrentKeyboard.CurrentLayout.H + keyHeight + keySpace;
            }
        }

        public void Stop()
        {
            int keyHeight = -50;
            int keySpace = -5;
            SetWidthForAzertyKeyboard( keyHeight, keySpace );
        }

        public void Teardown()
        {
        }
    }
}
