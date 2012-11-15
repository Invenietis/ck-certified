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
        string Text { get; set; }

        int CaretIndex { get; set; }

        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been sent by the service.
        /// </summary>
        event EventHandler<PredictionAreaContentEventArgs> TextSent;

        /// <summary>
        /// Raises the <see cref="TextualContextSent"/> event
        /// </summary>
        void SendText();
    }
}
