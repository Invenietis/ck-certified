using BasicCommandHandlers;
using CK.Plugin;
using CommonServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextTemplate
{
    [Plugin("{DD0D0FBA-9FC2-48FA-B3D1-6CE9CB2D133E}",
        Categories = new string[] { "Accessibility" },
        Version = "1.0.0",
        PublicName = "Text Template")]
    public class TextTemplate : BasicCommandHandler
    {
        const string CMD = "placeholder:";
        TemplateEditor _editor;
        Template _current;

        public override bool Setup(IPluginSetupInfo info)
        {
            LaunchEditor("blabla balbas qscmqmsc  poi \r\n{{caca}} sklqskcj qslkcjqkskjeziofzef^$i<sd sd {{nom}}" + Environment.NewLine + "qmskdlqsc {{caca}} qscqscqsccqsc");

            return base.Setup(info);
        }

        protected override void OnCommandSent(object sender, CommandSentEventArgs e)
        {
            string cmd;
            string m;

            CommandParser p = new CommandParser(e.Command);

            if (!e.Canceled && p.IsIdentifier(out cmd) && cmd == CMD)
            {
                if (p.Match(CommandParser.Token.OpenPar))
                    if (p.IsString(out m))
                        if (p.Match(CommandParser.Token.ClosePar))
                            if (cmd == CMD)
                                LaunchEditor(m);
            }
        }

        public void LaunchEditor(string template)
        {
            _current = Template.Load(template);
            _editor = new TemplateEditor(_current);

            _editor.Closed += (o, e) =>
            {
                SendFormatedTemplate();
            };

            _editor.Show();
        }
        
        public void SendFormatedTemplate()
        {

        }
    }
}
