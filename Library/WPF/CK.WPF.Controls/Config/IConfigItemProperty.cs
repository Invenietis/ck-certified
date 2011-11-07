using System;
using System.ComponentModel;

namespace CK.WPF.Controls
{
    public interface IConfigItemProperty<T> : IConfigItem, INotifyPropertyChanged
    {
        T Value { get; set; }
    }
}
