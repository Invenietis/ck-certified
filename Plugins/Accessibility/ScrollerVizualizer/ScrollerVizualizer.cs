using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;
using CommonServices.Accessibility;
using HighlightModel;

namespace ScrollerVisualizer
{
    [Plugin( ScrollerVisualizer.PluginIdString,
        PublicName = PluginPublicName,
        Version = ScrollerVisualizer.PluginIdVersion,
        Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScrollerVisualizer : IPlugin
    {
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        internal const string PluginIdString = "{D58F0DC4-E45D-47D9-9AA0-88B53B3B2351}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Scroller Visualizer";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IHighlighterService> Scroller { get; set; }

        List<IVisualizableHighlightableElement> Vizualizables { get; set; }
        Visualization _window;
        VisualizationViewModel _windowVm;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            Vizualizables = new List<IVisualizableHighlightableElement>();
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
            _windowVm = new VisualizationViewModel( highlighter );
            _window = new Visualization( _windowVm );
            _window.Show();
            _windowVm.Init( highlighter );
        }

        void Close(IHighlighterService highlighter)
        {
            highlighter.BeginHighlight -= OnBeginHilightElement;
            highlighter.EndHighlight -= OnEndHilightElement;
        }

        void OnBeginHilightElement( object sender, HighlightEventArgs elem )
        {
        }

        void OnElementRegistered(IHighlightableElement element)
        {
            IVisualizableHighlightableElement vEl = element as IVisualizableHighlightableElement;
            if(vEl != null)
            {
                Vizualizables.Add( vEl );
            }
        }

        void OnElementUnregistered( IHighlightableElement element )
        {
            IVisualizableHighlightableElement vEl = element as IVisualizableHighlightableElement;
            if( vEl != null )
            {
                Vizualizables.Remove( vEl );
            }
        }

        void OnEndHilightElement( object sender, HighlightEventArgs elem )
        {
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