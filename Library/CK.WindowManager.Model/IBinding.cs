using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.WindowManager.Model
{
    public enum BindingPosition
    {
        Top,
        Right,
        Bottom,
        Left
    }
    /// <summary>
    /// Represents a binding between two <see cref="IWindowElement"/>.
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// Represents the position of the Master window relative to Slave the  window.
        /// <remarks>
        /// Master
        /// Slave
        /// > Position: Top
        /// 
        /// Master - Slave > Position: Left
        /// 
        /// Slave
        /// Master
        /// > Position: Bottom.
        /// 
        /// Slave - Master > Position: Right
        /// </remarks>
        /// </summary>
        BindingPosition Position { get; }

        IWindowElement Master { get; }

        IWindowElement Slave { get; }
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
