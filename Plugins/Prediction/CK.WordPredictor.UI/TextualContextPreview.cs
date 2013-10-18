using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( "{E9D02BE8-B1CA-4057-8E74-2A89C411565C}", PublicName = "TextualContext - Echo", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextPreview : IPlugin
    {
        TextualContextPreviewViewModel _vm;
        TextualContextPreviewWindow _preview;

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<ITextualContextService> TextualContextService { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _vm = new TextualContextPreviewViewModel( TextualContextService );
            _preview = new TextualContextPreviewWindow( _vm );
            _preview.Width = 600;
            _preview.Height = 300;
            _preview.Show();
        }

        public void Stop()
        {
            _vm.Dispose();
            _preview.Close();
        }

        public void Teardown()
        {
        }
    }
}
