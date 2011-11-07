using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    public interface IServiceB : IDynamicService
    {
        void Method2_1(string name, int age);
        void Method2_2(string name, int age);
        string Property2_1 { get; set; }
        int Property2_2 { get; set; }
        event EventHandler Event2_1;
        event EventHandler Event2_2;
    }
}
