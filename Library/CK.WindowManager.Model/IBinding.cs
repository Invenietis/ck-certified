using System;

namespace CK.WindowManager.Model
{
    [Flags]
    public enum BindingPosition
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8
    }
    /// <summary>
    /// Represents a binding between two <see cref="IWindowElement"/>.
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// Represents the position of the Master window relative to Slave the  window.
        /// <remarks>
        /// Target
        /// Origin
        /// > Position: Bottom
        /// 
        /// Origin - Target > Position: Left
        /// 
        /// Origin
        /// Target
        /// > Position: Top.
        /// 
        /// Target - Origin > Position: Right
        /// </remarks>
        /// </summary>
        BindingPosition Position { get; }

        /// <summary>
        /// The target window element
        /// </summary>
        IWindowElement Target { get; }

        /// <summary>
        /// The origin window element (or current window)
        /// </summary>
        IWindowElement Origin { get; }
    }

    public interface ISpatialBinding
    {
        /// <summary>
        /// Gets the reference window
        /// </summary>
        IWindowElement Window { get; }

        /// <summary>
        /// Gets the left binding of the reference window
        /// </summary>
        ISpatialBinding Left { get; }

        /// <summary>
        /// Gets the right binding of the reference window
        /// </summary>
        ISpatialBinding Right { get; }

        /// <summary>
        /// Gets the bottom binding of the reference window
        /// </summary>
        ISpatialBinding Bottom { get; }

        /// <summary>
        /// Gets the top binding of the reference window
        /// </summary>
        ISpatialBinding Top { get; }
    }

}
