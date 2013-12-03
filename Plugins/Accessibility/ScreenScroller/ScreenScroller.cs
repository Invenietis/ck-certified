using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;
using CommonServices.Accessibility;
using System.Windows.Controls;
using HighlightModel;
using KeyScroller;
using System.Drawing;

namespace ScreenScroller
{
    [Plugin( ScreenScrollerPlugin.PluginIdString,
           PublicName = ScreenScrollerPlugin.PluginPublicName,
           Version = ScreenScrollerPlugin.PluginIdVersion )]
    public class ScreenScrollerPlugin : NodeViewModel, IPlugin, IHighlightableElement, IRootNode
    {
        const string PluginIdString = "{AE25D80B-B927-487E-9274-48362AF95FC0}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "0.0.1";
        const string PluginPublicName = "Screen Scroller";

        int _currentLoopCount = 0;

        public IPluginConfigAccessor Config { get; set; }
        public int MaxLoop { get; set; }
        public int MaxDepth { get; set; }
        public int GridChildrenNumber { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            MaxDepth = 4;
            GridChildrenNumber = 9;
            ChildNodes = new System.Collections.ObjectModel.ObservableCollection<NodeViewModel>();
            //foreach( System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens.Reverse() )
            //{
            //    ChildNodes.Add( new NodeViewModel( s.Bounds.Left, s.Bounds.Top, GridChildrenNumber, MaxDepth ) );
            //}
            ChildNodes.Add( new NodeViewModel( this, 0, 0, GridChildrenNumber, MaxDepth, false ) );
            ChildNodes.Add( new NodeViewModel( this, 1000, 0, GridChildrenNumber, MaxDepth, false ) );
            return true;
        }

        public void Start()
        {
            foreach( var node in ChildNodes )
            {
                Screen screen = new Screen();
                screen.DataContext = node;
                screen.Show();
            }
            InitializeHighligther();

        }

        private void Pause()
        {
        }

        public void Stop()
        {
            UnInitializeHighlighter();
            //foreach( var w in _attachedWindows )
            //{
            //    w.Close();
            //}
            //_attachedWindows.Clear();
        }

        public void Teardown()
        {
        }

        #region Hightlight Methods

        void OnHighlighterServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                UnregisterHighlighter();
            }
        }

        private void InitializeHighligther()
        {
            Highlighter.ServiceStatusChanged += OnHighlighterServiceStatusChanged;
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                RegisterHighlighter();
            }
        }

        private void UnInitializeHighlighter()
        {
            if( Highlighter.Status == InternalRunningStatus.Started )
            {
                UnregisterHighlighter();
            }
            Highlighter.ServiceStatusChanged -= OnHighlighterServiceStatusChanged;
        }

        private void UnregisterHighlighter()
        {
            Highlighter.Service.UnregisterTree( this );
        }

        private void RegisterHighlighter()
        {
            Highlighter.Service.RegisterTree( this );
        }

        #endregion


        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return CKReadOnlyListEmpty<IHighlightableElement>.Empty; }
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

        #endregion

        /// <summary>
        /// Gets the node which is dispatching the scrolls on its children
        /// </summary>
        public NodeViewModel CurrentNode { get; private set; }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            Console.Out.WriteLine( "BeginHighlight" );

            //If we haven't entered the children yet, we highlight all of the screens.
            //Once a first Select element has been triggered, we start scrolling the screens.
            if( CurrentNode == null )
            {
                foreach( var node in ChildNodes )
                {
                    node.IsHighlighted = true;
                }
            }
            else
            { 
                CurrentNode.MoveNext();
            }
            return scrollingDirective;
        }


        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( !_entered ) //we were scrolling the plugin itself, which had highlighted all the screens
            {
                foreach( var node in ChildNodes )
                {
                    node.IsHighlighted = false;
                }
            }
            else
            {
                CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex ).IsHighlighted = false;
                Console.Out.WriteLine( "EndHighlight" );
                Console.Out.WriteLine( "Current node : " + CurrentNode.Level );
            }

            return scrollingDirective;
        }

        bool _entered = false;
        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            if( CurrentNode == null )
            {
                CurrentNode = this;
                _entered = true;
            }
            else
            {
                CurrentNode = CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex );
            }

            CurrentNode.Entered();
            scrollingDirective.NextActionType = ActionType.StayOnTheSameLocked;
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }
    }
}
