using System;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Context;

namespace ScreenScroller.Editor
{

    [Plugin( ScreenScrollerEditor.PluginIdString,
           PublicName = ScreenScrollerEditor.PluginPublicName,
           Version = ScreenScrollerEditor.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScreenScrollerEditor : IPlugin
    {
        internal const string PluginIdString = "{652CFF65-5CF7-4FE9-8FF5-45C5E2A942E6}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Screen scroller Editor";

        EditorViewModel _editor;
        EditorView _window;

        [ConfigurationAccessor( ScreenScrollerPlugin.PluginIdString )]
        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _editor = new EditorViewModel( Configuration, Context );
            _window = new EditorView() { DataContext = _editor };
            _window.Show();
        }

        public void Stop()
        {
            _window.Close();
        }

        public void Teardown()
        {
        }

        #endregion
    }
}
