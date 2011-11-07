using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Certified.Tests
{
    [Plugin("{0BDC5F21-DC01-4fde-B1D9-609FED773865}", Categories = new string[] { "Advanced" },
        PublicName = "Plugin03",
        Version = "1.0.0")]
    public class Plugin03 : IPlugin, IServiceC
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

        public void Method3_1(string name, int age)
        {

        }
        public void Method3_2(string name, int age)
        {

        }

        public string Property3_1 { get; set; }
        public int Property3_2 { get; set; }

        public event EventHandler Event3_1;
        public event EventHandler Event3_2;
    }
}
