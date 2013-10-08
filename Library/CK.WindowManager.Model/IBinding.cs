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

    public static class BindingExtensions
    {

        /// <summary>
        /// Gets all descendants
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IWindowElement> AllDescendants( this ISpatialBinding me )
        {
            if( me.Top != null ) foreach( var e in me.Top.Descendants( me.Window ) ) yield return e;
            if( me.Left != null ) foreach( var e in me.Left.Descendants( me.Window ) ) yield return e;
            if( me.Bottom != null ) foreach( var e in me.Bottom.Descendants( me.Window ) ) yield return e;
            if( me.Right != null ) foreach( var e in me.Right.Descendants( me.Window ) ) yield return e;
        }

        private static IEnumerable<IWindowElement> Descendants( this ISpatialBinding me, IWindowElement reference )
        {
            if( me.Top != null && me.Top.Window != reference ) foreach( var e in me.Top.Descendants( me.Window ) ) yield return e;
            if( me.Left != null && me.Left.Window != reference ) foreach( var e in me.Left.Descendants( me.Window ) ) yield return e;
            if( me.Bottom != null && me.Bottom.Window != reference ) foreach( var e in me.Bottom.Descendants( me.Window ) ) yield return e;
            if( me.Right != null && me.Right.Window != reference ) foreach( var e in me.Right.Descendants( me.Window ) ) yield return e;

            yield return me.Window;
        }
    }
}
