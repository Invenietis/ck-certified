﻿using BasicCommandHandlers;
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
        const string CMD = "placeholder";
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
            Skip = SkippingBehavior.Skip;
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
            Command cmd = new Command(e.Command);
            if (!e.Canceled && cmd.Name == CMD)
            {
                LaunchEditor(cmd.Content);
            }
        }

        public void LaunchEditor(string template)
        {
            _viewModel.Template = Template.Load(template);
            if (_editor != null) _editor.Close();
            _editor = new TemplateEditor(_viewModel);
            var list = _viewModel.Template.TextFragments.Where(t => t.IsEditable == true)
                .Cast<IHighlightableElement>()
                .Distinct()
                .ToList();
            
            //Ok Button
            list.Add(_viewModel.ValidateTemplate);

            //Cancel button
            list.Add(_viewModel.Cancel);
            _children = new CKReadOnlyListOnIList<IHighlightableElement>(list);
            Skip = SkippingBehavior.None;
            _editor.Show();
            _editor.Closed += (o, e) =>
            {
                Skip = SkippingBehavior.Skip;
            };
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
            get;
            private set;
        }
    }

    public class Command
    {
        static readonly string SeparationToken = ":";
        public string Name { get; private set; }

        public string Content { get; private set; }

        public Command(string cmd)
        {
            int pos = cmd.IndexOf(SeparationToken);
            Name = cmd.Substring(0, pos);
            Content = cmd.Substring(pos + SeparationToken.Length);
        }
    }
}
