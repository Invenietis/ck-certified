using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServices.Accessibility;
namespace TurboScroll
{
    interface IScrollingStrategy
    {
        event EventHandler<HighlightEventArgs> BeginHighlight;

        event EventHandler<HighlightEventArgs> EndHighlight;

        event EventHandler<HighlightEventArgs> SelectElement;
    }
}
