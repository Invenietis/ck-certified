using System;
using System.Windows.Input;

namespace CK.Plugins.ObjectExplorer
{
    public interface ISelectableElement
    {
        /// <summary>
        /// Gets or sets if this element is currently selected or not.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// This method will be called when the selected state changed.
        /// In most cases it will fire an <see cref="INotifyPropertyChanged"/> event.
        /// </summary>
        void SelectedChanged();
    }
}
