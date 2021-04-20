using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Orcus.Administration.Extensions
{
    /// <summary>
    ///     Implements a virtualized panel for
    ///     presenting items as tiles.
    /// </summary>
    public class VirtualizingTilePanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty RelativeChildHeightProperty = DependencyProperty.Register(
            "RelativeChildHeight", typeof (double), typeof (VirtualizingTilePanel), new PropertyMetadata(1.0d));

        /// <summary>
        ///     Controls the number of the child elements in a row.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty
            = DependencyProperty.RegisterAttached("Columns", typeof (int), typeof (VirtualizingTilePanel),
                new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                  FrameworkPropertyMetadataOptions.AffectsArrange));

        bool _canHScroll;
        bool _canVScroll;
        Size _extent = new Size(0, 0);
        Point _offset;
        ScrollViewer _owner;

        readonly TranslateTransform _trans = new TranslateTransform();
        Size _viewport = new Size(0, 0);

        /// <summary>
        ///     Default Constructor.
        /// </summary>
        public VirtualizingTilePanel()
        {
            // For use in the IScrollInfo implementation
            RenderTransform = _trans;
            Loaded += (sender, args) =>
            {
                var scrollViewer = FindParent<ScrollViewer>(this);
                scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
                InvalidateMeasure();
            };
        }

        public double RelativeChildHeight
        {
            get { return (double) GetValue(RelativeChildHeightProperty); }
            set { SetValue(RelativeChildHeightProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the number of desired columns.
        /// </summary>
        public int Columns
        {
            get { return (int) GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// </summary>
        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        ///     Gets or sets whether the viewer can
        ///     scroll content horizontally.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get { return _canHScroll; }
            set { _canHScroll = value; }
        }

        /// <summary>
        ///     Gets or sets whether the viewer can
        ///     scroll content vertically.
        /// </summary>
        public bool CanVerticallyScroll
        {
            get { return _canVScroll; }
            set { _canVScroll = value; }
        }

        /// <summary>
        ///     Gets the horizontal offset value.
        /// </summary>
        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        /// <summary>
        ///     Gets the vertical offset value.
        /// </summary>
        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        /// <summary>
        ///     Gets the total height, visible
        ///     and invisible.
        /// </summary>
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        /// <summary>
        ///     Gets the total width, visible
        ///     and invisible.
        /// </summary>
        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        /// <summary>
        ///     Gets the height of the viewable area.
        /// </summary>
        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        /// <summary>
        ///     Gets the width of the viewable area.
        /// </summary>
        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        /// <summary>
        ///     Scroll the content up by one line.
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - 10);
        }

        /// <summary>
        ///     Scroll the content down by one line.
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + 10);
        }

        /// <summary>
        ///     Scroll the content up one viewable partition.
        /// </summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _viewport.Height);
        }

        /// <summary>
        ///     Scroll the content down one viewable partition.
        /// </summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _viewport.Height);
        }

        /// <summary>
        ///     Scroll the content up by 10 pixels.
        /// </summary>
        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - 10);
        }

        /// <summary>
        ///     Scroll the content down by 10 pixels.
        /// </summary>
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + 10);
        }

        /// <summary>
        ///     Scroll the content left by 1 line.
        ///     This method is not implemented and
        ///     will throw an exception if called.
        /// </summary>
        public void LineLeft()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Scroll the content right by 1 line.
        ///     This method is not implemented and
        ///     will throw an exception if called.
        /// </summary>
        public void LineRight()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        /// <summary>
        ///     Scroll the content left by 10 pixels.
        ///     This method is not implemented and
        ///     will throw an exception if called.
        /// </summary>
        public void MouseWheelLeft()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Scroll the content right by 10 pixels.
        ///     This method is not implemented and
        ///     will throw an exception if called.
        /// </summary>
        public void MouseWheelRight()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Scroll the content left by 1 viewable.
        ///     partition. This method is not implemented
        ///     and will throw an exception if called.
        /// </summary>
        public void PageLeft()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Scroll the content right by 1 viewable.
        ///     partition. This method is not implemented
        ///     and will throw an exception if called.
        /// </summary>
        public void PageRight()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Set the horizontal offset value of the viewer.
        ///     This method is not implemented and will throw
        ///     and exception if called.
        /// </summary>
        /// <param name="offset">The new horizontal offset value.</param>
        public void SetHorizontalOffset(double offset)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///     Set the vertical offset value of the viewer.
        ///     This method is not implemented and will throw
        ///     and exception if called.
        /// </summary>
        /// <param name="offset">The new vertical offset value.</param>
        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }

            _offset.Y = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.Y = -offset;

            // Force us to realize the correct children
            InvalidateMeasure();
        }

        /// <summary>
        ///     Measure the children
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns>Size desired</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateScrollInfo(availableSize);

            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = InternalChildren;
            IItemContainerGenerator generator = ItemContainerGenerator;

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleItemIndex;
                    itemIndex <= lastVisibleItemIndex;
                    ++itemIndex, ++childIndex)
                {
                    bool newlyRealized;

                    // Get or create the child
                    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= children.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        // The child has already been created, let's be sure it's in the right spot
                        Debug.Assert(child == children[childIndex], "Wrong child was generated");
                    }

                    // Measurements will depend on layout algorithm
                    child?.Measure(GetChildSize(availableSize));
                }
            }

            // Note: this could be deferred to idle time for efficiency
            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);

            return _extent;
        }

        /// <summary>
        ///     Arrange the children
        /// </summary>
        /// <param name="finalSize">Size available</param>
        /// <returns>Size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            IItemContainerGenerator generator = ItemContainerGenerator;

            UpdateScrollInfo(finalSize);

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                // Map the child offset to an item offset
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child, finalSize);
            }

            return finalSize;
        }

        /// <summary>
        ///     Revirtualize items that are no longer visible
        /// </summary>
        /// <param name="minDesiredGenerated">first item index that should be visible</param>
        /// <param name="maxDesiredGenerated">last item index that should be visible</param>
        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            var children = InternalChildren;
            var generator = ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated)
                {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        ///     When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender">System.Object repersenting the source of the event.</param>
        /// <param name="args">The arguments for the event.</param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    //Peform layout refreshment.
                    if (_owner != null)
                    {
                        _owner.ScrollToTop();
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        }

        /*I've isolated the layout specific code to this region. If you want 
        to do something other than tiling, this is where you'll make your changes*/

        /// <summary>
        ///     Calculate the extent of the view based on the available size
        /// </summary>
        /// <param name="availableSize">available size</param>
        /// <param name="itemCount">number of data items</param>
        /// <returns>Returns the extent size of the viewer.</returns>
        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            //Gets the width of each child.
            double childWidth = CalculateChildWidth(availableSize);

            // See how big we are
            return new Size(Columns*childWidth,
                (childWidth*RelativeChildHeight)*Math.Ceiling((double) itemCount/Columns));
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindParent<T>(parentObject);
        }

        public void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            //If tile mode.
            var childHeight = CalculateChildWidth(_extent)*RelativeChildHeight;

            var scrollViewer = FindParent<ScrollViewer>(this);
            if (scrollViewer == null)
            {
                firstVisibleItemIndex = 0;
                lastVisibleItemIndex = -1;
                return;
            }

            firstVisibleItemIndex = (int) Math.Floor(scrollViewer.VerticalOffset/childHeight)*Columns;
            lastVisibleItemIndex =
                (int) Math.Ceiling((scrollViewer.VerticalOffset + scrollViewer.ViewportHeight)/childHeight)*Columns - 1;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;
        }

        private void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs scrollChangedEventArgs)
        {
            InvalidateMeasure();
        }

        /// <summary>
        ///     Get the size of the each child.
        /// </summary>
        /// <returns>The size of each child.</returns>
        Size GetChildSize(Size availableSize)
        {
            var width = CalculateChildWidth(availableSize);
            return new Size(width
                , width*RelativeChildHeight);
        }

        /// <summary>
        ///     Position a child
        /// </summary>
        /// <param name="itemIndex">The data item index of the child</param>
        /// <param name="child">The element to position</param>
        /// <param name="finalSize">The size of the panel</param>
        void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            //Get the width of each child.
            double childWidth = CalculateChildWidth(finalSize);

            int row = itemIndex/Columns;
            int column = itemIndex%Columns;

            child.Arrange(new Rect(column*childWidth, row*(childWidth*RelativeChildHeight),
                childWidth, (childWidth*RelativeChildHeight)));
        }

        /// <summary>
        ///     Calculate the width of each tile by
        ///     dividing the width of available size
        ///     by the number of required columns.
        /// </summary>
        /// <param name="availableSize">The total layout size available.</param>
        /// <returns>The width of each tile.</returns>
        double CalculateChildWidth(Size availableSize)
        {
            return availableSize.Width/Columns;
        }

        /// <summary>
        ///     See Ben Constable's series of posts at http://blogs.msdn.com/bencon/
        /// </summary>
        /// <param name="availableSize"></param>
        void UpdateScrollInfo(Size availableSize)
        {
            //Initialize items control.
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);

            //See how many items there are
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            //Get the total size, visible and invisible.
            Size extent = CalculateExtent(availableSize, itemCount);

            // Update extent
            if (extent != _extent)
            {
                //Store in class scope.
                _extent = extent;

                //Peform layout refreshment.
                if (_owner != null) _owner.InvalidateScrollInfo();
            }

            // Update viewport
            if (availableSize != _viewport)
            {
                //Store in class scope.
                _viewport = availableSize;

                //Perform layout refreshment.
                if (_owner != null) _owner.InvalidateScrollInfo();
            }
        }
    }
}