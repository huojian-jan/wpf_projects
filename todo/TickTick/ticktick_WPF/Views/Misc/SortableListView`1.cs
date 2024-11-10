// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SortableListView`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SortableListView<T> : Grid where T : SortableListItemViewModel
  {
    public bool DragEnable = true;
    private double _startY = -1.0;
    private T _dragItem;
    private int _currentDragIndex;
    protected readonly ListView ItemList = new ListView();
    private readonly Popup _dragPopup = new Popup()
    {
      AllowsTransparency = true,
      Placement = PlacementMode.Relative
    };
    public double PopupContentHeight = 40.0;
    public FrameworkElement PopupContent;
    private bool _currentDragIsOpen;

    public SortableListView()
    {
      this.MouseMove += new MouseEventHandler(this.OnDragMove);
      this.ItemList.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this.ItemList.VerticalAlignment = VerticalAlignment.Top;
      this.Children.Add((UIElement) this.ItemList);
      this._dragPopup.PlacementTarget = (UIElement) this;
      this._dragPopup.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDrop);
    }

    public ObservableCollection<T> Items
    {
      get
      {
        return this.ItemList.ItemsSource is ObservableCollection<T> itemsSource ? itemsSource : new ObservableCollection<T>();
      }
    }

    public void TryDragItem(T item, MouseEventArgs e)
    {
      if (!this.DragEnable)
        return;
      this._startY = e.GetPosition((IInputElement) this).Y;
      this._dragItem = item;
      this._currentDragIndex = this.Items.IndexOf(item);
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      if ((object) this._dragItem == null)
        return;
      System.Windows.Point position1 = e.GetPosition((IInputElement) this);
      if (this._dragPopup.IsOpen)
      {
        if (this._dragItem.IsSection)
        {
          this._dragPopup.HorizontalOffset = position1.X - this.PopupContentHeight / 2.0;
          this._dragPopup.VerticalOffset = position1.Y - this.PopupContentHeight / 2.0;
        }
        else
        {
          this._dragPopup.HorizontalOffset = 10.0;
          this._dragPopup.VerticalOffset = position1.Y - this.PopupContentHeight / 2.0;
        }
        ListViewItem mousePositionItem = this.GetMousePositionItem(new System.Windows.Point(10.0, e.GetPosition((IInputElement) this).Y));
        if (!(mousePositionItem?.DataContext is T dataContext))
          return;
        int num = this.Items.IndexOf(dataContext);
        if (this._currentDragIndex < 0)
          return;
        System.Windows.Point position2 = e.GetPosition((IInputElement) mousePositionItem);
        if (num == this._currentDragIndex)
          return;
        int val2 = -1;
        if (this._currentDragIndex < num && position2.Y > mousePositionItem.ActualHeight / 2.0)
        {
          val2 = num;
          T obj = val2 + 1 < this.Items.Count ? this.Items[val2 + 1] : default (T);
          if (this._dragItem.IsSection && (object) obj != null && !obj.IsSection)
            return;
        }
        else if (this._currentDragIndex > num && position2.Y < mousePositionItem.ActualHeight / 2.0)
        {
          val2 = num;
          if (this._dragItem.IsSection && !dataContext.IsSection || !this._dragItem.IsSection && val2 == 0 && dataContext.IsSection)
            return;
        }
        if (val2 == this._currentDragIndex || val2 < 0 || val2 >= this.Items.Count)
          return;
        List<T> list = this.Items.ToList<T>();
        list.Remove(this._dragItem);
        list.Insert(Math.Min(list.Count, val2), this._dragItem);
        try
        {
          ItemsSourceHelper.SetItemsSource<T>((ItemsControl) this.ItemList, list);
        }
        catch (Exception ex)
        {
        }
        this._currentDragIndex = val2;
      }
      else
      {
        if (this._startY <= 0.0 || (object) this._dragItem == null || Math.Abs(position1.Y - this._startY) <= 8.0)
          return;
        this._dragItem.Dragging = true;
        this.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDrop);
        this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDrop);
        Mouse.Capture((IInputElement) this.ItemList);
        if (this._dragItem.IsSection)
          this.StartDragSection(this._dragItem);
        this.ShowDragPop(this._dragItem, position1);
      }
    }

    private void ShowDragPop(T model, System.Windows.Point position)
    {
      if (this.PopupContent != null)
      {
        if (this._dragPopup.Child == null)
          this._dragPopup.Child = (UIElement) this.PopupContent;
        this.PopupContent.DataContext = (object) model;
      }
      this._dragPopup.HorizontalOffset = position.X - 1.0 * (this.ActualWidth > 400.0 ? 200.0 : this.ActualWidth / 2.0);
      this._dragPopup.VerticalOffset = position.Y - this.PopupContentHeight / 2.0;
      this._dragPopup.Width = this.ActualWidth - 20.0;
      try
      {
        this._dragPopup.IsOpen = true;
      }
      catch (Exception ex)
      {
        UtilLog.Info("SortableListView.ShowDragPop error");
      }
    }

    private void StartDragSection(T model)
    {
      if (!model.IsOpen)
        return;
      ItemsSourceHelper.SetItemsSource<T>((ItemsControl) this.ItemList, this.Items.Where<T>((Func<T, bool>) (h => !h.IsSection || !(h.SectionId == model.Id))).ToList<T>());
      this._currentDragIsOpen = model.IsOpen;
      model.IsOpen = false;
    }

    private ListViewItem GetMousePositionItem(System.Windows.Point pos)
    {
      HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual) this, pos);
      return hitTestResult != null ? Utils.FindParent<ListViewItem>(hitTestResult.VisualHit) : (ListViewItem) null;
    }

    private void OnDrop(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      T dragItem = this._dragItem;
      if (!this._dragPopup.IsOpen && (object) this._dragItem != null)
      {
        this._dragItem.Dragging = false;
        this._dragItem = default (T);
      }
      this.ReleaseMouseCapture();
      this.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDrop);
      if ((object) dragItem != null && !dragItem.IsSection)
      {
        long num = dragItem.SortOrder;
        T obj1 = this._currentDragIndex > 0 ? this.Items[this._currentDragIndex - 1] : default (T);
        T obj2 = this._currentDragIndex < this.Items.Count - 1 ? this.Items[this._currentDragIndex + 1] : default (T);
        long? nullable1 = (object) obj1 == null || obj1.IsSection ? new long?() : new long?(obj1.SortOrder);
        long? nullable2 = (object) obj2 == null || obj2.IsSection ? new long?() : new long?(obj2.SortOrder);
        if (nullable1.HasValue || nullable2.HasValue)
          num = nullable1.HasValue ? (nullable2.HasValue ? nullable1.Value + (nullable2.Value - nullable1.Value) / 2L : nullable1.Value + 268435456L) : nullable2.Value - 268435456L;
        string str = dragItem.SectionId;
        if ((object) obj1 != null && !obj1.IsSection)
          str = obj1.SectionId;
        else if ((object) obj1 != null && obj1.IsSection)
          str = obj1.Id;
        if (num != dragItem.SortOrder || str != dragItem.SectionId)
        {
          dragItem.SectionId = str;
          dragItem.SortOrder = num;
          dragItem.SaveSortOrder();
        }
      }
      else if ((object) dragItem != null && dragItem.IsSection)
      {
        List<T> list = this.Items.Where<T>((Func<T, bool>) (v => v.IsSection)).ToList<T>();
        int num1 = list.IndexOf(dragItem);
        long num2 = SortableListView<T>.GetSortOrderBetween(num1 > 0 ? new long?(list[num1 - 1].SortOrder) : new long?(), num1 < list.Count - 1 ? new long?(list[num1 + 1].SortOrder) : new long?()) ?? dragItem.SortOrder;
        dragItem.IsOpen = this._currentDragIsOpen;
        if (num2 != dragItem.SortOrder)
        {
          dragItem.SortOrder = num2;
          dragItem.SortOrder = num2;
          dragItem.SaveSortOrder();
        }
      }
      if ((object) this._dragItem != null)
      {
        this._dragItem.Dragging = false;
        this._dragItem = default (T);
      }
      this._startY = -1.0;
      this._currentDragIndex = -1;
      this._dragPopup.IsOpen = false;
    }

    private static long? GetSortOrderBetween(long? front, long? next)
    {
      if (!front.HasValue && !next.HasValue)
        return new long?();
      if (!front.HasValue)
        return new long?(next.Value - 268435456L);
      return !next.HasValue ? new long?(front.Value + 268435456L) : new long?(front.Value + (next.Value - front.Value) / 2L);
    }
  }
}
