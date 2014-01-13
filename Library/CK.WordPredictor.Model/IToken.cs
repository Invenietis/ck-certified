namespace CK.WordPredictor.Model
{
    /// <summary>
    /// A token describes a group of symbols.
    /// </summary>
    public interface IToken
    {
        string Value { get; }
    }
}
