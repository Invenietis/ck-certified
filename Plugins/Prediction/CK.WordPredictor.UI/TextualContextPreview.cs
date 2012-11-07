using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CK.WordPredictor.UI.ViewModels;

namespace CK.WordPredictor.UI
{

    [Plugin( "{E9D02BE8-B1CA-4057-8E74-2A89C411565C}", PublicName = "Word Prediction UI - TextualContext Preview", Categories = new string[] { "Prediction", "Visual" } )]
    public class TextualContextPreview : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            TextualContextPreviewViewModel previewVm = new TextualContextPreviewViewModel( TextualContextService );
            TextualContextPreviewWindow preview = new TextualContextPreviewWindow( previewVm );
            preview.Width = 600;
            preview.Height = 300;
            preview.Show();
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }
    }
}
