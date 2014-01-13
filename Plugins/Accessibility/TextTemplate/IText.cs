using HighlightModel;

namespace TextTemplate
{
    public interface IText : IHighlightableElement, IHighlightable
    {
        /// <summary>
        /// True if the property Text is writeable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        /// Return the text. If IsEditable is set to false, all attempts to set Text property should throw an exception.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The text placeholder
        /// </summary>
        string Placeholder { get; set; }
    }

    public interface IHighlightable
    {
        /// <summary>
        /// Wheter the IText is highlight or not
        /// </summary>
        bool IsHighlighted { get; set; }
    }
}
