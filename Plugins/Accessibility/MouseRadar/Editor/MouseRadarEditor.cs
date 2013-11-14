using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using CK.Context;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

namespace MouseRadar.Editor
{

    [Plugin( MouseRadarEditor.PluginIdString,
           PublicName = MouseRadarEditor.PluginPublicName,
           Version = MouseRadarEditor.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadarEditor : IPlugin
    {
        internal const string PluginIdString = "{275B0E68-B880-463A-96E5-342C8E31E229}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Radar Editor";

        EditorViewModel _editor;
        [ConfigurationAccessor( MouseRadar.PluginIdString )]
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardDriver KeyboardHook { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerInput { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IWindowManager wnd = IoC.Get<WindowManager>();
            _editor = new EditorViewModel( Configuration ) { Context = Context };
            wnd.ShowWindow( _editor );
        }

        public void Stop()
        {
            _editor.Stopping = true;
            _editor.TryClose();
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
