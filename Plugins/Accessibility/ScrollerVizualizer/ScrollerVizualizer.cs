using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CommonServices.Accessibility;
using HighlightModel;

namespace ScrollerVizualizer
{
    [Plugin( ScrollerVizualizer.PluginIdString,
        PublicName = PluginPublicName,
        Version = ScrollerVizualizer.PluginIdVersion,
        Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScrollerVizualizer : IPlugin
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        internal const string PluginIdString = "{D58F0DC4-E45D-47D9-9AA0-88B53B3B2351}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Scroller Vizualizer";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHighlighterService> Scroller { get; set; }

        List<IVizualizableHighlightableElement> Vizualizables { get; set; }
        Vizualization _window;
        VizualizationViewModel _windowVm;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            Vizualizables = new List<IVizualizableHighlightableElement>();
            return true;
        }

        public void Start()
        {
            if( Scroller.Status == InternalRunningStatus.Started )
            {
                Init( Scroller.Service );
            }
            Scroller.ServiceStatusChanged += ( o, e ) =>
            {
                if( e.Current == InternalRunningStatus.Started )
                {
                    Init( Scroller.Service );
                }
            };
            Scroller.ServiceStatusChanged += ( o, e ) =>
            {
                if( e.Current == InternalRunningStatus.Stopped )
                {
                    Close( Scroller.Service );
                }
            };
        }

        void Init( IHighlighterService highlighter)
        {
            highlighter.BeginHighlight += OnBeginHilightElement;
            highlighter.EndHighlight += OnEndHilightElement;
            highlighter.ElementRegisteredOrUnregistered += ( o, e ) =>
            {
                if( e.HasRegistered )
                {
                    OnElementRegistered( e.Element );
                }
                else
                {
                    OnElementUnregistered( e.Element );
                }
            };
            _windowVm = new VizualizationViewModel( Vizualizables, highlighter );
            _window = new Vizualization( _windowVm );
            _window.Show();
        }

        void Close(IHighlighterService highlighter)
        {
            highlighter.BeginHighlight -= OnBeginHilightElement;
            highlighter.EndHighlight -= OnEndHilightElement;
        }

        void OnBeginHilightElement( object sender, HighlightEventArgs elem )
        {
            Console.WriteLine( "BEGIN H" );
        }

        void OnElementRegistered(IHighlightableElement element)
        {
            IVizualizableHighlightableElement vEl = element as IVizualizableHighlightableElement;
            if(vEl != null)
            {
                Vizualizables.Add( vEl );
            }
        }

        void OnElementUnregistered( IHighlightableElement element )
        {
            IVizualizableHighlightableElement vEl = element as IVizualizableHighlightableElement;
            if( vEl != null )
            {
                Vizualizables.Remove( vEl );
            }
        }

        void OnEndHilightElement( object sender, HighlightEventArgs elem )
        {
            Console.WriteLine( "END H" );
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        #endregion
    }
}