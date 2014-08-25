#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\KeyScroller\StrategyBridge.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using CK.Core;
using CK.Plugin.Config;
using CommonServices.Accessibility;
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

        public StrategyBridge( DispatcherTimer timer, Func<ICKReadOnlyList<IHighlightableElement>> getElements, IPluginConfigAccessor config )
        {
            Implementations = new Dictionary<string, IScrollingStrategy>();

            foreach( Type t in GetStrategyTypes() )
            {
                //Ignore some types
                if( t == typeof( IScrollingStrategy ) || t == typeof( StrategyBridge ) || t == typeof( ScrollingStrategyBase ) )
                    continue;

                IScrollingStrategy instance = (IScrollingStrategy)Activator.CreateInstance( t );
                instance.Setup( timer, getElements, config );

                if( Implementations.ContainsKey( instance.Name ) )
                    throw new InvalidOperationException( "Cannot register the duplicate strategy name : " + instance.Name );

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
        public void SwitchTo( string strategyName )
        {
            if( !Implementations.ContainsKey( strategyName ) )
            {
                strategyName = ZoneScrollingStrategy.StrategyName;
                if( !Implementations.ContainsKey( strategyName ) ) throw new InvalidOperationException( "trying to switch ot an unknown strategy (" + strategyName + "). Fallbacking failed (there are no implementations available)" );

                //A previous version of CiviKey has strategies that don't exist anymore. Loosening the process by implementing a fallback.
                //throw new InvalidOperationException( "Cannot switch to the unknown strategy : " + strategyName );
            }

            _current.Stop();
            _current = Implementations[strategyName];
            _current.Start();
        }

        #region IScrollingStrategy Members

        public event EventHandler<HighlightEventArgs> BeginHighlightElement
        {
            add 
            {
                foreach( var strategy in Implementations.Values )
                {
                    strategy.BeginHighlightElement += value; 
                }
            }
            remove 
            {
                foreach( var strategy in Implementations.Values )
                {
                    strategy.BeginHighlightElement -= value;
                }
            }
        }

        public event EventHandler<HighlightEventArgs> EndHighlightElement
        {
            add 
            {
                foreach( var strategy in Implementations.Values )
                {
                    strategy.EndHighlightElement += value; 
                }
            }
            remove 
            {
                foreach( var strategy in Implementations.Values )
                {
                    strategy.EndHighlightElement -= value;
                }
            }
        }

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

        public void Setup( DispatcherTimer timer, Func<ICKReadOnlyList<IHighlightableElement>> getElements, IPluginConfigAccessor config )
        {
            _current.Setup( timer, getElements, config );
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