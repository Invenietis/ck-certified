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

        IList<Screen> _screens;

        public int ClickDepth { get; set; }
        public int MaxLapCount { get; set; }
        public int GridChildrenCount { get; set; }
        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            Root = this;
            Parent = this;
            ChildNodes = new System.Collections.ObjectModel.ObservableCollection<NodeViewModel>();
            _screens = new List<Screen>();

            return true;
        }

        public void Start()
        {
            ClickDepth = 5;
            GridChildrenCount = 4;
            MaxLapCount = 2;

            foreach( System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens.Reverse() )
            {
                NodeViewModel node = new NodeViewModel( this, GridChildrenCount, ClickDepth, false );

                ChildNodes.Add( node );
                Screen screen = new Screen();
                screen.DataContext = node;
                screen.Show();

                //A Window's Left & Top properties can't be bound. 
                //Every time the framework sets it (when shown, moved, resized..) the binding is flushed. 
                screen.Left = s.Bounds.Left;
                screen.Top = s.Bounds.Top;
                _screens.Add( screen );
            }

            InitializeHighligther();
        }

        private void Pause()
        {
        }

        public void Stop()
        {
            UnInitializeHighlighter();
            foreach( var screen in _screens )
            {
                screen.Close();
            }
        }

        public void Teardown()
        {
        }

        #region Hightlight Implementation

        bool _entered = false;
        bool _hasJustBeenEntered = false;

        NodeViewModel _currentNode;
        /// <summary>
        /// Gets the node which is dispatching the scrolls on its children
        /// </summary>
        public NodeViewModel CurrentNode
        {
            get
            {
                return _currentNode;
            }
            private set
            {
                NodeViewModel previousNode = _currentNode;

                _currentNode = value;

                int previousLevel = previousNode == null ? -1 : previousNode.Level;
                int currentLevel = _currentNode == null ? -1 : _currentNode.Level;

                if( LevelChanged != null )
                    LevelChanged( this, new LevelChangedEventArgs( previousLevel, currentLevel ) );
            }
        }
        public event EventHandler<LevelChangedEventArgs> LevelChanged;

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
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
                //if we have just entered the root, we need to un highlight all the screens, except the one that has been entered.
                if( _hasJustBeenEntered )
                {
                    _hasJustBeenEntered = false;
                    foreach( var node in ChildNodes )
                    {
                        if( node.Index != CurrentIndex )
                            node.IsHighlighted = false;
                    }
                }

                //And we move to the next node
                if( !CurrentNode.MoveNext() ) //if the node was at the end at its laps
                {
                    if( CurrentNode.IsRoot )
                    {
                        CurrentNode = null;
                        _entered = false;
                        scrollingDirective.NextActionType = ActionType.Normal;
                    }
                    else
                    {
                        //if the Node had finished its last lap during the last MoveNext, we go up one level
                        CurrentNode = CurrentNode.Parent;
                        CurrentNode.Entered();
                        CurrentNode.MoveNext();
                    }
                }
            }
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            if( !_entered ) //we were scrolling the plugin itself (we hadn't entered yet), which had highlighted all the screens
            {
                foreach( var node in ChildNodes )
                {
                    node.IsHighlighted = false;
                }
            }
            else
            {
                CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex ).IsHighlighted = false;
            }

            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            scrollingDirective.NextActionType = ActionType.StayOnTheSameLocked;

            if( CurrentNode == null )
            {
                CurrentNode = this;
                _hasJustBeenEntered = true;
                _entered = true;
            }
            else
            {
                if( CurrentNode.Level < ClickDepth - 1 )
                {
                    CurrentNode = CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex );
                    CurrentNode.Entered();
                }
                else
                {
                    MessageBox.Show( "Click !" );
                    CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex ).IsHighlighted = false;
                    CurrentNode = null;
                    _entered = false;
                    ExitAll();
                    scrollingDirective.NextActionType = ActionType.AbsoluteRoot;
                }
            }

            return scrollingDirective;
        }

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

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }
    }
}
