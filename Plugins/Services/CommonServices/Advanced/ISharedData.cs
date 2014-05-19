using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CK.Plugin;

namespace CommonServices
{
    public interface ISharedData : IDynamicService
    {
        event EventHandler<SharedPropertyChangedEventArgs> SharedPropertyChanged;
        event EventHandler<SharedPropertyChangingEventArgs> SharedPropertyChanging;

        double WindowOpacity { get; set; }
        int WindowBorderThickness { get; set; }
        Color WindowBorderBrush { get; set; }
        Color WindowBackgroundColor { get; set; }
    }

    public class SharedPropertyChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; }
        public SharedPropertyChangedEventArgs( string propertyName )
        {
            PropertyName = propertyName;
        }
    }

    public class SharedPropertyChangingEventArgs : CancelEventArgs
    {
        public string PropertyName { get; set; }
        public SharedPropertyChangingEventArgs( string propertyName )
        {
            PropertyName = propertyName;
        }
    }
}
