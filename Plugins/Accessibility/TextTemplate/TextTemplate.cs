using BasicCommandHandlers;
using CK.Core;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

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

        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public IService<ISendStringService> SendService { get; set; }

        public override bool Setup(IPluginSetupInfo info)
        {
            _viewModel = new TemplateEditorViewModel();
            _viewModel.TemplateValidated += (o, e) =>
            {
                string generatedText = _viewModel.Template.GenerateFormatedString();
                Console.WriteLine(generatedText);
                _editor.WindowState = System.Windows.WindowState.Minimized;
                SendFormatedTemplate();
                _editor.Close();
            };
            _viewModel.Canceled += (o, e) =>
            {
                _editor.Close();
            };
            return base.Setup(info);
        }

        public override void Start()
        {
            base.Start();
            LaunchEditor(@"J'aime les {{fruit}}s bien {{couleur}}s mais je n'aime pas la couleur {{couleur}}.Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam eu dolor sed tortor congue egestas commodo et quam. Proin tempus bibendum sem, sed sagittis quam bibendum eu. Vestibulum ullamcorper arcu mi, at laoreet sem luctus at. Nulla tristique lacus sit amet augue imperdiet luctus. Pellentesque massa nisl, viverra ut interdum sed, vestibulum sit amet lectus. Fusce tristique aliquet lectus dignissim iaculis. Ut ac neque eleifend, eleifend justo eu, mollis metus. Nunc varius leo at orci sagittis, non volutpat dolor suscipit. Nunc tempus eget justo non volutpat. Etiam scelerisque, elit non gravida aliquam, turpis erat iaculis nulla, nec fermentum eros nibh vel mauris. Curabitur eu est ut ipsum facilisis commodo. Suspendisse molestie est sit amet magna scelerisque, vitae rutrum est pellentesque.

Pellentesque in porttitor risus, vitae sagittis nunc. Aenean {{truc}}facilisis erat vitae tortor vehicula, sit amet pellentesque nisi euismod. Integer eu diam consectetur, varius tellus id, volutpat mauris. Curabitur in sem libero. {{Vivamus}} nunc enim, sollicitudin at enim non, feugiat elementum felis. Sed non velit semper, dapibus ligula a, auctor elit. Sed non sapien vitae nulla fermentum facilisis eu sed ipsum. Integer nec ante lectus. Mauris pretium nisi non fermentum aliquam. Morbi imperdiet, orci quis malesuada ultrices, est massa tristique sem, sit amet placerat neque libero in ante. Nam a tristique enim. Vivamus eu interdum arcu. Quisque gravida sapien mi, volutpat fringilla leo ornare in.

Cras non lorem facilisis, facilisis felis sed, eleifend augue. Praesent vitae sagittis nibh. Aliquam pharetra semper justo, quis euismod nibh lacinia quis. Quisque feugiat, felis sed malesuada vehicula, metus leo dictum dui, eget blandit eros ligula ac libero. Praesent feugiat est libero, quis vehicula felis gravida mollis. Proin convallis risus id aliquam porta. Suspendisse non feugiat sem. Maecenas in justo sit amet massa iaculis dignissim. Fusce volutpat magna in tortor rutrum iaculis. Praesent dictum imperdiet odio, at luctus risus tempor vel. Pellentesque eget ante nunc. Donec aliquet sem at massa bibendum commodo. Donec quis nisl magna. Nullam nisl velit, ultrices eu nisl gravida, tincidunt condimentum massa. Aenean lobortis dui sit amet tellus porttitor pellentesque.
{{Nom}}  {{Prénom}} ...");
            Highlighter.Service.RegisterTree(this);
            Highlighter.Service.BeginHighlight += (o, e) =>
            {
                if(e.Element == this)
                {
                    foreach(var elem in Children)
                    {
                        IActionableElement aElem = elem as IActionableElement;
                        if (aElem != null && aElem.ActionType == ActionType.UpToParent) aElem.ActionType = ActionType.Normal;
                    }
                }
                else 
                {
                    if (e.Element is IHighlightable)
                    {
                        IHighlightable h = (IHighlightable)e.Element;
                        h.IsHighlighted = true;
                    }
                    if (e.Element is IText)
                    {
                        IText t = (IText)e.Element;
                        _viewModel.Template.TextFragments.IndexOf(x => x.IsHighlighted == true);
                        Console.WriteLine("Text highlighted : " + t.Text);
                        _editor.FocusOnElement(t);
                    }
                }
            };
            Highlighter.Service.SelectElement += (o, e) =>
            {
                if(e.Element is IText)
                {
                    IText t = (IText)e.Element;
                    t.ActionType = ActionType.UpToParent;
                }
                else if (e.Element is ICommand)
                {
                    ICommand cmd = e.Element as ICommand;
                    cmd.Execute(null);
                }
            };
            Highlighter.Service.EndHighlight += (o, e) =>
            {
                if (e.Element is IHighlightable)
                {
                    IHighlightable h = (IHighlightable)e.Element;
                    h.IsHighlighted = false;
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
            
            //Ok Button
            list.Add(_viewModel.ValidateTemplate);

            //Cancel button
            list.Add(_viewModel.Cancel);
            _children = new CKReadOnlyListOnIList<IHighlightableElement>(list);
            _editor.Show();
        }
        
        public void SendFormatedTemplate()
        {
            SendService.Service.SendString(_viewModel.Template.GenerateFormatedString());
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
