using System;
using System.ComponentModel;

namespace CK.WPF.Controls
{

    public interface IConfigItemCurrent<T>
    {
        ICollectionView Values { get; }
    }

}
