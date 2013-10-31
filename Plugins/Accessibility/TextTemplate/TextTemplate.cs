using BasicCommandHandlers;
using CK.Core;
using CK.Plugin;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
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
    public class TextTemplate : BasicCommandHandler, IHighlightableElement
    {
        const string CMD = "placeholder:";
        TemplateEditor _editor;
        TemplateEditorViewModel _viewModel;

        ICKReadOnlyList<IHighlightableElement> _children;

        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public IService<IHighlighterService> Highlighter { get; set; }

        public override bool Setup(IPluginSetupInfo info)
        {
            _viewModel = new TemplateEditorViewModel();
            _viewModel.TemplateValidated += (o, e) =>
            {
                string generatedText = _viewModel.Template.GenerateFormatedString();
                Console.WriteLine(generatedText);
                SendFormatedTemplate();
            };
            return base.Setup(info);
        }

        public override void Start()
        {
            base.Start();
            LaunchEditor("blabla balbas qscmqmsc  poi \r\n{{caca}} sklqskcj qslkcjqkskjeziofzef^$i<sd sd {{nom}}" + Environment.NewLine + "qmskdlqsc {{caca}} qscqscqsccqsc");
            Highlighter.Service.RegisterTree(this);
            Highlighter.Service.BeginHighlight += (o, e) =>
            {
                if(e.Element == this)
                {
                    foreach(var elem in Children)
                    {
                        IActionableElement aElem = (IActionableElement) elem;
                        if (aElem.ActionType == ActionType.UpToParent) aElem.ActionType = ActionType.Normal;
                    }
                }
                else if(e.Element is IText)
                {
                    IText t = (IText)e.Element;

                    t.IsHighlighted = true;
                    _viewModel.Template.TextFragments.IndexOf(x => x.IsHighlighted == true);
                    Console.WriteLine("Text highlighted : " + t.Text);
                    _editor.FocusOnElement(t);
                }
            };
            Highlighter.Service.SelectElement += (o, e) =>
            {
                if(e.Element is IText)
                {
                    IText t = (IText)e.Element;
                    t.ActionType = ActionType.UpToParent;
                }
            };
            Highlighter.Service.EndHighlight += (o, e) =>
            {
                if (e.Element is IText)
                {
                    IText t = (IText)e.Element;
                    t.IsHighlighted = false;
                }
            };
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
            _viewModel.Template = Template.Load(template);

            _editor = new TemplateEditor(_viewModel);
            var list = _viewModel.Template.TextFragments.Where(t => t.IsEditable == true)
                .Cast<IHighlightableElement>()
                .ToList();

            _children = new CKReadOnlyListOnIList<IHighlightableElement>(list);

            _editor.Show();
        }
        
        public void SendFormatedTemplate()
        {
            //TODO Send string
        }

        public CK.Core.ICKReadOnlyList<IHighlightableElement> Children
        {
            get 
            {
                return _children ?? CKReadOnlyListEmpty<IHighlightableElement>.Empty;
            }
        }

        public int X
        {
            get { return 0; }
        }

        public int Y
        {
            get { return 0; }
        }

        public int Width
        {
            get { return 0; }
        }

        public int Height
        {
            get { return 0; }
        }

        public SkippingBehavior Skip
        {
            get { return SkippingBehavior.None; }
        }
    }
}
