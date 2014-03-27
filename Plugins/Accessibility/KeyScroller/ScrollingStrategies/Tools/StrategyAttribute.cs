using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeyScroller
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public class StrategyAttribute : Attribute
    {
        static List<string> _strategies;
        public static List<string> AvailableStrategies { get { return _strategies; } }
        public string Name { get; private set; }

        static StrategyAttribute()
        {
            _strategies = GetStrategyNames().ToList();
        }

        public StrategyAttribute( string name )
        {
            Name = name;

        }

        public static IEnumerable<Type> GetStrategyTypes()
        {
            foreach( Type type in Assembly.GetExecutingAssembly().GetTypes().Where( x => typeof( IScrollingStrategy ).IsAssignableFrom( x ) ) )
            {
                yield return type;   
            }
        }

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
    }
}
