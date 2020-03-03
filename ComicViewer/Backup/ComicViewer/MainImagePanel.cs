using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;

namespace ComicViewer
{
    public class MainImagePanel:VirtualizingPanel, IScrollInfo,INotifyPropertyChanged
    {
        public event EventHandler<CurrentPageChangedEventArgs> CurrentPageChanged;

        public MainImagePanel()
        {
            this.RenderTransform = trans;
            this.ChildSize = new Size(1, 1);
            this.OriginalChildSize = new Size(1, 1);
            this.Cursor = CBCursors.HandNormal;
            //this.IsChildSizeValid = false;
        }

        #region Public Properties
        int currentPage = 0;
        public int CurrentPage
        {
            get { return currentPage; }
            private set
            {
                
                if (value != currentPage)
                {
                    currentPage = value;
                    this.SetPage(currentPage);
                }
            }
        }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.RegisterAttached(
            "PageSize", typeof(Size), typeof(MainImagePanel),
            new FrameworkPropertyMetadata(new Size(1,1)));

        public Size PageSize
        {
            get { return (Size)this.GetValue(PageSizeProperty); }
            set
            {
                
                this.SetValue(PageSizeProperty, value);
            }
        }

        public static readonly DependencyProperty RotatePageProperty = DependencyProperty.RegisterAttached(
            "RotatePage", typeof(RotatePage), typeof(MainImagePanel),
            new FrameworkPropertyMetadata(RotatePage.RotateNormal));

        public RotatePage RotatePage
        {
            get { return (RotatePage)this.GetValue(RotatePageProperty); }
            set
            {
                
                this.SetValue(RotatePageProperty, value);
            }
        }
        public static readonly DependencyProperty IsFileOpenProperty = DependencyProperty.RegisterAttached(
            "IsFileOpen", typeof(bool), typeof(MainImagePanel),
            new FrameworkPropertyMetadata(false));

