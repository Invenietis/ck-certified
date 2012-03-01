using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.WPF.ViewModel;
using System.Windows;

namespace CK.Plugins.ObjectExplorer.ViewModels.LogViewModels
{
    public class VMOutputLogEntry : VMBase
    {
        public VMOutputLogEntry( VMLogOutputContainer holder, LogEventArgs e, string message, int index )
        {
            _holder = holder;
            _logEventArgs = e;
            _isCreating = e.IsCreating;
            _message = message;
            _index = index;

            ILogInterceptionEntry logEntry = _logEventArgs as ILogInterceptionEntry;
            if( logEntry != null ) _category = logEntry.Member.DeclaringType.Name;
            else _category = "Other";
        }

        public void NotifyVisibilityChanged()
        {
            OnPropertyChanged( "IsVisible" );
            OnPropertyChanged( "LogObject" );
        }

        public Thickness Margin { get { return new Thickness( _logEventArgs.Depth * 2, 2, 0, 2 ); } }
        public string UnderlyingType { get { return _logEventArgs.EntryType.ToString(); } }
        public bool IsVisible { get { return _holder.IsCategoryFiltered( Category ); } }
        public LogEventArgs LogObject { get { return _logEventArgs; } }
        public string Category { get { return _category; } }
        public string Message { get { return _message; } }
        public int Index { get { return _index; } }
        public bool IsCreating { get { return _isCreating; } }

        VMLogOutputContainer _holder;
        LogEventArgs _logEventArgs;
        bool _isCreating;
        string _category;
        string _message;
        int _index;
    }
}
