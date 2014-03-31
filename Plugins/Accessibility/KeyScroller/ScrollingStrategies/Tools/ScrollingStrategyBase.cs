﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;

namespace Scroller
{
    public abstract class ScrollingStrategyBase : IScrollingStrategy, IHighlightableElement
    {
        protected IPluginConfigAccessor Configuration { get; set; }
        protected Timer Timer { get; set; }
        protected ScrollingDirective LastDirective { get; set; }
        protected IHighlightableElement PreviousElement { get; set; }
        protected ITreeWalker Walker { get; set; }     //Big up to Olivier Spineli
        protected Dictionary<string, IHighlightableElement> Elements { get; set; }

        bool CanMove()
        {
            if( Walker.Current == this && Children.Count( c => c.Skip != SkippingBehavior.Skip ) == 0 )
                return false;

            if( Elements.Values.All( e => e.Children.All(c => c.Skip == SkippingBehavior.Skip )) )
                return false;

            return true;
        }

        public ScrollingStrategyBase()
        {
            LastDirective = new ScrollingDirective( ActionType.MoveNext, ActionTime.NextTick );
            Walker = new Walker( this );
        }

        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            Console.WriteLine( "BEAT \n" );
            if( LastDirective == null ) LastDirective = new ScrollingDirective( ActionType.MoveNext, ActionTime.NextTick );

            //Saving the currently highlighted element
            PreviousElement = Walker.Current;

            //Move the cursor to the next element
            MoveNext( LastDirective.NextActionType );

            //End highlight on the previous element (if different from the current one)
            if( PreviousElement != null )
                FireEndHighlight( PreviousElement, Walker.Current );

            //Begin highlight on the current element (even if the previous element is also the current element, we send the beginhighlight to give the component the beat)
            if( Walker.Current != null )
                FireBeginHighlight();
        }