        public bool IsFileOpen
        {
            get { return (bool)this.GetValue(IsFileOpenProperty); }
            set
            {
                
                this.SetValue(IsFileOpenProperty, value);
            }
        }
        public static readonly DependencyProperty ZoomTextProperty = DependencyProperty.RegisterAttached(
            "ZoomText", typeof(string), typeof(MainImagePanel),
            new FrameworkPropertyMetadata("Fit Width",
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string ZoomText
        {
            get { return (string)this.GetValue(ZoomTextProperty); }
            set
            {

                this.SetValue(ZoomTextProperty, value);
            }
        }
        public static readonly DependencyProperty PanelModeProperty = DependencyProperty.RegisterAttached(
           "PanelMode", typeof(PanelMode), typeof(MainImagePanel),
           new FrameworkPropertyMetadata(PanelMode.SinglePage,
               FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public PanelMode PanelMode
        {
            get { return (PanelMode)this.GetValue(PanelModeProperty); }
            set
            {
                
                this.SetValue(PanelModeProperty, value);
            }
        }

        public static readonly DependencyProperty GoToPageProperty = DependencyProperty.RegisterAttached(
           "GoToPage", typeof(int), typeof(MainImagePanel),
           new FrameworkPropertyMetadata(0, GoToPagePropertyChanged));

        public int GoToPage
        {
            get { return (int)this.GetValue(GoToPageProperty); }
            set
            {
                
                this.SetValue(GoToPageProperty, value);
            }
        }

        public void SetPage(int pageNo)
        {
            
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            //pageNo++;
            int itemCount = itemsControl != null ? itemsControl.Items.Count : 0;
            if (pageNo >= 0 && pageNo < itemCount)
            {
               double offset  = Math.Floor(pageNo * this.ChildSize.Height) / this.ChildrenPerRow;
               this.SetVerticalOffset(offset+1);
            }
        }

        static void GoToPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainImagePanel panel = d as MainImagePanel;
            if (panel != null)
            {
                panel.SetPage(panel.GoToPage);
            }
        }
        #endregion

        #region Overrides
        Point prvLocation = new Point(1, 1);
        Point currLocation = new Point(1, 1);
       
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.Cursor = CBCursors.HandPressed;
            prvLocation = Mouse.GetPosition(null);

            Mouse.Capture(this);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                currLocation = e.GetPosition(null);

                double tempVertOffset = prvLocation.Y - currLocation.Y;
                double tempHoriOffset = prvLocation.X - currLocation.X;
                if (tempHoriOffset > 10 || tempHoriOffset < -10)
                {
                    //tempHoriOffset = (tempHoriOffset / SystemParameters.PrimaryScreenWidth) * this.viewPortSize.Width;
                    SetHorizontalOffset(this.HorizontalOffset + tempHoriOffset);
                    prvLocation.X = currLocation.X;
                }
                if (tempVertOffset > 10)
                {
                    this.MoveDown(this.VerticalOffset + tempVertOffset);
                    prvLocation.Y = currLocation.Y;
                }
                if( tempVertOffset < -10)
                {
                    //tempVertOffset = (tempVertOffset / SystemParameters.PrimaryScreenHeight) * this.viewPortSize.Height;
                    this.MoveUp(this.VerticalOffset + tempVertOffset);
                    prvLocation.Y = currLocation.Y;
                }
            }
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            Mouse.Capture(null);
            this.Cursor = CBCursors.HandNormal;
        }
        #endregion

        public void Reset()
        {
            //this.ChildSize = new Size(1, 1);
            //this.extentSize = new Size(1, 1);
            //this.offset = new Vector();
            //this.viewPortSize = new Size(1, 1);
            //this.IsChildSizeValid = false;
        }

        #region Override Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;
            Size measureChildSize;
            Size tempPageSize = this.PageSize;
            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex=0, lastVisibleItemIndex=0;

            #region Image Size (Zoom Calculation)
            switch (ZoomToSizingType.GetItemSizeType(this.ZoomText))
            {
                case ItemSizingType.Fit:
                    if (this.PanelMode == PanelMode.SinglePage || this.PanelMode == PanelMode.ContniousPage)
                    {
                        measureChildSize = availableSize;
                    }
                    else
                    {
                        measureChildSize = new Size(availableSize.Width / 2, availableSize.Height);
                    }
                    break;
                case ItemSizingType.FitWidth:
                    if (this.PanelMode == PanelMode.SinglePage || this.PanelMode == PanelMode.ContniousPage)
                    {
                        measureChildSize = new Size(availableSize.Width, Double.PositiveInfinity);
                    }
                    else
                    {
                        measureChildSize = new Size(availableSize.Width / 2, Double.PositiveInfinity);
                    }
                    break;
                case ItemSizingType.Original:
                    measureChildSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
                    break;
                case ItemSizingType.Z8DOT33:
                    measureChildSize = new Size(tempPageSize.Width * 0.0833, tempPageSize.Height * 0.0833);
                    break;
                case ItemSizingType.Z12DOT5:
                    measureChildSize = new Size(tempPageSize.Width * 0.125, tempPageSize.Height * 0.125);
                    break;
                case ItemSizingType.Z25:
                    measureChildSize = new Size(tempPageSize.Width * 0.25, tempPageSize.Height * 0.25);
                    break;
                case ItemSizingType.Z33DOT33:
                    measureChildSize = new Size(tempPageSize.Width * 0.3333, tempPageSize.Height * 0.3333);
                    break;
                case ItemSizingType.Z50:
                    measureChildSize = new Size(tempPageSize.Width * 0.5, tempPageSize.Height * 0.5);
                    break;
                case ItemSizingType.Z66DOT67:
                    measureChildSize = new Size(tempPageSize.Width * 0.6667, tempPageSize.Height * 0.6667);
                    break;
                case ItemSizingType.Z75:
                    measureChildSize = new Size(tempPageSize.Width * 0.75, tempPageSize.Height * 0.75);
                    break;
                case ItemSizingType.Z100:
                    measureChildSize = new Size(tempPageSize.Width * 1, tempPageSize.Height * 1);
                    break;
                case ItemSizingType.Z125:
                    measureChildSize = new Size(tempPageSize.Width * 1.25, tempPageSize.Height * 1.25);
                    break;
                case ItemSizingType.Z150:
                    measureChildSize = new Size(tempPageSize.Width * 1.5, tempPageSize.Height * 1.5);
                    break;
                case ItemSizingType.Z200:
                    measureChildSize = new Size(tempPageSize.Width * 2, tempPageSize.Height * 2);
                    break;
                case ItemSizingType.Z300:
                    measureChildSize = new Size(tempPageSize.Width * 3, tempPageSize.Height * 3);
                    break;
                case ItemSizingType.Z400:
                    measureChildSize = new Size(tempPageSize.Width * 4, tempPageSize.Height * 4);
                    break;
                case ItemSizingType.Z600:
                    measureChildSize = new Size(tempPageSize.Width * 6, tempPageSize.Height * 6);
                    break;
                case ItemSizingType.Z800:
                    measureChildSize = new Size(tempPageSize.Width * 8, tempPageSize.Height * 8);
                    break;
                case ItemSizingType.Z1200:
                    measureChildSize = new Size(tempPageSize.Width * 12, tempPageSize.Height * 12);
                    break;
                case ItemSizingType.Z1600:
                    measureChildSize = new Size(tempPageSize.Width * 16, tempPageSize.Height * 16);
                    break;
                case ItemSizingType.Z3200:
                    measureChildSize = new Size(tempPageSize.Width * 32, tempPageSize.Height * 32);
                    break;
                case ItemSizingType.Z6400:
                    measureChildSize = new Size(tempPageSize.Width * 64, tempPageSize.Height * 64);
                    break;
                default:
                    double tempFactor = Double.Parse(this.ZoomText) / 100.0;
                    measureChildSize = new Size(tempPageSize.Width * tempFactor, tempPageSize.Height * tempFactor);
                    break;

            } 
            #endregion

            if (this.RotatePage == RotatePage.Rotate90 || this.RotatePage == RotatePage.Rotate90CounterClock)
            {
                Size tempSize = measureChildSize;
                measureChildSize = new Size(tempSize.Height, tempSize.Width);
            }

            #region Get First Child Size
            //{
            //    if (this.IsFileOpen == true)
            //    {
            //        this.IsChildSizeValid = true;
            //        ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            //        if (itemsControl != null && itemsControl.HasItems)
            //        {

            //            // Get the generator position of the first visible data item
            //            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(0);


            //            // Get index where we'd insert the child for this position. If the item is realized
            //            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            //            // insert after the corresponding child
            //            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            //            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            //            {

            //                bool newlyRealized;

            //                // Get or create the child
            //                UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
            //                if (newlyRealized)
            //                {
            //                    // Figure out if we need to insert the child at the end or somewhere in the middle
            //                    if (childIndex >= children.Count)
            //                    {
            //                        base.AddInternalChild(child);
            //                    }
            //                    else
            //                    {
            //                        base.InsertInternalChild(childIndex, child);
            //                    }
            //                    generator.PrepareItemContainer(child);
            //                }
            //                else
            //                {
            //                    // The child has already been created, let's be sure it's in the right spot
            //                    //Debug.Assert(child == children[childIndex], "Wrong child was generated");
            //                }

            //                // Measurements will depend on layout algorithm
            //                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //                this.OriginalChildSize = child.DesiredSize;


            //            }


            //            GeneratorPosition childGeneratorPos = new GeneratorPosition(0, 0);
            //            generator.Remove(childGeneratorPos, 1);
            //            RemoveInternalChildRange(0, 1);
            //        }
            //    }
            //} 
            #endregion

            Size prvChildSize = this.ChildSize;
            Size prvExtentedSize = this.extentSize;
            Size prvViewPortSize = this.viewPortSize;

            this.ChildSize = DoubleUtil.MeasureArrangeHelper(measureChildSize, this.PageSize, Stretch.Uniform, StretchDirection.Both);

            this.UpdateScrollInfo(availableSize);
            this.UpdateOffset(availableSize, prvExtentedSize, prvViewPortSize);

            {
                
                GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

                // Get the generator position of the first visible data item
                GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);


                // Get index where we'd insert the child for this position. If the item is realized
                // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
                // insert after the corresponding child
                int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

                using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
                {
                    for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; ++itemIndex, ++childIndex)
                    {
                        bool newlyRealized;

                        // Get or create the child
                        UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                        if (newlyRealized)
                        {
                            // Figure out if we need to insert the child at the end or somewhere in the middle
                            if (childIndex >= children.Count)
                            {
                                base.AddInternalChild(child);
                            }
                            else
                            {
                                base.InsertInternalChild(childIndex, child);
                            }
                            generator.PrepareItemContainer(child);
                        }
                        else
                        {
                            // The child has already been created, let's be sure it's in the right spot
                            //Debug.Assert(child == children[childIndex], "Wrong child was generated");
                        }

                        // Measurements will depend on layout algorithm
                        child.Measure(this.ChildSize);
                    }
                }

