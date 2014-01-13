using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeyScroller
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public class StrategyAttribute : Attribute
    {
        static List<string> _strategies = new List<string>();
        public static List<string> AvalaibleStrategies { get { return _strategies; } }
        public string Name { get; private set; }
        public StrategyAttribute( string name )
        {
            Name = name;
        }

        public static IEnumerable<string> GetStrategies()
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
