using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.WordPredictor.Model
{
    public interface IPredictionTextAreaService : IDynamicService
    {
        /// <summary>
        /// This event is raised whenever the text area content changed
        /// </summary>
        event EventHandler<PredictionAreaContentEventArgs> PredictionAreaContentChanged;

        event EventHandler<IsDrivenChangedEventArgs> IsDrivenChanged;

        /// <summary>
        /// This event is raised when the <see cref="ITextualContextService"/> has been sent by the service.
        /// </summary>
        event EventHandler<PredictionAreaContentEventArgs> PredictionAreaTextSent;

        /// <summary>
        /// Gets or sets the fact that this service's implementations handle the caretIndex and prediction context
        /// This value should ONLY be set by the plugin that holds the textarea.
        /// Others should NOT set this value.
        /// </summary>
        bool IsDriven { get; set; }
      
        /// <summary>
        /// Raises the <see cref="TextualContextSent"/> event
        /// </summary>
        void SendText();

        /// <summary>
        /// Raises the <see cref="PredictionAreaContentChanged" /> event if any of the properties changes
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caretIndex"></param>
        void ChangePredictionAreaContent( string text, int caretIndex );
    }
}