                //// Note: this could be deferred to idle time for efficiency
                CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);
            }
            this.currentPage = this.GetCurrentPage();
            if (this.CurrentPageChanged != null)
            {
                this.CurrentPageChanged(this, new CurrentPageChangedEventArgs(currentPage));
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            UpdateScrollInfo(finalSize);

            for (int index = 0; index < this.Children.Count; index++)
            {
                UIElement child = this.Children[index];

                // Map the child offset to an item offset
                int genIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(index, 0));

                ArrangeChild(genIndex, child, finalSize);
            }
            return finalSize;
        }
        /// <summary>
        /// Revirtualize items that are no longer visible
        /// </summary>
        /// <param name="minDesiredGenerated">first item index that should be visible</param>
        /// <param name="maxDesiredGenerated">last item index that should be visible</param>
        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated)
                {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        /// When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        } 
        #endregion

        #region Layout specific code
        //bool IsChildSizeValid { get; set; }
        Size ChildSize { get; set; }
        Size OriginalChildSize { get; set; }

        private Size CaculateChildSize(Size availableSize)
        {
            double height = availableSize.Height;
            if (Double.IsInfinity(availableSize.Height))
            {
                height = availableSize.Width;
            }
            double width = availableSize.Width;
            if (Double.IsInfinity(availableSize.Width))
            {
                height = 100;
                width = 100;
            }
            return new Size(width, height);
        }
        // I've isolated the layout specific code to this region. If you want to do something other than tiling, this is
        // where you'll make your changes

        /// <summary>
        /// Calculate the extent of the view based on the available size
        /// </summary>
        /// <param name="availableSize">available size</param>
        /// <param name="itemCount">number of data items</param>
        /// <returns></returns>
        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            int childrenPerRow = this.ChildrenPerRow;
 
            // See how big we are
            return new Size(childrenPerRow * this.ChildSize.Width,
                this.ChildSize.Height * Math.Ceiling((double)itemCount / childrenPerRow));
        }

        /// <summary>
        /// Get the range of children that are visible
        /// </summary>
        /// <param name="firstVisibleItemIndex">The item index of the first visible item</param>
        /// <param name="lastVisibleItemIndex">The item index of the last visible item</param>
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            int childrenPerRow = this.ChildrenPerRow;
            
            firstVisibleItemIndex = (int)Math.Floor(this.offset.Y / this.ChildSize.Height) * childrenPerRow;
            lastVisibleItemIndex = (int)Math.Ceiling((this.offset.Y + this.viewPortSize.Height) / this.ChildSize.Height) * childrenPerRow - 1;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            //caching
            firstVisibleItemIndex--;
            lastVisibleItemIndex++;

            if (lastVisibleItemIndex >= itemCount)
            {
                lastVisibleItemIndex = itemCount - 1;
            }
            if (firstVisibleItemIndex < 0)
            {
                firstVisibleItemIndex = 0;
            }
        }
        private int GetCurrentPage()
        {
            return (int)Math.Floor(this.offset.Y / this.ChildSize.Height) * this.ChildrenPerRow;
        }
        /// <summary>
        /// Get the size of the children. We assume they are all the same
        /// </summary>
        /// <returns>The size</returns>
        private Size GetChildSize()
        {
            return new Size(this.ChildSize.Width, this.ChildSize.Height);
        }

        /// <summary>
        /// Position a child
        /// </summary>
        /// <param name="itemIndex">The data item index of the child</param>
        /// <param name="child">The element to position</param>
        /// <param name="finalSize">The size of the panel</param>
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            int childrenPerRow = this.ChildrenPerRow;

            int row = itemIndex / childrenPerRow;
            int column = itemIndex % childrenPerRow;

            double tempX = column * this.ChildSize.Width;
            double tempY = row * this.ChildSize.Height;

            double tempEmpty = finalSize.Width - this.ChildSize.Width;

            if (tempEmpty > 0) 
            {
                if (this.PanelMode == PanelMode.SinglePage || this.PanelMode == PanelMode.ContniousPage)
                {
                    tempX += tempEmpty / 2;
                }
                else
                {
                    double tempTwoEmpty = finalSize.Width - this.ChildSize.Width * 2;
                    if (tempTwoEmpty > 0)
                    {
                        if (column == 0)
                        {
                            tempX += tempTwoEmpty / 4;
                        }
                        else
                        {
                            tempX += (tempTwoEmpty / 4) * 3;
                        }
                    }
                }
            }

            child.Arrange(new Rect(tempX,tempY , this.ChildSize.Width, this.ChildSize.Height));
        }

        /// <summary>
        /// Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns></returns>
        private int ChildrenPerRow
        {
            get
            {
                // Figure out how many children fit on each row
                return this.PanelMode == PanelMode.DoublePage || this.PanelMode == PanelMode.DoubleContniousPage ? 2 : 1;
            }
        }

        #endregion

        #region IScrollInfo Members

        private void UpdateScrollInfo(Size availableSize)
        {
            int itemCount = this.ChildCount;

            Size extent = CalculateExtent(availableSize, itemCount);
            // Update extent
            if (extent != this.extentSize)
            {
                this.extentSize = extent;
                if (this.ScrollOwner != null)
                    this.ScrollOwner.InvalidateScrollInfo();
            }

            // Update viewport
            if (availableSize != this.viewPortSize)
            {
                this.viewPortSize = availableSize;
                if (this.ScrollOwner != null)
                    this.ScrollOwner.InvalidateScrollInfo();
            }

        }

        private int ChildCount
        {
            get
            {
                // See how many items there are
                ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
                int itemCount = itemsControl != null ? itemsControl.Items.Count : 0;
                return itemCount;
            }
        }
        private void UpdateOffset(Size availableSize, Size prvExtendedSize,Size prvViewPortSize)
        {
            double horiOffset = (this.offset.X / (prvExtendedSize.Width - prvViewPortSize.Width)) * (this.extentSize.Width - this.viewPortSize.Width);
            double vertOffset = (this.offset.Y / (prvExtendedSize.Height - prvViewPortSize.Height)) * (this.extentSize.Height - this.viewPortSize.Height);

            this.SetVerticalOffset(vertOffset,false);
            this.SetHorizontalOffset(horiOffset,false);
        }

        public bool CanHorizontallyScroll{get;set;}

        public bool CanVerticallyScroll{get;set;}

        Size extentSize = new Size(1, 1);

        public double ExtentHeight
        {
            get { return extentSize.Height; }
        }

        public double ExtentWidth
        {
            get { return extentSize.Width; }
        }
        Vector offset = new Vector();
        public double HorizontalOffset
        {
            get { return offset.X; }
        }

        public virtual void LineDown()
        {
            MoveDown(this.VerticalOffset + SystemParameters.WheelScrollLines * 15);
        }

        private void MoveDown(double currentOffset)
        {
            if (this.PanelMode == PanelMode.SinglePage || this.PanelMode == PanelMode.DoublePage)
            {
                if (this.ChildSize.Height > this.ViewportHeight)
                {
                    double currentPageEnd = CalculateCurrentPageEnd();

                    if (currentOffset > currentPageEnd)
                    {
                        currentOffset = currentPageEnd;
                    }
                    this.SetVerticalOffset(currentOffset);
                }
            }
            else
            {
                this.SetVerticalOffset(currentOffset);
            }
        }

        private double CalculateCurrentPageEnd()
        {
            double retVal = 0.0;

            if (this.PanelMode == PanelMode.SinglePage)
            {
                retVal = ((this.CurrentPage + 1) * this.ChildSize.Height) - this.ViewportHeight;

            }
            else
            {
                retVal = ((((this.CurrentPage + 1) / this.ChildrenPerRow) * this.ChildSize.Height) + this.ChildSize.Height) - this.ViewportHeight;
            }

            return retVal ;
        }

        public virtual void LineLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - SystemParameters.WheelScrollLines * 5);
        }

        public virtual void LineRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + SystemParameters.WheelScrollLines * 5);
        }

        public virtual void LineUp()
        {
            this.MoveUp(this.VerticalOffset - SystemParameters.WheelScrollLines * 15);
        }

        private void MoveUp(double currentOffset)
        {
            if (this.PanelMode == PanelMode.SinglePage || this.PanelMode == PanelMode.DoublePage)
            {
                if (this.ChildSize.Height > this.ViewportHeight)
                {
                    double currentPageStart = CalculateCurrentPageStart();

                    if (currentOffset < currentPageStart)
                    {
                        currentOffset = currentPageStart;
                    }
                    this.SetVerticalOffset(currentOffset);
                }
            }
            else
            {
                this.SetVerticalOffset(currentOffset);
            }
        }

        private double CalculateCurrentPageStart()
        {
            double retVal;
            if (this.PanelMode == PanelMode.SinglePage)
            {
                retVal = this.CurrentPage * this.ChildSize.Height;
            }
            else
            {
                retVal = ((this.CurrentPage + 1) / this.ChildrenPerRow) * this.ChildSize.Height;
            }
            return retVal;
        }


        public Rect MakeVisible(Visual visual,Rect rectangle)
        {
            return rectangle;
        }

        public virtual void MouseWheelDown()
        {
            this.LineDown();
        }

        public virtual void MouseWheelLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - 3.0 );
        }

        public virtual void MouseWheelRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + 3.0 );
        }

        public virtual void MouseWheelUp()
        {
            this.LineUp();
        }

        public virtual void PageDown()
        {
            int tempPage = this.CurrentPage + this.ChildrenPerRow;
            if (tempPage < this.ChildCount)
            {
                this.CurrentPage = tempPage;
            }
        }

        public virtual void PageLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - this.ViewportWidth);
        }

        public virtual void PageRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + this.ViewportWidth);
        }

        public virtual void PageUp()
        {
            int tempPage = this.CurrentPage - this.ChildrenPerRow;
            if (tempPage >= 0)
            {
                this.CurrentPage = tempPage;
            }
        }


        public ScrollViewer ScrollOwner { get;set;}

        public void SetHorizontalOffset(double offset)
        {
            SetHorizontalOffset(offset, true);
        }
        public void SetHorizontalOffset(double offset, bool supressInvalidateMeasure)
        {
            if (offset < 1 || viewPortSize.Width >= extentSize.Width)
            {
                offset = 1;
            }
            else
            {
                if (offset + viewPortSize.Width >= extentSize.Width)
                {
                    offset = extentSize.Width - viewPortSize.Width;
                }
            }

            this.offset.X = offset;

            if (this.ScrollOwner != null)
            {
                this.ScrollOwner.InvalidateScrollInfo();
            }

            this.trans.X = -offset;

            if (supressInvalidateMeasure)
            {
                // Force us to realize the correct children
                InvalidateMeasure();
            }

        }
        private TranslateTransform trans = new TranslateTransform();
        public void SetVerticalOffset(double offset)
        {
            SetVerticalOffset(offset, true);
        }
        public void SetVerticalOffset(double offset, bool supressInvalidateMeasure)
        {

            if (offset < 0 || viewPortSize.Height >= extentSize.Height)
            {
                offset = 0;
            }

            else
            {
                if (offset + viewPortSize.Height >= extentSize.Height)
                {
                    offset = extentSize.Height - viewPortSize.Height;
                }
            }

            this.offset.Y = offset;

            if (this.ScrollOwner != null)
            {
                this.ScrollOwner.InvalidateScrollInfo();
            }

            this.trans.Y = -offset;
            if (supressInvalidateMeasure)
            {
                // Force us to realize the correct children
                InvalidateMeasure();
            }
        }

        public double VerticalOffset
        {
            get { return offset.Y; }
        }
        Size viewPortSize = new Size(1, 1);
        public double ViewportHeight
        {
            get { return viewPortSize.Height; }
        }

        public double ViewportWidth
        {
            get { return viewPortSize.Width; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void InternalPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }

    public class CurrentPageChangedEventArgs : EventArgs
    {
        public int CurrentPage{get;private set;}
        public CurrentPageChangedEventArgs(int pageNo):base()
        {
            this.CurrentPage = pageNo;
        }
    }

    public enum ModeType
    {
        FixedChild,
        VariableChild
    }

}
