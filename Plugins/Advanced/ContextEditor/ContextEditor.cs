using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using CK.Context;
using CK.Core;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Windows.Config;
using ContextEditor.ViewModels;

namespace ContextEditor
{
    [Plugin( ContextEditor.PluginIdString,
        PublicName = PluginPublicName,
        Version = ContextEditor.PluginIdVersion,
        Categories = new string[] { "Visual", "Advanced" } )]
    public class ContextEditor : IPlugin
    {
        const string PluginIdString = "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "ContextEditor";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IKeyboardContext> KeyboardContext { get; set; }

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        ContextEditorBootstrapper bootstrap;
        WindowManager w = new WindowManager();
        AppViewModel _appViewModel;

        public bool Setup( IPluginSetupInfo info )
        {
            bootstrap = new ContextEditorBootstrapper();
            return bootstrap != null;
        }


        public void Start()
        {
            w = new WindowManager();
            _appViewModel = new AppViewModel( this );
            dynamic settings = new ExpandoObject();

            settings.SizeToContent = SizeToContent.WidthAndHeight;
            settings.Height = 800;
            settings.Width = 800;
            w.ShowWindow( _appViewModel, null, settings );
        }

        public void Stop()
        {
            Window win = _appViewModel.GetView( null ) as Window;
            if( win != null )
                win.Close();
        }

        public void Teardown()
        {
            _appViewModel = null;
            w = null;
        }
    }
}

