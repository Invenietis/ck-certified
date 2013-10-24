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
        /// Enumerates through all descendant with the ability to filter them.
        /// </summary>
        /// <param name="me"><see cref="ISpatialBinding"/></param>
        /// <param name="filter">A filter function that take a descendant as a <see cref="IWindowElement"/>, and the <see cref="BindingPosition"/>.</param>
        /// <returns>An enumeration of all descendants</returns>
        public static IEnumerable<ISpatialBinding> AllDescendants( this ISpatialBinding me, BindingPosition excludes = BindingPosition.None )
        {
            //List<IWindowElement> toExclude = new List<IWindowElement>();

            //if( excludes.HasFlag( BindingPosition.Top ) )
            //    foreach( var e in me.Top.Descendants( me.Window ) ) toExclude.Add( e );
            //if( excludes.HasFlag( BindingPosition.Bottom ) )
            //{
            //    foreach( var e in me.Bottom.Descendants( me.Window ) ) toExclude.Add( e );
            //}
            //if( excludes.HasFlag( BindingPosition.Left ) )
            //    foreach( var e in me.Left.Descendants( me.Window ) ) toExclude.Add( e );
            //if( excludes.HasFlag( BindingPosition.Right ) )
            //    foreach( var e in me.Right.Descendants( me.Window ) ) toExclude.Add( e );

            //return me.AllDescendants().Except( toExclude );

            var v = new List<ISpatialBinding>();
            BrowseExclude( me, v, excludes );
            return v.Except( new[] { me } );
        }

        private static void BrowseExclude( this ISpatialBinding me, IList<ISpatialBinding> visited, BindingPosition excludes )
        {
            if( !visited.Contains( me ) )
            {
                visited.Add( me );

                if( me.Top != null && !excludes.HasFlag( BindingPosition.Top ) ) BrowseExclude( me.Top, visited, excludes );
                if( me.Left != null && !excludes.HasFlag( BindingPosition.Left ) ) BrowseExclude( me.Left, visited, excludes );
                if( me.Bottom != null && !excludes.HasFlag( BindingPosition.Bottom ) ) BrowseExclude( me.Bottom, visited, excludes );
                if( me.Right != null && !excludes.HasFlag( BindingPosition.Right ) ) BrowseExclude( me.Right, visited, excludes );
            }
        }

        public static IEnumerable<IWindowElement> SubTree( this ISpatialBinding me, BindingPosition position )
        {
            var visited = new List<IWindowElement>();
            visited.Add( me.Window );

            if( me.Top != null && position.HasFlag( BindingPosition.Top ) ) Browse( me.Top, visited );
            if( me.Left != null && position.HasFlag( BindingPosition.Left ) ) Browse( me.Left, visited );
            if( me.Bottom != null && position.HasFlag( BindingPosition.Bottom ) ) Browse( me.Bottom, visited );
            if( me.Right != null && position.HasFlag( BindingPosition.Right ) ) Browse( me.Right, visited );

            return visited.Except( new[] { me.Window } );
        }

        private static void Browse( this ISpatialBinding me, IList<IWindowElement> visited )
        {
            if( !visited.Contains( me.Window ) )
            {
                visited.Add( me.Window );

                if( me.Top != null ) Browse( me.Top, visited );
                if( me.Left != null ) Browse( me.Left, visited );
                if( me.Bottom != null ) Browse( me.Bottom, visited );
                if( me.Right != null ) Browse( me.Right, visited );
            }
        }

        //private static IEnumerable<IWindowElement> Descendants( this ISpatialBinding me, IWindowElement reference )
        //{
        //    if( me != null )
        //    {
        //        if( me.Top != null && me.Top.Window != reference ) foreach( var e in me.Top.Descendants( me.Window ) ) yield return e;
        //        if( me.Left != null && me.Left.Window != reference ) foreach( var e in me.Left.Descendants( me.Window ) ) yield return e;
        //        if( me.Bottom != null && me.Bottom.Window != reference ) foreach( var e in me.Bottom.Descendants( me.Window ) ) yield return e;
        //        if( me.Right != null && me.Right.Window != reference ) foreach( var e in me.Right.Descendants( me.Window ) ) yield return e;

        //        yield return me.Window;
        //    }
        //}

        /// <summary>
        /// Gets the window area of the <see cref="IBinding.Origin"/> window comparing to the <see cref="IBinding.Target"/> and <see cref="IBinding.BindingPosition"/>.
        /// </summary>
        /// <param name="binding"><see cref="IBinding"/></param>
        /// <returns>A <see cref="Rect"/> or <see cref="Rect.Empty"/> if the <see cref="BindingPosition"/> is <see cref="BindingPosition.None"/></returns>
        public static Rect GetWindowArea( this IBinding binding )
        {
            if( binding.Position == BindingPosition.Top )
            {
                double top = binding.Target.Top - binding.Origin.Height;
                double left = binding.Target.Left;
                double width = binding.Target.Width;
                double height = binding.Origin.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Bottom )
            {
                double top = binding.Target.Top + binding.Target.Height;
                double left = binding.Target.Left;
                double width = binding.Target.Width;
                double height = binding.Origin.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Left )
            {
                double top = binding.Target.Top;
                double left = binding.Target.Left - binding.Origin.Width;
                double width = binding.Origin.Width;
                double height = binding.Target.Height;
                return new Rect( left, top, width, height );
            }
            if( binding.Position == BindingPosition.Right )
            {
                double top = binding.Target.Top;
                double left = binding.Target.Left + binding.Target.Width;
                double width = binding.Origin.Width;
                double height = binding.Target.Height;
                return new Rect( left, top, width, height );
            }
            return Rect.Empty;
        }
    }
}
