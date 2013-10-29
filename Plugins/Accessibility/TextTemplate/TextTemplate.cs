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

        public override bool Setup(IPluginSetupInfo info)
        {
            _editor = new TemplateEditor();
            Template p = Template.Load("blabla balbas qscmqmsc  poi {{caca}} sklqskcj qslkcjqkskjeziofzef^$i<sd sd {{nom}} qmskdlqsc {{caca}} qscqscqsccqsc");
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
                                Exec(m);
            }
        }

        public void Exec(string template)
        {
            /*
            if (_actions.ContainsKey(actionKey))
                _actions[actionKey]();
             * */
        }
    }
}
