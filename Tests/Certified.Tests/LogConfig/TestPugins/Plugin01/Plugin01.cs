using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    [Plugin("{C40D66FB-5289-49d4-913F-053B8E6AEA2C}", Categories = new string[] { "Advanced" },
        PublicName = "Plugin01",
        Version = "1.0.0")]
    public class Plugin01 : IPlugin, IServiceA
    {
        #region IPlugin Implementation

        public bool Setup(IPluginSetupInfo info)
        {
            return true;
        }
        public void Start()
        {
            Console.Out.WriteLine("Start Plugin01");
        }
        public void Stop()
        {
            Console.Out.WriteLine("Stop Plugin01");
        }
        public void Teardown()
        {

        }

        #endregion

        public void Method1_1(string name, int age)
        {

        }
        public void Method1_2(string name, int age)
        {

        }

        public string Property1_1 { get; set; }
        public int Property1_2 { get; set; }

        public event EventHandler Event1_1;
        public event EventHandler Event1_2;
    }
}
