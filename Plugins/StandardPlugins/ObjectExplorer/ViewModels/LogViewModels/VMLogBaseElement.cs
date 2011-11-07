using CK.WPF.ViewModel;
using System;
using System.ComponentModel;

namespace CK.StandardPlugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMLogBaseElement : VMBase
    {        
        public bool IsBound { get; private set; }
        internal bool _doLog;
        public virtual bool DoLog 
        { 
            get { return _doLog; }
            set { _doLog = value; OnPropertyChanged("DoLog"); OnLogConfigChanged("DoLog");} 
        }
        public event EventHandler<PropertyChangedEventArgs> LogConfigChanged;

        public string Name { get; private set; }

        public VMLogBaseElement(string name, bool isBound)
        {
            Name = name;
            IsBound = isBound;
        }

        protected void OnLogConfigChanged(string name)
        {
            if (LogConfigChanged != null)
            {
                LogConfigChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}