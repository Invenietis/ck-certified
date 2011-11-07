using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    public interface IServiceC : IDynamicService
    {
        void Method3_1(string name, int age);
        void Method3_2(string name, int age);
        string Property3_1 { get; set; }
        int Property3_2 { get; set; }
        event EventHandler Event3_1;
        event EventHandler Event3_2;
    }
}
