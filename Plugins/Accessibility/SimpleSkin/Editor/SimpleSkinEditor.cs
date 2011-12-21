using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Keyboard.Model;
using System.Windows.Media;
using CommonServices;
using Caliburn.Micro;
using SimpleSkin;
using CK.Context;

namespace SimpleSkinEditor
{
    [Plugin( SimpleSkinEditor.PluginIdString,
        PublicName = PluginPublicName,
        Version = SimpleSkinEditor.PluginIdVersion,
        Categories = new string[] { "Visual", "Accessibility" } )]
    public class SimpleSkinEditor : IPlugin
    {
        public const string PluginIdString = "{402C9FF7-545A-4E3C-AD35-70ED37497805}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Simple skin editor";

        EditorViewModel _editor;

        [RequiredService]
        public IKeyboardContext KeyboardContext { get; set; }

        [RequiredService]
        public IContext Context { get; set; }

        [RequiredService]
        public ISkinService Skin { get; set; }

        [ConfigurationAccessor( "{36C4764A-111C-45e4-83D6-E38FC1DF5979}" )]
        public IPluginConfigAccessor EditedPluginConfiguration { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IWindowManager wnd = IoC.Get<WindowManager>();
            _editor = new EditorViewModel( KeyboardContext, EditedPluginConfiguration ) { Context = Context };
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
