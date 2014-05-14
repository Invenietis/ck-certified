using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Windows.Config;
using System.Linq;
using CK.WPF.ViewModel;
using System.Diagnostics;
using System.ComponentModel;

namespace Host.VM
{
    /// <summary>
    /// Handles an ICommand and an array of INotifySelectionChanged objects.
    /// This class keeps track of the selected object among its array.
    /// When the selected changes, the button is enabled. otherwise it is disabled.
    /// Note : if no objects are linked, works as a simple button triggering the action set as parameter of the constructor.
    /// </summary>
    public class IsDirtyConfigItemApply : ConfigItemAction
    {
        Func<bool> _getIsDirty;

        public IsDirtyConfigItemApply( ConfigManager configManager, ICommand cmd, Func<bool> getIsDirty )
            : base( configManager, cmd )
        {
            _getIsDirty = getIsDirty;
        }

        public bool IsEnabled
        {
            get
            {
                return _getIsDirty();
            }
        }

        public void UpdateIsEnabled()
        {
            NotifyOfPropertyChange( "IsEnabled" );
        }
    }
}
