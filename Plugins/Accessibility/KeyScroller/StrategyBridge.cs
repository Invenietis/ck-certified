﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using CK.Plugin.Config;
using HighlightModel;

namespace Scroller
{
    /// <summary>
    /// Make the bridge between the different implementations
    /// </summary>
    public class StrategyBridge : IScrollingStrategy
    {
        public static List<string> AvailableStrategies { get; private set; }
        IScrollingStrategy _current;
        Dictionary<string, IScrollingStrategy> Implementations { get; set; }

        /// <summary>
        /// Get the list of all types that implements IScrollingStrategy
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetStrategyTypes()
        {
            foreach( Type type in Assembly.GetExecutingAssembly().GetTypes().Where( x => typeof( IScrollingStrategy ).IsAssignableFrom( x ) ) )
            {
                yield return type;
            }
        }

        /// <summary>
        /// Get all the available strategy names
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetStrategyNames()
        {
            foreach( Type type in Assembly.GetExecutingAssembly().GetTypes().Where( x => typeof( IScrollingStrategy ).IsAssignableFrom( x ) ) )
            {
                StrategyAttribute strategy = (StrategyAttribute)type.GetCustomAttributes( typeof( StrategyAttribute ), false ).FirstOrDefault();
                if( strategy != null )
                {
                    yield return strategy.Name;
                }
            }
        }

        static StrategyBridge()
        {
            AvailableStrategies = GetStrategyNames().ToList();
        }

        public StrategyBridge( DispatcherTimer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor config )
        {
            Implementations = new Dictionary<string, IScrollingStrategy>();

            foreach(Type t in  GetStrategyTypes())
            {
                //Ignore some types
                if( t == typeof( IScrollingStrategy ) || t == typeof( StrategyBridge ) || t == typeof( ScrollingStrategyBase ) )
                    continue;

                IScrollingStrategy instance = (IScrollingStrategy) Activator.CreateInstance( t );
                instance.Setup( timer, elements, config );
                
                if( Implementations.ContainsKey( instance.Name ) )
                    throw new InvalidOperationException("Cannot register the duplicate strategy name : " + instance.Name );

                Implementations[instance.Name] = instance;
            }

            _current = Implementations.Values.FirstOrDefault();
            if( _current == null )
                throw new InvalidOperationException( "One scrolling strategy at least must be available" );
        }

        /// <summary>
        /// Switch to the specified strategy
        /// </summary>
        /// <param name="strategyName">The name of the strategy to switch to</param>
        public void SwitchTo(string strategyName)
        {
            if( !Implementations.ContainsKey( strategyName ) ) 
                throw new InvalidOperationException("Cannot switch to the unknown strategy : " + strategyName);

            _current.Stop();
            _current = Implementations[strategyName];
            _current.Start();
        }

        #region IScrollingStrategy Members

        public bool IsStarted
        {
            get { return _current.IsStarted; }
        }

        public bool IsPaused
        {
            get { return _current.IsPaused; }
        }

        public string Name
        {
            get { return _current.Name; }
        }

        public void Setup( DispatcherTimer timer, Dictionary<string, IHighlightableElement> elements, IPluginConfigAccessor config )
        {
            _current.Setup( timer, elements, config );
        }

        public void Start()
        {
            _current.Start();
        }

        public void Stop()
        {
            _current.Stop();
        }

        public void GoToElement( HighlightModel.IHighlightableElement element )
        {
            _current.GoToElement( element );
        }

        public void Pause( bool forceEndHighlight )
        {
            _current.Pause( forceEndHighlight );
        }

        public void Resume()
        {
            _current.Resume();
        }

        public void OnExternalEvent()
        {
            _current.OnExternalEvent();
        }

        public void ElementUnregistered( HighlightModel.IHighlightableElement element )
        {
            _current.ElementUnregistered( element );
        }

        #endregion
    }
}