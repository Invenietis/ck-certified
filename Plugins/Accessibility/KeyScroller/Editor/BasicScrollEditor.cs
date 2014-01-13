using System;
using Caliburn.Micro;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices;

namespace KeyScroller.Editor
{
    [Plugin( BasicScrollEditor.PluginIdString,
           PublicName = PluginPublicName,
           Version = BasicScrollEditor.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class BasicScrollEditor : IPlugin
    {   
        internal const string PluginIdString = "{48D3977C-EC26-48EF-8E47-806E11A1C041}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "BasicScrollEditor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        EditorViewModel _editor;

        [RequiredService]
        public IContext Context { get; set; }

        [ConfigurationAccessor( KeyScrollerPlugin.PluginIdString )]
        public IPluginConfigAccessor BasicScrollConfiguration { get; set; }

        [ConfigurationAccessor( "{4E3A3B25-7FD0-406F-A958-ECB50AC6A597}" )]
        public IPluginConfigAccessor KeyboardTriggerConfiguration { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITriggerService TriggerService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> PointerInput { get; set; }
        
        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IWindowManager wnd = IoC.Get<WindowManager>();
            _editor = new EditorViewModel( BasicScrollConfiguration, KeyboardTriggerConfiguration, TriggerService ) { Context = Context };
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
