using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using CK.Core;
using CK.Plugin.Config;
using HighlightModel;

namespace KeyScroller
{
    public abstract class ScrollingStrategy : IScrollingStrategy, IHighlightableElement
    {
        protected IPluginConfigAccessor Configuration { get; set; }
        protected Timer Timer { get; set; }
        protected ScrollingDirective LastDirective { get; set; }
        protected IHighlightableElement PreviousElement { get; set; }
        protected ITreeWalker Johnnie { get; set; }     //Big up to Olivier Spineli
        protected Dictionary<string, IHighlightableElement> Elements { get; set; }

        public ScrollingStrategy()
        {
            Johnnie = new Walker();
        }

        protected virtual void OnInternalBeat( object sender, EventArgs e )
        {
            Console.WriteLine( "BEAT \n" );
            if( LastDirective == null ) LastDirective = new ScrollingDirective( ActionType.MoveNext, ActionTime.NextTick );

            //Saving the currently highlighted element
            PreviousElement = Johnnie.Current;

            //Move the cursor to the next element
            MoveNext( LastDirective.NextActionType );

            //End highlight on the previous element (if different from the current one)
            if( PreviousElement != null )
                FireEndHighlight( PreviousElement, Johnnie.Current );

            //Begin highlight on the current element (even if the previous element is also the current element, we send the beginhighlight to give the component the beat)
            if( Johnnie.Current != null )
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
        /// <param name="action"></param>
        public virtual void MoveNext( ActionType action )
        {
            if( Johnnie.Current == null) 
                Johnnie.GoTo( this );

            //False if there are registered elements
            if( Johnnie.Current == this && Children.Count == 0 ) return;

            // reset the action type
            LastDirective.NextActionType = ActionType.MoveNext;

            switch(action)
            {
                case ActionType.MoveNext :
                    if( !Johnnie.MoveNext() )
                        MoveNext(ActionType.UpToParent);
                    break;
                case ActionType.UpToParent:
                    if( !Johnnie.UpToParent() )
                        MoveNext(ActionType.MoveToFirst);
                    break;
                case ActionType.EnterChild :
                    if( !Johnnie.EnterChild() )
                        MoveNext(ActionType.MoveNext);
                    break;
                case ActionType.MoveToFirst :
                    Johnnie.MoveFirst();
                    break;
                case ActionType.MoveToLast :
                    Johnnie.MoveLast();
                    break;
                case ActionType.StayOnTheSame :
                    LastDirective.NextActionType = ActionType.StayOnTheSame;
                    break;
                default :
                     //StayOnTheSameOnce
                    break;
            }

            ProcessSkipBehavior();
        }

        /// <summary>
        /// Read the the SkipBehavior of the current element and process it.
        /// </summary>
        protected virtual void ProcessSkipBehavior()
        {
            switch(Johnnie.Current.Skip)
            {
                case SkippingBehavior.Skip :
                    MoveNext(ActionType.MoveNext);
                    break;
                case SkippingBehavior.EnterChildren :
                    MoveNext( ActionType.EnterChild );
                    break;
                default : 
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
            if( Johnnie.Current != null )
            {
                LastDirective = Johnnie.Current.SelectElement( LastDirective );
                EnsureReactivity();
            }
        }

        /// <summary>
        /// Calls the BeginHighlight method of the current IHighlightableElement
        /// It also sets LastDirective to the ScrollingDirective object returned by the call to BeginHighlight.
        /// </summary>
        protected virtual void FireBeginHighlight()
        {
            if( Johnnie.Current != null )
            {
                LastDirective = Johnnie.Current.BeginHighlight( new BeginScrollingInfo( Timer.Interval, PreviousElement ), LastDirective );
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

        public virtual bool IsStarted
        {
            get;
            protected set;
        }

        public abstract string Name
        {
            get;
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

            Timer.Start();
            IsStarted = true;
        }

        public virtual void Stop()
        {
            if( Timer.Enabled )
            {
                FireEndHighlight( Johnnie.Current, null );
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
            Johnnie.GoTo( element );
        }

        public virtual void Pause( bool forceEndHighlight )
        {
            if( Timer.Enabled )
            {
                if( forceEndHighlight )
                {
                    FireEndHighlight( Johnnie.Current, null );
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
            if( Johnnie.Current != null )
            {
                if( Johnnie.Current.Children.Count > 0 ) LastDirective.NextActionType = ActionType.EnterChild;
                else
                {
                    LastDirective.NextActionType = ActionType.StayOnTheSameOnce;
                }
                FireSelectElement();
            }
        }

        public virtual void ElementUnregistered( HighlightModel.IHighlightableElement element )
        {
            Johnnie.GoTo( this );
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
            get { return false; }
        }

        #endregion
    }
}
