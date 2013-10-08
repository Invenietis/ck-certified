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
        IWindowElement Window { get; }

        ISpatialBinding Left { get; }

        ISpatialBinding Right { get; }

        ISpatialBinding Bottom { get; }

        ISpatialBinding Top { get; }
    }

}
