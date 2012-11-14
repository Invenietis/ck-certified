using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Plugin;

namespace CK.Plugins.WizardFirstStart
{
    [Plugin( "{A4CD5FA6-C9ED-40D5-9BF7-B5C96ADA73C0}", PublicName = "Wizard First Start" )]
    public class WizardFirstStart : IPlugin
    {
        WizardViewModel _w;

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            WindowManager windowManager = new WindowManager();
            _w = new WizardViewModel();
            windowManager.ShowWindow( _w, null, null );
        }

        public void Stop()
        {
            _w.TryClose();
        }

        public void Teardown()
        {
        }
    }
}
