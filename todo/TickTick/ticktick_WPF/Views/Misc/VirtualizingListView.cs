// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.VirtualizingListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class VirtualizingListView : VirtualizingPanel, IScrollInfo
  {
    private const double ScrollLineAmount = 16.0;
    private Size _extentSize;
    private Size _viewportSize;
    private System.Windows.Point _offset;
    private ItemsControl _itemsControl;
    private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof (ItemWidth), typeof (double), typeof (VirtualizingListView), new PropertyMetadata((object) -1.0, new PropertyChangedCallback(VirtualizingListView.HandleItemDimensionChanged)));
    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof (ItemHeight), typeof (double), typeof (VirtualizingListView), new PropertyMetadata((object) 1.0, new PropertyChangedCallback(VirtualizingListView.HandleItemDimensionChanged)));
    private static readonly DependencyProperty VirtualItemIndexProperty = DependencyProperty.RegisterAttached("VirtualItemIndex", typeof (int), typeof (VirtualizingListView), new PropertyMetadata((object) -1));
    private IRecyclingItemContainerGenerator _itemsGenerator;
    private bool _isInMeasure;
    private bool _needRemeasure;

    private static int GetVirtualItemIndex(DependencyObject obj)
    {
      return (int) obj.GetValue(VirtualizingListView.VirtualItemIndexProperty);
    }

    private static void SetVirtualItemIndex(DependencyObject obj, int value)
    {
      obj.SetValue(VirtualizingListView.VirtualItemIndexProperty, (object) value);
    }

    public double ItemHeight
    {
      get => (double) this.GetValue(VirtualizingListView.ItemHeightProperty);
      set => this.SetValue(VirtualizingListView.ItemHeightProperty, (object) value);
    }

    public double ItemWidth
    {
      get => (double) this.GetValue(VirtualizingListView.ItemWidthProperty);
      set => this.SetValue(VirtualizingListView.ItemWidthProperty, (object) value);
    }

    public VirtualizingListView()
    {
      if (DesignerProperties.GetIsInDesignMode((DependencyObject) this))
        return;
      this.Dispatcher.BeginInvoke((Delegate) new Action(this.Initialize));
    }

    private void Initialize()
    {
      this._itemsControl = ItemsControl.GetItemsOwner((DependencyObject) this);
      this._itemsGenerator = (IRecyclingItemContainerGenerator) this.ItemContainerGenerator;
      this.InvalidateMeasure();
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
      base.OnItemsChanged(sender, args);
      this.InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      if (this._itemsControl == null)
        return new Size(double.IsInfinity(availableSize.Width) ? 0.0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0.0 : availableSize.Height);
      if (this._itemsGenerator == null)
      {
        this._itemsGenerator = (IRecyclingItemContainerGenerator) this.ItemContainerGenerator;
        if (this._itemsGenerator == null)
          return new Size(double.IsInfinity(availableSize.Width) ? 0.0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0.0 : availableSize.Height);
      }
      this._isInMeasure = true;
      this._childLayouts.Clear();
      (VirtualizingListView.ExtentInfo extentInfo, VirtualizingListView.ItemLayoutInfo layoutInfo) = this.GetInfos(availableSize);
      this.RecycleItems(layoutInfo);
      GeneratorPosition position = this._itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);
      int index = 0;
      double firstRealizedLineTop = layoutInfo.FirstRealizedLineTop;
      using (this._itemsGenerator.StartAt(position, GeneratorDirection.Forward, true))
      {
        int realizedItemIndex = layoutInfo.FirstRealizedItemIndex;
        while (realizedItemIndex <= layoutInfo.LastRealizedItemIndex)
        {
          if (realizedItemIndex >= 0)
          {
            bool isNewlyRealized;
            FrameworkElement next = (FrameworkElement) this._itemsGenerator.GenerateNext(out isNewlyRealized);
            VirtualizingListView.SetVirtualItemIndex((DependencyObject) next, realizedItemIndex);
            if (isNewlyRealized)
              this.InsertInternalChild(index, (UIElement) next);
            else if (!this.Children.Contains((UIElement) next))
              this.AddInternalChild((UIElement) next);
            this._itemsGenerator.PrepareItemContainer((DependencyObject) next);
            double width = this.ItemWidth < 0.0 ? availableSize.Width : this.ItemWidth;
            double itemHeight = this.ItemHeight;
            if (next.DataContext is IVirtualizingItem dataContext)
              itemHeight = dataContext.GetItemHeight();
            this._childLayouts.Add((UIElement) next, new Rect(0.0, firstRealizedLineTop, width, itemHeight));
            firstRealizedLineTop += itemHeight;
            ++realizedItemIndex;
            ++index;
          }
          else
            break;
        }
      }
      this.EnsureScrollOffsetIsWithinConstrains(extentInfo);
      this.RemoveRedundantChildren();
      this.UpdateScrollInfo(availableSize, extentInfo);
      Size size = new Size(double.IsInfinity(availableSize.Width) ? 0.0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0.0 : availableSize.Height);
      this._isInMeasure = false;
      if (!this._needRemeasure)
        return size;
      this.TryInvalidateMeasure();
      return size;
    }

    private void EnsureScrollOffsetIsWithinConstrains(VirtualizingListView.ExtentInfo extentInfo)
    {
      this._offset.Y = this.Clamp(this._offset.Y, 0.0, extentInfo.MaxVerticalOffset);
    }

    private void RecycleItems(VirtualizingListView.ItemLayoutInfo layoutInfo)
    {
      foreach (DependencyObject child in this.Children)
        VirtualizingListView.SetVirtualItemIndex(child, -1);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      foreach (UIElement child in this.Children)
        child.Arrange(this._childLayouts[child]);
      return finalSize;
    }

    private void UpdateScrollInfo(Size availableSize, VirtualizingListView.ExtentInfo extentInfo)
    {
      this._viewportSize = availableSize;
      this._extentSize = new Size(availableSize.Width, extentInfo.ExtentHeight);
      this.InvalidateScrollInfo();
    }

    private void RemoveRedundantChildren()
    {
      for (int index = this.Children.Count - 1; index >= 0; --index)
      {
        if (VirtualizingListView.GetVirtualItemIndex((DependencyObject) this.Children[index]) == -1)
          this.RemoveInternalChildRange(index, 1);
      }
    }

    private (VirtualizingListView.ExtentInfo, VirtualizingListView.ItemLayoutInfo) GetInfos(
      Size viewPortSize)
    {
      if (this._itemsControl == null)
        return (new VirtualizingListView.ExtentInfo(), new VirtualizingListView.ItemLayoutInfo());
      VirtualizingListView.ExtentInfo extentInfo = new VirtualizingListView.ExtentInfo();
      VirtualizingListView.ItemLayoutInfo itemLayoutInfo = new VirtualizingListView.ItemLayoutInfo();
      List<IVirtualizingItem> virtualizingItemList = (List<IVirtualizingItem>) null;
      try
      {
        virtualizingItemList = this._itemsControl.ItemsSource.Cast<IVirtualizingItem>().ToList<IVirtualizingItem>();
      }
      catch (Exception ex)
      {
      }
      if (virtualizingItemList != null)
      {
        double num1 = 0.0;
        int num2 = -1;
        double num3 = -1.0;
        for (int index = 0; index < virtualizingItemList.Count; ++index)
        {
          IVirtualizingItem virtualizingItem = virtualizingItemList[index];
          num1 += virtualizingItem.GetItemHeight();
          if (num2 < 0 && num1 >= this.VerticalOffset)
          {
            num2 = Math.Max(0, index - 2);
            num3 = num1 - this.VerticalOffset - virtualizingItem.GetItemHeight() - (index >= 2 ? virtualizingItemList[index - 2].GetItemHeight() : 0.0) - (index >= 1 ? virtualizingItemList[index - 1].GetItemHeight() : 0.0);
          }
        }
        int num4 = (int) Math.Ceiling((viewPortSize.Height - num3) / this.ItemHeight);
        itemLayoutInfo.FirstRealizedItemIndex = num2;
        itemLayoutInfo.FirstRealizedLineTop = num3;
        itemLayoutInfo.LastRealizedItemIndex = Math.Min(num2 + num4 + 2, this._itemsControl.Items.Count - 1);
        extentInfo.ExtentHeight = num1;
      }
      else
      {
        double num5 = Math.Max((double) this._itemsControl.Items.Count * this.ItemHeight, viewPortSize.Height);
        extentInfo.ExtentHeight = num5 + 10.0;
        int num6 = (int) Math.Floor(this.VerticalOffset / this.ItemHeight);
        int num7 = Math.Max(num6 - 2, 0);
        double num8 = (double) num7 * this.ItemHeight - this.VerticalOffset;
        double num9 = num6 == 0 ? num8 : num8 + this.ItemHeight;
        int num10 = (int) Math.Ceiling((viewPortSize.Height - num9) / this.ItemHeight);
        int num11 = Math.Min(num7 + num10 + 2, this._itemsControl.Items.Count - 1);
        itemLayoutInfo.FirstRealizedItemIndex = num7;
        itemLayoutInfo.FirstRealizedLineTop = num8;
        itemLayoutInfo.LastRealizedItemIndex = num11;
      }
      extentInfo.MaxVerticalOffset = extentInfo.ExtentHeight - viewPortSize.Height;
      return (extentInfo, itemLayoutInfo);
    }

    public void LineUp() => this.SetVerticalOffset(this.VerticalOffset - 16.0);

    public void LineDown() => this.SetVerticalOffset(this.VerticalOffset + 16.0);

    public void LineLeft() => this.SetHorizontalOffset(this.HorizontalOffset + 16.0);

    public void LineRight() => this.SetHorizontalOffset(this.HorizontalOffset - 16.0);

    public void PageUp() => this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight);

    public void PageDown() => this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight);

    public void PageLeft() => this.SetHorizontalOffset(this.HorizontalOffset + this.ActualWidth);

    public void PageRight() => this.SetHorizontalOffset(this.HorizontalOffset - this.ActualWidth);

    public void MouseWheelUp()
    {
      this.SetVerticalOffset(this.VerticalOffset - 16.0 * (double) SystemParameters.WheelScrollLines);
    }

    public void MouseWheelDown()
    {
      this.SetVerticalOffset(this.VerticalOffset + 16.0 * (double) SystemParameters.WheelScrollLines);
    }

    public void MouseWheelLeft()
    {
      this.SetHorizontalOffset(this.HorizontalOffset - 16.0 * (double) SystemParameters.WheelScrollLines);
    }

    public void MouseWheelRight()
    {
      this.SetHorizontalOffset(this.HorizontalOffset + 16.0 * (double) SystemParameters.WheelScrollLines);
    }

    public void SetHorizontalOffset(double offset)
    {
      if (this._isInMeasure)
        return;
      offset = this.Clamp(offset, 0.0, this.ExtentWidth - this.ViewportWidth);
      this._offset = new System.Windows.Point(offset, this._offset.Y);
      this.InvalidateScrollInfo();
      this.InvalidateMeasure();
    }

    public void SetVerticalOffset(double offset)
    {
      if (this._isInMeasure)
        return;
      offset = this.Clamp(offset, 0.0, this.ExtentHeight - this.ViewportHeight);
      this._offset = new System.Windows.Point(this._offset.X, offset);
      this.InvalidateScrollInfo();
      this.InvalidateMeasure();
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
      if (rectangle.IsEmpty || visual == null || visual == this || !this.IsAncestorOf((DependencyObject) visual))
        return Rect.Empty;
      rectangle = visual.TransformToAncestor((Visual) this).TransformBounds(rectangle);
      Rect rect = new Rect(this.HorizontalOffset, this.VerticalOffset, this.ViewportWidth, this.ViewportHeight);
      rectangle.X += rect.X;
      rectangle.Y += rect.Y;
      rect.X = VirtualizingListView.CalculateNewScrollOffset(rect.Left, rect.Right, rectangle.Left, rectangle.Right);
      rect.Y = VirtualizingListView.CalculateNewScrollOffset(rect.Top, rect.Bottom, rectangle.Top, rectangle.Bottom);
      this.SetHorizontalOffset(rect.X);
      this.SetVerticalOffset(rect.Y);
      rectangle.Intersect(rect);
      rectangle.X -= rect.X;
      rectangle.Y -= rect.Y;
      return rectangle;
    }

    private static double CalculateNewScrollOffset(
      double topView,
      double bottomView,
      double topChild,
      double bottomChild)
    {
      bool flag1 = topChild < topView && bottomChild < bottomView;
      bool flag2 = bottomChild > bottomView && topChild > topView;
      bool flag3 = bottomChild - topChild > bottomView - topView;
      if (!flag1 && !flag2)
        return topView;
      return flag1 && !flag3 || flag2 & flag3 ? topChild : bottomChild - (bottomView - topView);
    }

    public bool CanVerticallyScroll { get; set; }

    public bool CanHorizontallyScroll { get; set; }

    public double ExtentWidth => this._extentSize.Width;

    public double ExtentHeight => this._extentSize.Height;

    public double ViewportWidth => this._viewportSize.Width;

    public double ViewportHeight => this._viewportSize.Height;

    public double HorizontalOffset => this._offset.X;

    public double VerticalOffset => this._offset.Y;

    public ScrollViewer ScrollOwner { get; set; }

    private void InvalidateScrollInfo()
    {
      if (this.ScrollOwner == null)
        return;
      this.ScrollOwner.InvalidateScrollInfo();
    }

    private static void HandleItemDimensionChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is VirtualizingListView virtualizingListView))
        return;
      virtualizingListView.InvalidateMeasure();
    }

    private double Clamp(double value, double min, double max)
    {
      return Math.Min(Math.Max(value, min), max);
    }

    public void TryInvalidateMeasure()
    {
      if (this._isInMeasure)
        this._needRemeasure = true;
      else
        this.InvalidateMeasure();
    }

    internal class ExtentInfo
    {
      public double ExtentHeight;
      public double MaxVerticalOffset;
    }

    public class ItemLayoutInfo
    {
      public int FirstRealizedItemIndex;
      public double FirstRealizedLineTop;
      public int LastRealizedItemIndex;
    }
  }
}
