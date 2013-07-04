using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KeyScroller
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Strategy : Attribute
    {
        static List<string> _strategies = new List<string>();
        public static List<string> AvalaibleStrategies { get { return _strategies; } }
        public string Name { get; private set; }
        public Strategy(string name)
        {
            Name = name;
        }

        public static IEnumerable<Strategy> GetStrategies( )
        {
            foreach( Type type in Assembly.GetCallingAssembly().GetTypes() )
            {
                Strategy strategy = (Strategy)Strategy.GetCustomAttribute( type, typeof( Strategy ) );
                if( strategy != null )
                {
                    yield return strategy;
                }
            }
        }
        public static IEnumerable<Type> GetTypes()
        {
            foreach( Type type in Assembly.GetCallingAssembly().GetTypes() )
            {
                if( type.GetCustomAttributes( typeof( Strategy ), true ).Length > 0 )
                {
                    yield return type;
                }
            }
        }
    }
}
