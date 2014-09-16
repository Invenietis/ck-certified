#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\ScreenScroller\ScreenScroller.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WindowManager.Model;
using CommonServices.Accessibility;
using HighlightModel;
using ScreenScroller.Resources;

namespace ScreenScroller
{
    [Plugin( ScreenScrollerPlugin.PluginIdString,
           PublicName = ScreenScrollerPlugin.PluginPublicName,
           Version = ScreenScrollerPlugin.PluginIdVersion )]
    public class ScreenScrollerPlugin : NodeViewModel, IPlugin, IVisualizableHighlightableElement, IRootNode
    {
        internal const string PluginIdString = "{AE25D80B-B927-487E-9274-48362AF95FC0}";
        readonly Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "0.0.1";
        const string PluginPublicName = "Screen Scroller";

        IList<Screen> _screens;

        public string BackgroundColor { get; set; }
        public int ClickDepth { get; set; }
        public int MaxLapCount { get; set; }

        public int SquareSize { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IHighlighterService> Highlighter { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<ITopMostService> TopMostService { get; set; }

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
            ClickDepth = Config.User.GetOrSet<int>( "ClickDepth", 5 );
            SquareSize = Config.User.GetOrSet<int>( "SquareSize", 2 );
            MaxLapCount = Config.User.GetOrSet<int>( "MaxLapCount", 2 );
            BackgroundColor = Config.User.GetOrSet<string>( "BackgroundColor", "Black" );

            InitializeGrid();

            InitializeHighligther();

            InitializeTopMost();

            Config.ConfigChanged += OnConfigChanged;
        }

        private void InitializeGrid()
        {
            foreach( System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens.Reverse() )
            {
                var node = new NodeViewModel( this, s.Bounds.Top, s.Bounds.Left, SquareSize * SquareSize, ClickDepth, false );

                ChildNodes.Add( node );
                var screen = new Screen();
                screen.DataContext = node;
                screen.Show();

                //A Window's Left & Top properties can't be bound. 
                //Every time the framework sets it (when shown, moved, resized..) the binding is flushed. 
                screen.Left = s.Bounds.Left;
                screen.Top = s.Bounds.Top;
                screen.WindowState = WindowState.Maximized;
                _screens.Add( screen );
            }
        }

        readonly ResourceDictionary _resourceDictionary = new ResourceDictionary()
        {
            Source = new Uri( "pack://application:,,,/ScreenScroller;component/Views/Resources.xaml", UriKind.Absolute )
        };

        public ResourceDictionary ImageDictionary { get { return _resourceDictionary; } }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => c.UniqueId.Equals( this.PluginGuid ) ) && !String.IsNullOrEmpty( e.Key ) )
            {
                switch( e.Key )
                {
                    case "MaxLapCount":
                        MaxLapCount = (int)e.Value;
                        break;
                    case "ClickDepth":
                        ClickDepth = (int)e.Value;
                        break;
                    case "SquareSize":
                        SquareSize = (int)e.Value;
                        break;
                    case "BackgroundColor":
                        BackgroundColor = e.Value.ToString();
                        break;
                    default:
                        return;
                }
                ExitAll();
                CurrentNode = null;
                ChildNodes.Clear();
                InitializeGrid();
            }
        }

        public void Stop()
        {
            Config.ConfigChanged += OnConfigChanged;
            UninitializeTopMost();
            UnInitializeHighlighter();

            foreach( var screen in _screens )
            {
                screen.Close();
            }
        }

        public void Teardown()
        {
        }

        #region ITopMostService Members

        void InitializeTopMost()
        {
            RegisterTopMost();
            TopMostService.ServiceStatusChanged += OnTopMostServiceStatusChanged;
        }
        void UninitializeTopMost()
        {
            TopMostService.ServiceStatusChanged -= OnTopMostServiceStatusChanged;
            UnregisterTopMost();
        }

        void RegisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted )
            {
                foreach( var screen in _screens )
                {
                    TopMostService.Service.RegisterTopMostElement( "150", screen );
                }
            }
        }

        void UnregisterTopMost()
        {
            if( TopMostService.Status.IsStartingOrStarted )
            {
                foreach( var screen in _screens )
                {
                    TopMostService.Service.UnregisterTopMostElement( screen );
                }
            }
        }

        void OnTopMostServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Started )
            {
                foreach( var screen in _screens )
                {
                    TopMostService.Service.RegisterTopMostElement( "150", screen );
                }
            }
            else if( e.Current == InternalRunningStatus.Stopping )
            {
                foreach( var screen in _screens )
                {
                    TopMostService.Service.UnregisterTopMostElement( screen );
                }
            }
        }

        #endregion

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
                ProcessTick( scrollingDirective );
            }
            return scrollingDirective;
        }

        private void ProcessTick( ScrollingDirective scrollingDirective )
        {
            //if we have just entered the root, we need to highlight all the screens, except the one that has been entered.
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
            if( !CurrentNode.MoveNext() ) //if the node was at the end of its laps
            {
                if( CurrentNode.IsRoot || (CurrentNode.Parent.IsRoot && CurrentNode.Parent.ChildNodes.Count == 1) ) //we are getting out of the root level, so we release the scroller so that it can scroll on the other devices.
                {
                    CurrentNode = null;
                    _entered = false;
                    scrollingDirective.NextActionType = ActionType.MoveNext;
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
            scrollingDirective.NextActionType = ActionType.StayOnTheSame;

            if( CurrentNode == null )
            {
                if( ChildNodes.Count == 1 )
                {
                    CurrentNode = ChildNodes.Single();
                    CurrentNode.Entered();
                }
                else
                {
                    CurrentNode = this;
                }
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
                    NodeViewModel selectedNode = CurrentNode.ChildNodes[CurrentNode.CurrentIndex];
                    CK.InputDriver.MouseProcessor.MoveMouseToAbsolutePosition( (int)(selectedNode.OffsetWidth + (selectedNode.Width / 2)), (int)(selectedNode.OffsetHeight + (selectedNode.Height / 2)) );

                    CurrentNode.ChildNodes.ElementAt( CurrentNode.CurrentIndex ).IsHighlighted = false;
                    CurrentNode = null;
                    _entered = false;
                    ExitAll();
                    scrollingDirective.NextActionType = ActionType.GoToAbsoluteRoot;
                }
            }

            scrollingDirective.ActionTime = ActionTime.Immediate;
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
            Highlighter.Service.UnregisterTree( "ScreenScroller", this );
        }

        private void RegisterHighlighter()
        {
            Highlighter.Service.RegisterTree( "ScreenScroller", R.ScreenScrollerName, this );
        }

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

        public new int Width
        {
            get { return 0; }
        }

        public new int Height
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

        #region IVizualizableHighlightableElement Members

        public string ElementName
        {
            get { return PluginPublicName; }
        }

        public string VectorImagePath
        {
            get { return "F1M-291.695,57.6518L-283.906,49.558 -312.88,21.6882C-308.319,18.7482,-302.404,16.1439,-294.691,14.278L-347.906,-5.82477 -327.806,48.5737C-327.806,48.5737,-329.177,38.7078,-321.354,29.1245L-291.695,57.6518z"; }
        }

        #endregion
    }
}
