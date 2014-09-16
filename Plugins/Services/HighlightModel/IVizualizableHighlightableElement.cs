namespace HighlightModel
{
    public interface IVisualizableHighlightableElement : IHighlightableElement
    {
        string ElementName { get; }

        string VectorImagePath { get; }
    }
}