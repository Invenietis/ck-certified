using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CommonLogginPlugins
{
    public class CustomObject
    {
        int _age;
        string _name;
        public CustomObject ( string name, int age )
	    {
            _age = age;
            _name = name;
	    }

        public int Age { get{ return _age; }}
        public string Name { get{ return _name; }}
    }

    public class CustomEventArgs : EventArgs
    {
       int _age;
        string _name;
        public CustomEventArgs( string name, int age )
	    {
            _age = age;
            _name = name;
	    }

        public int Age { get{ return _age; }}
        public string Name { get{ return _name; }} 
    }

    public interface IService01 : IDynamicService
    {
        string MethodComplexOK( CustomObject c, List<string> stringList, HashSet<CustomObject> hash );
        string MethodSimpleOK( string name );
        string MethodError( string name );
        void LaunchEvents();

        event EventHandler<CustomEventArgs> EventOK;
        event EventHandler<CustomEventArgs> EventError;
    }
}
