using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;
using CK.WPF.ViewModel;
using System.Windows.Input;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CK.StandardPlugins.ObjectExplorer
{
    public class VMIFolder : VMBase, ISelectableElement
    {
        public IEnumerable Items { get; private set; }

        public String Name { get; private set; }

        public VMIFolder( IEnumerable items, string name )
        {
            Name = name;
            Items = items;
        }

        #region ISelectableElement Members

        public bool IsSelected
        {
            get { return false; }
            set { }
        }

        public void SelectedChanged()
        {
            OnPropertyChanged( "IsSelected" );
        }

        #endregion
    }
}