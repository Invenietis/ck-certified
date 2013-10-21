using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CK.WindowManager.Model
{

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
        
        /// <summary>
        /// Enumerates through all descendant with the ability to filter them.
        /// </summary>
        /// <param name="me"><see cref="ISpatialBinding"/></param>
        /// <param name="filter">A filter function that take a descendant as a <see cref="IWindowElement"/>, and the <see cref="BindingPosition"/>.</param>
        /// <returns>An enumeration of all descendants</returns>
        public static IEnumerable<IWindowElement> AllDescendants( this ISpatialBinding me, BindingPosition excludes )
        {
            List<IWindowElement> toExclude = new List<IWindowElement>();

            if( excludes == BindingPosition.Top )
                foreach( var e in me.Top.Descendants( me.Window ) ) toExclude.Add( e );
            if( excludes == BindingPosition.Bottom )
                foreach( var e in me.Bottom.Descendants( me.Window ) ) toExclude.Add( e );
            if( excludes == BindingPosition.Left )
                foreach( var e in me.Left.Descendants( me.Window ) ) toExclude.Add( e );
            if( excludes == BindingPosition.Right )
                foreach( var e in me.Right.Descendants( me.Window ) ) toExclude.Add( e );

            return me.AllDescendants().Except( toExclude );
        }

        private static IEnumerable<IWindowElement> Descendants( this ISpatialBinding me, IWindowElement reference )
        {
            if( me.Top != null && me.Top.Window != reference ) foreach( var e in me.Top.Descendants( me.Window ) ) yield return e;
            if( me.Left != null && me.Left.Window != reference ) foreach( var e in me.Left.Descendants( me.Window ) ) yield return e;
            if( me.Bottom != null && me.Bottom.Window != reference ) foreach( var e in me.Bottom.Descendants( me.Window ) ) yield return e;
            if( me.Right != null && me.Right.Window != reference ) foreach( var e in me.Right.Descendants( me.Window ) ) yield return e;

            yield return me.Window;
        }

        /// <summary>
        /// Gets the window area of the <see cref="IBinding.Slave"/> window comparing to the <see cref="IBinding.Master"/> and <see cref="IBinding.BindingPosition"/>.
        /// </summary>
        /// <param name="binding"><see cref="IBinding"/></param>
        /// <returns>A <see cref="Rect"/> or <see cref="Rect.Empty"/> if the <see cref="BindingPosition"/> is <see cref="BindingPosition.None"/></returns>
        public static Rect GetWindowArea( this IBinding binding )
        {
            if( binding.Position == BindingPosition.Top )
            {
                double top = binding.Master.Top - binding.Slave.Height;
                double left = binding.Master.Left;
                double width = binding.Master.Width;
                double height = binding.Slave.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Bottom )
            {
                double top = binding.Master.Top + binding.Master.Height;
                double left = binding.Master.Left;
                double width = binding.Master.Width;
                double height = binding.Slave.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Left )
            {
                double top = binding.Master.Top;
                double left = binding.Master.Left - binding.Slave.Width;
                double width = binding.Slave.Width;
                double height = binding.Master.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Right )
            {
                double top = binding.Master.Top;
                double left = binding.Master.Left + binding.Master.Width;
                double width = binding.Slave.Width;
                double height = binding.Master.Height;
                return new Rect( left, top, width, height );
            }
            return Rect.Empty;
        }
    }
}
