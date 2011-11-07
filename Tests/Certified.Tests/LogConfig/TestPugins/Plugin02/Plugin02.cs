using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    [Plugin("{66293FA4-6EB7-432b-8525-79275B1B9AE5}",
        Categories = new string[] { "Advanced" },
        PublicName = "Plugin02",
        Version = "1.0.0")]
    public class Plugin02 : IPlugin, IServiceB
    {
        #region IPlugin Implementation

        public bool CanStart(out string lastError)
        {
            lastError = "";
            return true;
        }
        public bool Setup(IPluginSetupInfo info)
        {
            return true;
        }
        public void Start()
        {
            Console.Out.WriteLine("Start Plugin02");
        }
        public void Stop()
        {
            Console.Out.WriteLine("Stop Plugin01");
        }
        public void Teardown()
        {

        }

        #endregion

        public void Method2_1(string name, int age)
        {

        }
        public void Method2_2(string name, int age)
        {

        }

        public string Property2_1 { get; set; }
        public int Property2_2 { get; set; }

        public event EventHandler Event2_1;
        public event EventHandler Event2_2;
    }
}
