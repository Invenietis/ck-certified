using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Windows.Forms;
using System.Windows;

namespace IExplorer
{
    [Plugin ( Id="{EA79D1BF-A6AC-4e70-B2F2-98F306AB7F97}",
    PublicName="IExplorer",
    Categories= new string[] {"Accessibility"},
    DefaultPluginStatus= ConfigPluginStatus.Manual,
    Version= "0.0.0" ) ]
    public class IExplorer : IPlugin
    {
       string _text;
       IEWindow _IEwindow;

       public string Text
        {
                get { return _text ; }
                set { _text = value; }
                }

        public void Start()
        {
            //Text = "Explorer Ver 0";
            //MessageBox.Show(Text);
            _IEwindow = new IEWindow();

            _IEwindow.Show();
        }

        public void Stop()
        {
            _IEwindow.Close();
        }


        #region IPlugin Members

        public bool CanStart(out string lastError)
        {
            lastError = "";
            return true ;
        }

        public bool Setup(ISetupInfo info)
        {
            return true;
        }

        public void Teardown()
        {

        }

        #endregion
    }
}
