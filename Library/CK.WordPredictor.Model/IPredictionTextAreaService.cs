using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IPredictionTextAreaService : IDynamicService, INotifyPropertyChanged
    {
        string Text { get; }
    }
}
