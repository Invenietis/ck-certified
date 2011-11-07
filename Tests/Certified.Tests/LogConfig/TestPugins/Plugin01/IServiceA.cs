using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    public interface IServiceA : IDynamicService
    {
        void Method1_1(string name, int age);
        void Method1_2(string name, int age);
        string Property1_1 { get; set; }
        int Property1_2 { get; set; }
        event EventHandler Event1_1;
        event EventHandler Event1_2;
    }
}
