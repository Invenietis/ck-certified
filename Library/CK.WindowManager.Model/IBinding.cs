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
        /// Gets the left binding of the reference window and the unbind button
        /// </summary>
        ISpatialBindingWithButtonElement Left { get; }

        /// <summary>
        /// Gets the right binding of the reference window and the unbind button
        /// </summary>
        ISpatialBindingWithButtonElement Right { get; }

        /// <summary>
        /// Gets the bottom binding of the reference window and the unbind button
        /// </summary>
        ISpatialBindingWithButtonElement Bottom { get; }

        /// <summary>
        /// Gets the top binding of the reference window and the unbind button
        /// </summary>
        ISpatialBindingWithButtonElement Top { get; }
    }

    public interface ISpatialBindingWithButtonElement
    {
        /// <summary>
        /// Gets the ISpatialBinding
        /// </summary>
        ISpatialBinding SpatialBinding { get; }

        /// <summary>
        /// Gets the associated UnbindButton
        /// </summary>
        IWindowElement UnbindButton { get; }
    }
}