        protected virtual void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( u => u.UniqueId == ScrollerPlugin.PluginId.UniqueId ) )
            {
                if( e.Key == "Speed" )
                {
                    Timer.Interval = (double)(int)e.Value;
                }
            }
        }

        /// <summary>
        /// Move the cursor to the next element according to the given ActionType
        /// </summary>
        /// <param name="action">The ActionType that indicate the move direction</param>
        public virtual void MoveNext( ActionType action )
        {
            if( !CanMove() ) return;

            Console.WriteLine( "NEXT \n" );

            // defer action to the next tick
            if( LastDirective != null && LastDirective.ActionTime == ActionTime.Delayed )
            {
                LastDirective.ActionTime = ActionTime.NextTick;
                return;
            }

            //We retrieve parents that implement IHighlightableElementController
            var controlingParents = Walker.Parents.Where( ( i ) => { return i is IHighlightableElementController; } );

            foreach( IHighlightableElementController parent in controlingParents )
            {
                //we offer the possibility to change action
                action = parent.PreviewChildAction( Walker.Current, action );
            }

            foreach( IHighlightableElementController parent in controlingParents )
            {
                //inform that there is a new ActionType
                parent.OnChildAction( action );
            }

            // reset the action type
            LastDirective.NextActionType = ActionType.MoveNext;
            
            switch(action)
            {
                case ActionType.MoveNext :
                    if( !Walker.MoveNext() )
                        MoveNext(ActionType.UpToParent);
                    break;

                case ActionType.UpToParent:
                    if( !Walker.UpToParent() )
                        MoveNext(ActionType.MoveToFirst);
                    break;

                case ActionType.EnterChild :
                    if( !Walker.EnterChild() )
                        MoveNext(ActionType.MoveNext);
                    break;

                case ActionType.MoveToFirst :
                    Walker.MoveFirst();
                    break;

                case ActionType.MoveToLast :
                    Walker.MoveLast();
                    break;

                case ActionType.StayOnTheSame :
                    LastDirective.NextActionType = ActionType.StayOnTheSame;
                    break;

                case ActionType.GoToRelativeRoot :
                    Walker.GoToRelativeRoot();
                    MoveNext( ActionType.EnterChild );
                    break;

                default :
                    //StayOnTheSameOnce
                    LastDirective.NextActionType = ActionType.GoToRelativeRoot;
                    break;
            }

            ProcessSkipBehavior(action);
        }

        /// <summary>
        /// Read the the SkipBehavior of the current element and process it.
        /// </summary>
        protected virtual void ProcessSkipBehavior( ActionType action )
        {
            switch(Walker.Current.Skip)
            {
                case SkippingBehavior.Skip :
                    MoveNext(ActionType.MoveNext);
                    break;
                case SkippingBehavior.EnterChildren :

                    //False if the walker goes up. (Avoid infinite loop)
                    if( action != ActionType.UpToParent )
                        MoveNext( ActionType.EnterChild );
                    else
                        MoveNext( ActionType.MoveNext );
                    break;
                default :
                    //Enter in the module if it the only one registered
                    if( Walker.Current.IsHighlightableTreeRoot && Walker.Sibblings.Count( s => s.Skip != SkippingBehavior.Skip ) == 1 && Walker.Current.Children.Count( s => s.Skip != SkippingBehavior.Skip ) > 0 )
                    {
                        if( action != ActionType.UpToParent )
                            MoveNext( ActionType.EnterChild );
                        else
                            MoveNext( ActionType.MoveNext );
                        break;
                    }
                    break;
            }
        }

        /// <summary>
        /// if the directive is to react instantly, we stop the timer, simulate a tick, and relaunch the timer.
        /// </summary>
        internal void EnsureReactivity()
        {
            if( LastDirective != null )
            {
                if( LastDirective.ActionTime == ActionTime.Immediate )
                {
                    //Setting the ActionTime back to NextTick. Immediate has to be set explicitely at each step;
                    LastDirective.ActionTime = ActionTime.NextTick;

                    Timer.Stop();
                    OnInternalBeat( this, EventArgs.Empty );
                    //Console.Out.WriteLine( "Immediate !" );
                    Timer.Start();
                }
            }
        }

        /// <summary>
        /// Calls the SelectElement method of the current IHighlightableElement
        /// It also sets LastDirective to the ScrollingDirective object returned by the call to SelectElement.
        /// </summary>
        protected virtual void FireSelectElement()
        {
            if( Walker.Current != null )
            {
                LastDirective = Walker.Current.SelectElement( LastDirective );
                EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the BeginHighlight method of the current IHighlightableElement
        /// It also sets LastDirective to the ScrollingDirective object returned by the call to BeginHighlight.
        /// </summary>
        protected virtual void FireBeginHighlight()
        {
            if( Walker.Current != null )
            {
                LastDirective = Walker.Current.BeginHighlight( new BeginScrollingInfo( Timer.Interval, PreviousElement ), LastDirective );
                EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the EndHighlight method of the current IHighlightableElement
        /// It also sets _lastDirective to the ScrollingDirective object returned by the call to EndHighlight.
        /// </summary>
        protected virtual void FireEndHighlight( IHighlightableElement previousElement, IHighlightableElement element )
        {
            if( previousElement != null )
            {
                LastDirective = previousElement.EndHighlight( new EndScrollingInfo( Timer.Interval, previousElement, element ), LastDirective );
                EnsureReactivity();
            }
        }

        #region IScrollingStrategy Members

        public abstract string Name
        {
            get;
        }

        public virtual bool IsStarted
        {
            get;
            protected set;
        }

        public virtual void Setup( Timer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor config )
        {
            Timer = timer;
            Elements = elements;
            Configuration = config;
        }

        public virtual void Start()
        {
            if( IsStarted ) return;

            Timer.Elapsed += OnInternalBeat;
            Configuration.ConfigChanged += OnConfigChanged;
            
            //Reset the walker
            Walker.GoToAbsoluteRoot();
            Timer.Start();
            IsStarted = true;
        }

        public virtual void Stop()
        {
            if( Timer.Enabled )
            {
                FireEndHighlight( Walker.Current, null );
                Timer.Enabled = false;
            }
            Timer.Elapsed -= OnInternalBeat;
            Configuration.ConfigChanged -= OnConfigChanged;
            Timer.Stop();
            IsStarted = false;
        }

        public virtual void GoToElement( HighlightModel.IHighlightableElement element )
        {
            LastDirective = new ScrollingDirective( ActionType.MoveNext, ActionTime.Immediate );

            Walker.Current.EndHighlight( new EndScrollingInfo( Timer.Interval, PreviousElement, element ), LastDirective );
            Walker.GoTo( element );
            EnsureReactivity();
        }

        public virtual void Pause( bool forceEndHighlight )
        {
            if( Timer.Enabled )
            {
                if( forceEndHighlight )
                {
                    FireEndHighlight( Walker.Current, null );
                }
                Timer.Stop();
            }
        }

        public virtual void Resume()
        {
            if( !Timer.Enabled ) Timer.Start();
        }

        public virtual void OnExternalEvent()
        {
            if( Walker.Current != null )
            {
                if( Walker.Current.Children.Count > 0 ) LastDirective.NextActionType = ActionType.EnterChild;
                else
                {
                    LastDirective.NextActionType = ActionType.StayOnTheSameOnce;
                }
                FireSelectElement();
            }
        }

        public virtual void ElementUnregistered( HighlightModel.IHighlightableElement element )
        {
            GoToElement( this );
            EnsureReactivity();
        }

        #endregion

        #region IHighlightableElement Members

        public ICKReadOnlyList<IHighlightableElement> Children
        {
            get { return new CKReadOnlyListOnIList<IHighlightableElement>(Elements.Values.ToList()); }
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
            get { return SkippingBehavior.EnterChildren; }
        }

        public ScrollingDirective BeginHighlight( BeginScrollingInfo beginScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective EndHighlight( EndScrollingInfo endScrollingInfo, ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public ScrollingDirective SelectElement( ScrollingDirective scrollingDirective )
        {
            return scrollingDirective;
        }

        public bool IsHighlightableTreeRoot
        {
            get { return true; }
        }

        #endregion
    }
}