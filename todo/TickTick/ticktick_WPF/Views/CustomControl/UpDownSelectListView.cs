// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.UpDownSelectListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class UpDownSelectListView : ListView, ITabControl, IComponentConnector, IStyleConnector
  {
    private System.Windows.Point _movePoint;
    private DateTime _lastMoveTime;
    private UpDownSelectViewModel _mouseDownItem;
    private bool _contentLoaded;

    public bool CanBatchSelected { get; set; }

    public event UpDownSelectListViewItemSelect ItemSelected;

    public event EventHandler<bool> LeftRightKeyDown;

    public bool CanUpDownOverLimit { get; set; }

    public bool CanTabSelect { get; set; } = true;

    public bool NeedHandleItemEnter { get; set; } = true;

    public UpDownSelectListView() => this.InitializeComponent();

    public bool HandleTab(bool shift) => this.CanTabSelect && this.UpDownSelect(shift);

    public bool HandleEsc() => false;

    public bool LeftRightSelect(bool isLeft)
    {
      if (!(this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource) || itemsSource.ToList<UpDownSelectViewModel>().ToList<UpDownSelectViewModel>().FirstOrDefault<UpDownSelectViewModel>((Func<UpDownSelectViewModel, bool>) (m => m.HoverSelected)) == null)
        return false;
      EventHandler<bool> leftRightKeyDown = this.LeftRightKeyDown;
      if (leftRightKeyDown != null)
        leftRightKeyDown((object) this, isLeft);
      return true;
    }

    public bool Focused() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource)
      {
        List<UpDownSelectViewModel> list = itemsSource.ToList<UpDownSelectViewModel>();
        int index = list.ToList<UpDownSelectViewModel>().FindIndex((Predicate<UpDownSelectViewModel>) (m => m.HoverSelected));
        if (index < 0)
        {
          UpDownSelectViewModel downSelectViewModel = isUp ? list.LastOrDefault<UpDownSelectViewModel>((Func<UpDownSelectViewModel, bool>) (i => i.IsEnable)) : list.FirstOrDefault<UpDownSelectViewModel>((Func<UpDownSelectViewModel, bool>) (i => i.IsEnable));
          if (downSelectViewModel != null)
          {
            downSelectViewModel.HoverSelected = true;
            this.ScrollIntoView((object) downSelectViewModel);
          }
        }
        else
        {
          if (list[index].SubOpened)
            return false;
          list[index].HoverSelected = false;
          do
          {
            int num = index + (isUp ? -1 : 1);
            if (this.CanUpDownOverLimit && (num < 0 || num >= list.Count))
              return false;
            index = (num + list.Count) % list.Count;
          }
          while (!list[index].IsEnable);
          list[index].HoverSelected = true;
          this.ScrollIntoView((object) list[index]);
        }
      }
      return true;
    }

    public bool HandleEnter()
    {
      if (this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource)
      {
        List<UpDownSelectViewModel> list = itemsSource.ToList<UpDownSelectViewModel>();
        int index = list.ToList<UpDownSelectViewModel>().FindIndex((Predicate<UpDownSelectViewModel>) (m => m.HoverSelected));
        if (index >= 0)
        {
          UpDownSelectViewModel downSelectViewModel = list[index];
          if (downSelectViewModel.HasChildren)
          {
            EventHandler<bool> leftRightKeyDown = this.LeftRightKeyDown;
            if (leftRightKeyDown != null)
              leftRightKeyDown((object) this, false);
            return false;
          }
          this.SelectItem((IEnumerable<UpDownSelectViewModel>) list, downSelectViewModel, true);
        }
      }
      return true;
    }

    private void SelectItem(
      IEnumerable<UpDownSelectViewModel> items,
      UpDownSelectViewModel item,
      bool onEnter = false)
    {
      if (item.Selectable)
      {
        if (!this.CanBatchSelected)
        {
          foreach (UpDownSelectViewModel downSelectViewModel in items)
            downSelectViewModel.Selected = false;
          item.Selected = true;
        }
        else
          item.Selected = !item.Selected;
        if (item.HasChildren)
          item.Selected = false;
      }
      UpDownSelectListViewItemSelect itemSelected = this.ItemSelected;
      if (itemSelected == null)
        return;
      itemSelected(onEnter, item);
    }

    private async void OnItemMouseEnter(object sender, MouseEventArgs e)
    {
      if (!this.NeedHandleItemEnter || !Utils.IsEmptyDate(this._lastMoveTime) && (DateTime.Now - this._lastMoveTime).TotalMilliseconds > 200.0)
        return;
      this.ClearHover();
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is UpDownSelectViewModel dataContext))
        return;
      dataContext.HoverSelected = true;
    }

    private async void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      UpDownSelectListView downSelectListView = this;
      // ISSUE: explicit non-virtual call
      if ((sender is FrameworkElement frameworkElement1 ? (__nonvirtual (frameworkElement1.IsMouseOver) ? 1 : 0) : 0) == 0)
        return;
      await Task.Delay(50);
      if (e.Handled || !(sender is FrameworkElement frameworkElement2) || !(frameworkElement2.DataContext is UpDownSelectViewModel dataContext) || dataContext != downSelectListView._mouseDownItem || !dataContext.IsEnable || !(downSelectListView.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource))
        return;
      downSelectListView._mouseDownItem = (UpDownSelectViewModel) null;
      downSelectListView.SelectItem(itemsSource, dataContext);
      e.Handled = true;
      int num = dataContext.Selectable ? 1 : 0;
    }

    public ListViewItem GetHoverSelectedItem()
    {
      if (this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource)
      {
        UpDownSelectViewModel downSelectViewModel = itemsSource.ToList<UpDownSelectViewModel>().ToList<UpDownSelectViewModel>().FirstOrDefault<UpDownSelectViewModel>((Func<UpDownSelectViewModel, bool>) (m => m.HoverSelected));
        if (downSelectViewModel != null)
          return this.ItemContainerGenerator.ContainerFromItem((object) downSelectViewModel) as ListViewItem;
      }
      return (ListViewItem) null;
    }

    public void ClearHover()
    {
      if (!(this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource))
        return;
      foreach (UpDownSelectViewModel downSelectViewModel in itemsSource)
        downSelectViewModel.HoverSelected = false;
    }

    private void OnViewMouseMove(object sender, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (Math.Abs(position.X - this._movePoint.X) <= 2.0 && Math.Abs(position.Y - this._movePoint.Y) <= 2.0)
        return;
      this._lastMoveTime = DateTime.Now;
      this._movePoint = position;
    }

    public void HoverFirst()
    {
      if (!(this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource))
        return;
      int num = 0;
      foreach (UpDownSelectViewModel downSelectViewModel in itemsSource)
      {
        downSelectViewModel.HoverSelected = num == 0;
        ++num;
      }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (!(this.ItemsSource is IEnumerable<UpDownSelectViewModel> itemsSource))
        return;
      foreach (UpDownSelectViewModel downSelectViewModel in itemsSource)
      {
        if (downSelectViewModel.HoverSelected && !downSelectViewModel.SubOpened)
          downSelectViewModel.HoverSelected = false;
      }
    }

    private void OnItemDown(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is UpDownSelectViewModel dataContext) || !dataContext.IsEnable)
        return;
      this._mouseDownItem = dataContext;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customcontrol/updownselectlistview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
      {
        ((UIElement) target).MouseMove += new MouseEventHandler(this.OnViewMouseMove);
        ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      }
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnItemDown);
      ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
      ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
    }
  }
}
