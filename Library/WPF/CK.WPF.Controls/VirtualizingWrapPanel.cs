using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CK.WPF.Controls
{
    #region VirtualizingWrapPanel
    /// <summary>
    /// 子要素を仮想化する <see cref="System.Windows.Controls.WrapPanel"/>。
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region ItemSize

        #region ItemWidth

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register( "ItemWidth", typeof( double ), typeof( VirtualizingWrapPanel ), new FrameworkPropertyMetadata( double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure ), new ValidateValueCallback( VirtualizingWrapPanel.IsWidthHeightValid ) );
        /// <summary>
        /// To get or set a value that specifies the width of all items that are included in the VirtualizingWrapPanel.
        /// </summary>
        [TypeConverter( typeof( LengthConverter ) ), Category( "Common" )]
        public double ItemWidth
        {
            get { return (double)GetValue( ItemWidthProperty ); }
            set { SetValue( ItemWidthProperty, value ); }
        }

        #endregion

        #region ItemHeight

        /// <summary>
        /// Identifier of the <see cref="ItemHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register( "ItemHeight", typeof( double ), typeof( VirtualizingWrapPanel ), new FrameworkPropertyMetadata( double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure ), new ValidateValueCallback( VirtualizingWrapPanel.IsWidthHeightValid ) );

        /// <summary>
        /// To get or set a value that specifies the height of all items that are included in the VirtualizingWrapPanel.
        /// </summary>
        [TypeConverter( typeof( LengthConverter ) ), Category( "Common" )]
        public double ItemHeight
        {
            get { return (double)GetValue( ItemHeightProperty ); }
            set { SetValue( ItemHeightProperty, value ); }
        }

        #endregion

        #region IsWidthHeightValid
        /// <summary>
        /// Callback to validate the value set in the <see cref="ItemWidth"/>, <see cref="ItemHeight"/> is valid.
        /// </summary>
        /// <param name="value">Value set for the property.</param>
        /// <returns>False true, if it is disabled if the value is valid.</returns>
        private static bool IsWidthHeightValid( object value )
        {
            var d = (double)value;
            return double.IsNaN( d ) || ((d >= 0) && !double.IsPositiveInfinity( d ));
        }
        #endregion

        #endregion

        #region Orientation

        /// <summary>
        /// Identifier of the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            WrapPanel.OrientationProperty.AddOwner(
                typeof( VirtualizingWrapPanel ),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback( VirtualizingWrapPanel.OnOrientationChanged )
                )
            );

        /// <summary>
        /// To get or set a value that specifies the orientation in which child content is placed.
        /// </summary>
        [Category( "Common" )]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue( OrientationProperty ); }
            set { SetValue( OrientationProperty, value ); }
        }

        /// <summary>
        /// Callback that is called when the <see cref="Orientation"/> dependency property changes.
        /// </summary>
        /// <param name="d"> <see cref="System.Windows.DependencyObject"/> value of the property has changed.</param>
        /// <param name="e">Event data that is issued by the event that tracks changes to the effective value of this property.</param>
        private static void OnOrientationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var panel = d as VirtualizingWrapPanel;
            panel._offset = default( Point );
            panel.InvalidateMeasure();
        }

        #endregion

        #region MeasureOverride, ArrangeOverride

        /// <summary>
        /// Dictionary that stores the location and size of items in the specified index.
        /// </summary>
        private Dictionary<int, Rect> _containerLayouts = new Dictionary<int, Rect>();

        /// <summary>
        /// Measuring the size of the layout required for child elements and determines the size of the panel.
        /// </summary>
        /// <param name="availableSize">Available sizes which can be given to a child element.</param>
        /// <returns>Size that this panel needs during layout.</returns>
        protected override Size MeasureOverride( Size availableSize )
        {
            _containerLayouts.Clear();

            var isAutoWidth = double.IsNaN( ItemWidth );
            var isAutoHeight = double.IsNaN( ItemHeight );
            var childAvailable = new Size( isAutoWidth ? double.PositiveInfinity : ItemWidth, isAutoHeight ? double.PositiveInfinity : ItemHeight );
            var isHorizontal = Orientation == Orientation.Horizontal;

            var childrenCount = InternalChildren.Count;

            var itemsControl = ItemsControl.GetItemsOwner( this );
            if( itemsControl != null )
                childrenCount = itemsControl.Items.Count;

            var generator = new ChildGenerator( this );

            var x = 0.0;
            var y = 0.0;
            var lineSize = default( Size );
            var maxSize = default( Size );

            for( int i = 0; i < childrenCount; i++ )
            {
                var childSize = ContainerSizeForIndex( i );

                // Adjust x, y at the intersection size provisional determination of the viewport
                var isWrapped = isHorizontal ?
                    lineSize.Width + childSize.Width > availableSize.Width :
                    lineSize.Height + childSize.Height > availableSize.Height;
                if( isWrapped )
                {
                    x = isHorizontal ? 0 : x + lineSize.Width;
                    y = isHorizontal ? y + lineSize.Height : 0;
                }

                // Child elements re-measure the size to generate a child element, if the viewport
                var itemRect = new Rect( x, y, childSize.Width, childSize.Height );
                var viewportRect = new Rect( _offset, availableSize );
                if( itemRect.IntersectsWith( viewportRect ) )
                {
                    var child = generator.GetOrCreateChild( i );
                    child.Measure( childAvailable );
                    childSize = ContainerSizeForIndex( i );
                }

                // The memory sizes were determined
                _containerLayouts[i] = new Rect( x, y, childSize.Width, childSize.Height );

                // Calculate lineSize, the maxSize
                isWrapped = isHorizontal ?
                    lineSize.Width + childSize.Width > availableSize.Width :
                    lineSize.Height + childSize.Height > availableSize.Height;
                if( isWrapped )
                {
                    maxSize.Width = isHorizontal ? Math.Max( lineSize.Width, maxSize.Width ) : maxSize.Width + lineSize.Width;
                    maxSize.Height = isHorizontal ? maxSize.Height + lineSize.Height : Math.Max( lineSize.Height, maxSize.Height );
                    lineSize = childSize;

                    isWrapped = isHorizontal ?
                        childSize.Width > availableSize.Width :
                        childSize.Height > availableSize.Height;
                    if( isWrapped )
                    {
                        maxSize.Width = isHorizontal ? Math.Max( childSize.Width, maxSize.Width ) : maxSize.Width + childSize.Width;
                        maxSize.Height = isHorizontal ? maxSize.Height + childSize.Height : Math.Max( childSize.Height, maxSize.Height );
                        lineSize = default( Size );
                    }
                }
                else
                {
                    lineSize.Width = isHorizontal ? lineSize.Width + childSize.Width : Math.Max( childSize.Width, lineSize.Width );
                    lineSize.Height = isHorizontal ? Math.Max( childSize.Height, lineSize.Height ) : lineSize.Height + childSize.Height;
                }

                x = isHorizontal ? lineSize.Width : maxSize.Width;
                y = isHorizontal ? maxSize.Height : lineSize.Height;
            }

            maxSize.Width = isHorizontal ? Math.Max( lineSize.Width, maxSize.Width ) : maxSize.Width + lineSize.Width;
            maxSize.Height = isHorizontal ? maxSize.Height + lineSize.Height : Math.Max( lineSize.Height, maxSize.Height );

            _extent = maxSize;
            _viewport = availableSize;

            generator.CleanupChildren();
            generator.Dispose();

            if( ScrollOwner != null )
                ScrollOwner.InvalidateScrollInfo();

            return maxSize;
        }

        #region ChildGenerator
        /// <summary>
        /// I manage the items of <see cref="VirtualizingWrapPanel"/>.
        /// </summary>
        private class ChildGenerator : IDisposable
        {
            #region fields

            /// <summary>
            /// <see cref="VirtualizingWrapPanel"/> for which to generate the items.
            /// </summary>
            private VirtualizingWrapPanel _owner;

            /// <summary>
            /// <see cref="System.Windows.Controls.ItemContainerGenerator"/> of <see cref="_owner"/>.
            /// </summary>
            private IItemContainerGenerator _generator;

            /// <summary>
            /// object to track the lifetime of the generation process of the <see cref="_generator"/>.
            /// </summary>
            private IDisposable _generatorTracker;

            /// <summary>
            /// Index of the first element in the display range.
            /// </summary>
            private int _firstGeneratedIndex;

            /// <summary>
            /// Index of the last element in the display range.
            /// </summary>
            private int _lastGeneratedIndex;

            /// <summary>
            /// Index of <see cref="System.Windows.Controls.Panel.InternalChildren"/> within the element to be generated next.
            /// </summary>
            private int _currentGenerateIndex;

            #endregion

            #region _ctor

            /// <summary>
            /// I want to create a new instance of <see cref="ChildGenerator"/>
            /// </summary>
            /// <see cref="VirtualizingWrapPanel"/> for which to generate the <param name="owner"> item.</param>
            public ChildGenerator( VirtualizingWrapPanel owner )
            {
                _owner = owner;

                // Become null If you do not access to InternalChildren ItemContainerGenerator to pre-acquisition
                var childrenCount = owner.InternalChildren.Count;
                _generator = owner.ItemContainerGenerator;
            }

            /// <summary>
            /// Discard the instance of <see cref="ChildGenerator"/>.
            /// </summary>
            ~ChildGenerator()
            {
                Dispose();
            }

            /// <summary>
            /// I will end the production of items.
            /// </summary>
            public void Dispose()
            {
                if( _generatorTracker != null )
                    _generatorTracker.Dispose();
            }

            #endregion

            #region GetOrCreateChild

            /// <summary>
            /// I will start the production of items.
            /// </summary>
            /// <param name="index">The index of the item.</param>
            private void BeginGenerate( int index )
            {
                _firstGeneratedIndex = index;
                var startPos = _generator.GeneratorPositionFromIndex( index );
                _currentGenerateIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
                _generatorTracker = _generator.StartAt( startPos, GeneratorDirection.Forward, true );
            }

            /// <summary>
            /// The generated items when necessary, to obtain the item at the specified index.
            /// </summary>
            /// <param name="index">The index of the item to be retrieved.</param>
            /// <returns>Item at the specified index.</returns>
            public UIElement GetOrCreateChild( int index )
            {
                if( _generator == null )
                    return _owner.InternalChildren[index];

                if( _generatorTracker == null )
                    BeginGenerate( index );

                bool newlyRealized;
                var child = _generator.GenerateNext( out newlyRealized ) as UIElement;
                if( newlyRealized )
                {
                    if( _currentGenerateIndex >= _owner.InternalChildren.Count )
                        _owner.AddInternalChild( child );
                    else
                        _owner.InsertInternalChild( _currentGenerateIndex, child );

                    _generator.PrepareItemContainer( child );
                }

                _lastGeneratedIndex = index;
                _currentGenerateIndex++;

                return child;
            }

            #endregion

            #region CleanupChildren
            /// <summary>
            /// I want to delete an item outside the display range.
            /// </summary>
            public void CleanupChildren()
            {
                if( _generator == null )
                    return;

                var children = _owner.InternalChildren;

                for( int i = children.Count - 1; i >= 0; i-- )
                {
                    var childPos = new GeneratorPosition( i, 0 );
                    var index = _generator.IndexFromGeneratorPosition( childPos );
                    if( index < _firstGeneratedIndex || index > _lastGeneratedIndex )
                    {
                        _generator.Remove( childPos, 1 );
                        _owner.RemoveInternalChildRange( i, 1 );
                    }
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Place the child element to determine the size of the panel.
        /// </summary>
        /// <param name="finalSize">Area at the end of the parent to be used to place the child elements and the panel itself.</param>
        /// <returns>The actual size you want to use.</returns>
        protected override Size ArrangeOverride( Size finalSize )
        {
            foreach( UIElement child in InternalChildren )
            {
                var gen = ItemContainerGenerator as ItemContainerGenerator;
                var index = (gen != null) ? gen.IndexFromContainer( child ) : InternalChildren.IndexOf( child );
                if( _containerLayouts.ContainsKey( index ) )
                {
                    var layout = _containerLayouts[index];
                    layout.Offset( _offset.X * -1, _offset.Y * -1 );
                    child.Arrange( layout );
                }
            }

            return finalSize;
        }

        #endregion

        #region ContainerSizeForIndex

        /// <summary>
        /// Size of the elements that are laid out just before.
        /// </summary>
        /// <remarks>
        /// <see cref="System.Windows.DataTemplate"/> when used, on the assumption that the size of all the elements are the same, I use to estimate the size of the element.
        /// </remarks>
        private Size _prevSize = new Size( 16, 16 );

        /// <summary>
        /// To estimate without generating an item actually, the size of the item to the specified index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <returns>The estimated size of the item to the specified index.</returns>
        private Size ContainerSizeForIndex( int index )
        {
            var getSize = new Func<int, Size>( idx =>
            {
                UIElement item = null;
                var itemsOwner = ItemsControl.GetItemsOwner( this );
                var generator = ItemContainerGenerator as ItemContainerGenerator;

                if( itemsOwner == null || generator == null )
                {
                    // If it is used in VirtualizingWrapPanel alone to return the item itself
                    if( InternalChildren.Count > idx )
                        item = InternalChildren[idx];
                }
                else
                {
                    // if not generation, generator uses the Items is there could utilize items
                    if( generator.ContainerFromIndex( idx ) != null )
                        item = generator.ContainerFromIndex( idx ) as UIElement;
                    else if( itemsOwner.Items.Count > idx )
                        item = itemsOwner.Items[idx] as UIElement;
                }

                if( item != null )
                {
                    // I return the size of size If the item is already measured
                    if( item.IsMeasureValid )
                        return item.DesiredSize;

                    // If you have not measured the size of the item use the recommended values
                    var i = item as FrameworkElement;
                    if( i != null )
                        return new Size( i.Width, i.Height );
                }

                // I spend there if there is a measure of last
                if( _containerLayouts.ContainsKey( idx ) )
                    return _containerLayouts[idx].Size;

                // If a valid size can not be obtained, I will return the size of the previous item
                return _prevSize;
            } );

            var size = getSize( index );

            // Adjust ItemWidth, ItemHeight if specified
            if( !double.IsNaN( ItemWidth ) )
                size.Width = ItemWidth;
            if( !double.IsNaN( ItemHeight ) )
                size.Height = ItemHeight;

            return _prevSize = size;
        }

        #endregion

        #region OnItemsChanged
        /// <summary>
        /// Callback that is called when the <see cref="System.Windows.Controls.ItemsControl.Items"/> collection that is associated with the <see cref="System.Windows.Controls.ItemsControl"/> of this panel has been changed.
        /// </summary>
        /// <param name="sender"><see cref="System.Object"/> that raised the event</param>
        /// <param name="args">Event data.</param>
        /// <remarks>
        /// When <see cref="System.Windows.Controls.ItemsControl.Items"/> are changed
        /// I also reflect on <see cref="System.Windows.Controls.Panel.InternalChildren"/>.
        /// </remarks>
        protected override void OnItemsChanged( object sender, ItemsChangedEventArgs args )
        {
            switch( args.Action )
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange( args.Position.Index, args.ItemUICount );
                    break;
            }
        }
        #endregion

        #region IScrollInfo Members

        #region Extent

        /// <summary>
        /// The size of the extent.
        /// </summary>
        private Size _extent = default( Size );

        /// <summary>
        /// I get the vertical size of the extent.
        /// </summary>
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        /// <summary>
        /// I get the width of the extent.
        /// </summary>
        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        #endregion Extent

        #region Viewport

        /// <summary>
        /// The size of the view port.
        /// </summary>
        private Size _viewport = default( Size );

        /// <summary>
        /// I get the vertical size of the viewport for this content.
        /// </summary>
        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        /// <summary>
        /// I get the width of the viewport for this content.
        /// </summary>
        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        #endregion

        #region Offset

        /// <summary>
        /// Content offset that was scrolled.
        /// </summary>
        private Point _offset;

        /// <summary>
        /// I get the horizontal offset of the content scrolls.
        /// </summary>
        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        /// <summary>
        /// I get the vertical offset of the content scrolls.
        /// </summary>
        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        #endregion

        #region ScrollOwner
        /// <summary>
        /// To get or set, the <see cref="System.Windows.Controls.ScrollViewer"/> element that controls scrolling behavior.
        /// </summary>
        public ScrollViewer ScrollOwner { get; set; }
        #endregion

        #region CanHorizontallyScroll
        /// <summary>
        /// To get or set a value indicating whether it is possible to scroll the horizontal axis.
        /// </summary>
        public bool CanHorizontallyScroll { get; set; }
        #endregion

        #region CanVerticallyScroll
        /// <summary>
        /// To get or set a value indicating whether it is possible to scroll the vertical axis.
        /// </summary>
        public bool CanVerticallyScroll { get; set; }
        #endregion

        #region LineUp
        /// <summary>
        /// I scrolling up within content by one logical unit.
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset( VerticalOffset - SystemParameters.ScrollHeight );
        }
        #endregion

        #region LineDown
        /// <summary>
        /// I scroll down within content by one logical unit.
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset( VerticalOffset + SystemParameters.ScrollHeight );
        }
        #endregion

        #region LineLeft
        /// <summary>
        /// I scroll to the left within content by one logical unit.
        /// </summary>
        public void LineLeft()
        {
            SetHorizontalOffset( HorizontalOffset - SystemParameters.ScrollWidth );
        }
        #endregion

        #region LineRight
        /// <summary>
        /// I scroll to the right within content by one logical unit.
        /// </summary>
        public void LineRight()
        {
            SetHorizontalOffset( HorizontalOffset + SystemParameters.ScrollWidth );
        }
        #endregion

        #region PageUp
        /// <summary>
        /// I want to scroll up one page within the content.
        /// </summary>
        public void PageUp()
        {
            SetVerticalOffset( VerticalOffset - _viewport.Height );
        }
        #endregion

        #region PageDown
        /// <summary>
        /// I scroll down one page within the content.
        /// </summary>
        public void PageDown()
        {
            SetVerticalOffset( VerticalOffset + _viewport.Height );
        }
        #endregion

        #region PageLeft
        /// <summary>
        /// I scroll to the left by one page within the content.
        /// </summary>
        public void PageLeft()
        {
            SetHorizontalOffset( HorizontalOffset - _viewport.Width );
        }
        #endregion

        #region PageRight
        /// <summary>
        /// I scroll to the right by one page within the content.
        /// </summary>
        public void PageRight()
        {
            SetHorizontalOffset( HorizontalOffset + _viewport.Width );
        }
        #endregion

        #region MouseWheelUp
        /// <summary>
        /// After the user clicks the wheel button on a mouse, I want to scroll up within content.
        /// </summary>
        public void MouseWheelUp()
        {
            SetVerticalOffset( VerticalOffset - SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines );
        }
        #endregion

        #region MouseWheelDown
        /// <summary>
        /// After the user clicks the wheel button on a mouse, I scroll down the content.
        /// </summary>
        public void MouseWheelDown()
        {
            SetVerticalOffset( VerticalOffset + SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines );
        }
        #endregion

        #region MouseWheelLeft
        /// <summary>
        /// After the user clicks the wheel button on a mouse, I want to scroll to the left within content
        /// </summary>
        public void MouseWheelLeft()
        {
            SetHorizontalOffset( HorizontalOffset - SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines );
        }
        #endregion

        #region MouseWheelRight
        /// <summary>
        /// After the user clicks the wheel button on a mouse, I want to scroll to the right content.
        /// </summary>
        public void MouseWheelRight()
        {
            SetHorizontalOffset( HorizontalOffset + SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines );
        }
        #endregion

        #region MakeVisible
        /// <summary>
        /// Coordinate space of the <see cref="System.Windows.Media.Visual"/> object to appear, I will scroll to force the content
        /// </summary>
        /// <param name="visual">The <see cref="System.Windows.Media.Visual"/> to become visible.</param>
        /// <param name="rectangle">Rectangle to identify the bounding coordinate space to display.</param>
        /// <returns><see cref="System.Windows.Rect"/> to be displayed.</returns>
        public Rect MakeVisible( Visual visual, Rect rectangle )
        {
            var idx = InternalChildren.IndexOf( visual as UIElement );

            var generator = ItemContainerGenerator as IItemContainerGenerator;
            if( generator != null )
            {
                var pos = new GeneratorPosition( idx, 0 );
                idx = generator.IndexFromGeneratorPosition( pos );
            }

            if( idx < 0 )
                return Rect.Empty;

            if( !_containerLayouts.ContainsKey( idx ) )
                return Rect.Empty;

            var layout = _containerLayouts[idx];

            if( HorizontalOffset + ViewportWidth < layout.X + layout.Width )
                SetHorizontalOffset( layout.X + layout.Width - ViewportWidth );
            if( layout.X < HorizontalOffset )
                SetHorizontalOffset( layout.X );

            if( VerticalOffset + ViewportHeight < layout.Y + layout.Height )
                SetVerticalOffset( layout.Y + layout.Height - ViewportHeight );
            if( layout.Y < VerticalOffset )
                SetVerticalOffset( layout.Y );

            layout.Width = Math.Min( ViewportWidth, layout.Width );
            layout.Height = Math.Min( ViewportHeight, layout.Height );

            return layout;
        }
        #endregion

        #region SetHorizontalOffset
        /// <summary>
        /// I want to set the value of the horizontal offset.
        /// </summary>
        /// <param name="offset">The extent of the horizontal offset of the content from including viewport.</param>
        public void SetHorizontalOffset( double offset )
        {
            if( offset < 0 || ViewportWidth >= ExtentWidth )
            {
                offset = 0;
            }
            else
            {
                if( offset + ViewportWidth >= ExtentWidth )
                    offset = ExtentWidth - ViewportWidth;
            }

            _offset.X = offset;

            if( ScrollOwner != null )
                ScrollOwner.InvalidateScrollInfo();

            InvalidateMeasure();
        }
        #endregion

        #region SetVerticalOffset
        /// <summary>
        /// I want to set the value of the vertical offset.
        /// </summary>
        /// <param name="offset">The degree of vertical offset from the viewport include.</param>
        public void SetVerticalOffset( double offset )
        {
            if( offset < 0 || ViewportHeight >= ExtentHeight )
            {
                offset = 0;
            }
            else
            {
                if( offset + ViewportHeight >= ExtentHeight )
                    offset = ExtentHeight - ViewportHeight;
            }

            _offset.Y = offset;

            if( ScrollOwner != null )
                ScrollOwner.InvalidateScrollInfo();

            InvalidateMeasure();
        }
        #endregion

        #endregion
    }
    #endregion
}