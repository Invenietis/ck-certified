using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scroller
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public class StrategyAttribute : Attribute
    {
        static List<string> _strategies;
        
        public string Name { get; private set; }

        public StrategyAttribute( string name )
        {
            Name = name;

        }
    }
}
