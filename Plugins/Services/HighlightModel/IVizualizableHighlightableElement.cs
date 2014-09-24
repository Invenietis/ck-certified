using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightModel
{
    public interface IVisualizableHighlightableElement : IHighlightableElement
    {
        string ElementName { get; }

        string VectorImagePath { get; }
    }
}