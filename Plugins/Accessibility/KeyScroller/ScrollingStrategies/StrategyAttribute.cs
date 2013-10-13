using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KeyScroller
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StrategyAttribute : Attribute
    {
        static List<string> _strategies = new List<string>();
        public static List<string> AvalaibleStrategies { get { return _strategies; } }
        public string Name { get; private set; }
        public StrategyAttribute(string name)
        {
            Name = name;
        }

        public static IEnumerable<string> GetStrategies( )
        {
            foreach( Type type in Assembly.GetCallingAssembly().GetTypes() )
            {
                StrategyAttribute strategy = (StrategyAttribute)StrategyAttribute.GetCustomAttribute( type, typeof( StrategyAttribute ) );
                if( strategy != null )
                {
                    yield return strategy.Name;
                }
            }
        }
        public static IEnumerable<Type> GetTypes()
        {
            foreach( Type type in Assembly.GetCallingAssembly().GetTypes() )
            {
                if( type.GetCustomAttributes( typeof( StrategyAttribute ), true ).Length > 0 )
                {
                    yield return type;
                }
            }
        }
    }
}
