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
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginVersion, Categories = new string[] { "Visual", "Accessibility" } )]
    public class ScrollerVisualizer : IPlugin
    {
        #region Plugin description

        const string PluginGuidString = "{D58F0DC4-E45D-47D9-9AA0-88B53B3B2351}";
        const string PluginVersion = "1.0.0";
        const string PluginPublicName = "Scroller Visualizer";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginVersion, PluginPublicName );

        #endregion Plugin description

        bool _isInit;

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Scroller { get; set; }

        Visualization _window;
        VisualizationViewModel _windowVm;

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
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
                if( e.Current == InternalRunningStatus.Stopping )
                {
                    Close( Scroller.Service );
                }
            };
        }

        void Init( IHighlighterService highlighter)
        {
            if( _isInit ) return;

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
            _isInit = true;
        }

        void Close(IHighlighterService highlighter)
        {
            highlighter.BeginHighlight -= OnBeginHilightElement;
            highlighter.EndHighlight -= OnEndHilightElement;

            if( _window != null )
                _window.Close();
            _isInit = false;
        }

        void OnBeginHilightElement( object sender, HighlightEventArgs elem )
        {
        }

        void OnElementRegistered(IHighlightableElement element)
        {
            IVisualizableHighlightableElement vEl = element as IVisualizableHighlightableElement;
       
        }

        void OnElementUnregistered( IHighlightableElement element )
        {

            IVisualizableHighlightableElement vEl = element as IVisualizableHighlightableElement;
        }

        void OnEndHilightElement( object sender, HighlightEventArgs elem )
        {
        }

        public void Stop()
        {
            Close( Scroller.Service );
        }

        public void Teardown()
        {
        }

        #endregion
    }
}