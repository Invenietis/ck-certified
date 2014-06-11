using CK.Core;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CommonServices;
using CommonServices.Accessibility;
using HighlightModel;
using System;
using System.Linq;
using TextTemplate.Resource;

namespace TextTemplate
{
    [Plugin( "{DD0D0FBA-9FC2-48FA-B3D1-6CE9CB2D133E}",
        Categories = new string[] { "Accessibility" },
        Version = "1.0.0",
        PublicName = "Text Template" )]
    public class TextTemplate : IPlugin, IHighlightableElement, ITextTemplateService
    {
        public static readonly string PlaceholderOpenTag = "{{";
        public static readonly string PlaceholderCloseTag = "}}";
        bool _isHighlightable = false;

        const string CMD = "placeholder";
        TemplateEditor _editor;
        TemplateEditorViewModel _viewModel;

        const string HIGHLIGH_REGISTER_ID = "TextTemplate";
        string HIGHLIGH_REGISTER_DISPLAY_NAME = R.TextTemplate;

        ICKReadOnlyList<IHighlightableElement> _children;

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendService { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _viewModel = new TemplateEditorViewModel();
            _viewModel.TemplateValidated += ( o, e ) =>
            {
                string generatedText = _viewModel.Template.GenerateFormatedString();
                //Console.WriteLine( generatedText );
                _editor.WindowState = System.Windows.WindowState.Minimized;
                SendFormatedTemplate();
                _editor.Close();
            };
            _viewModel.Canceled += ( o, e ) =>
            {
                _editor.Close();
            };

            return true;
        }

        public void Start()
        {
            if( !_isHighlightable ) return;

            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                Highlighter.Service.RegisterTree( HIGHLIGH_REGISTER_ID, HIGHLIGH_REGISTER_DISPLAY_NAME, this );
            }

            Highlighter.ServiceStatusChanged += ( o, e ) =>
            {
                if( e.Current == InternalRunningStatus.Started )
                {
                    Console.WriteLine( "dzeidkzpoekdpozekpo" );
                    Highlighter.Service.RegisterTree( HIGHLIGH_REGISTER_ID, HIGHLIGH_REGISTER_DISPLAY_NAME, this );
                }
                else if( e.Current == InternalRunningStatus.Stopping )
                {
                    Console.WriteLine( "tagada fuck" );
                    Highlighter.Service.UnregisterTree( HIGHLIGH_REGISTER_ID, this );
                }
            };

            Skip = SkippingBehavior.Skip;
        }

        public void OpenEditor( string template )
        {
            _viewModel.Template = Template.Load( template, this );
            if( _editor != null ) _editor.Close();
            _editor = new TemplateEditor( _viewModel );
            var list = _viewModel.Template.TextFragments.Where( t => t.IsEditable == true )
                .Cast<IHighlightableElement>()
                .Distinct()
                .ToList();

            //Cancel button
            list.Add( _viewModel.Cancel );

            //Ok Button
            list.Add( _viewModel.ValidateTemplate );

            _children = new CKReadOnlyListOnIList<IHighlightableElement>( list );
            Skip = SkippingBehavior.None;
            _editor.Show();
            _editor.Activate();
            _editor.Closed += ( o, e ) =>
            {
                Skip = SkippingBehavior.Skip;
            };

            if( _isHighlightable && Highlighter.Status == InternalRunningStatus.Started )
                Highlighter.Service.HighlightImmediately( this );
        }

        public void SendFormatedTemplate()
        {
            SendService.Service.SendString( _viewModel.Template.GenerateFormatedString() );
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

        public void FocusOnElement( IText text )
        {
            if( _editor != null ) _editor.FocusOnElement( text );
        }

        public void RemoveFocus( IText text )
        {
            if( _editor != null ) _editor.RemoveFocus( text );
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            _viewModel.IsWindowHighlighted = true;

            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            _viewModel.IsWindowHighlighted = false;
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            _editor.Activate();
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }

        #region IPlugin Members


        public void Stop()
        {
            if( Highlighter.Status.IsStartingOrStarted )
                Highlighter.Service.UnregisterTree( HIGHLIGH_REGISTER_ID, this );
        }

        public void Teardown()
        {
        }

        #endregion

        #region ITextTemplateService Members

        public string OpentTag
        {
            get { return TextTemplate.PlaceholderOpenTag; }
        }

        public string CloseTag
        {
            get { return TextTemplate.PlaceholderCloseTag; }
        }

        #endregion
    }

    public class Command
    {
        static readonly string SeparationToken = ":";
        public string Name { get; private set; }

        public string Content { get; private set; }

        public Command( string cmd )
        {
            int pos = cmd.IndexOf( SeparationToken );
            if( pos < 0 ) return;

            Name = cmd.Substring( 0, pos );
            Content = cmd.Substring( pos + SeparationToken.Length );
        }
    }
}
