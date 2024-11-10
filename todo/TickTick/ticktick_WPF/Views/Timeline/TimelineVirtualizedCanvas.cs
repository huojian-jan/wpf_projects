// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineVirtualizedCanvas
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineVirtualizedCanvas : Canvas
  {
    private readonly SemaphoreSlim _checkLocker = new SemaphoreSlim(1, 1);
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof (ItemsSource), typeof (TimeLineItemList), typeof (TimelineVirtualizedCanvas), new PropertyMetadata(new PropertyChangedCallback(TimelineVirtualizedCanvas.OnItemsSourcePropertyChanged)));
    public static readonly DependencyProperty ViewHeightProperty = DependencyProperty.Register(nameof (ViewHeight), typeof (double), typeof (TimelineVirtualizedCanvas), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineVirtualizedCanvas.OnViewSizeChanged)));
    public static readonly DependencyProperty ViewWidthProperty = DependencyProperty.Register(nameof (ViewWidth), typeof (double), typeof (TimelineVirtualizedCanvas), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineVirtualizedCanvas.OnViewSizeChanged)));
    public static readonly DependencyProperty HOffsetProperty = DependencyProperty.Register(nameof (HOffset), typeof (double), typeof (TimelineVirtualizedCanvas), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineVirtualizedCanvas.OnViewSizeChanged)));
    public static readonly DependencyProperty VOffsetProperty = DependencyProperty.Register(nameof (VOffset), typeof (double), typeof (TimelineVirtualizedCanvas), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineVirtualizedCanvas.OnViewSizeChanged)));
    public static readonly DependencyProperty IsResetProperty = DependencyProperty.Register(nameof (IsReset), typeof (bool), typeof (TimelineVirtualizedCanvas), new PropertyMetadata((object) false));
    private readonly BlockingSet<TimelineCellInline> _hideItemsDict = new BlockingSet<TimelineCellInline>();
    private readonly ConcurrentDictionary<TimelineCellViewModel, TimelineCellInline> _showItemsDict = new ConcurrentDictionary<TimelineCellViewModel, TimelineCellInline>();
    private bool _check;
    private double _checkedHOffset;
    private double _checkedVOffset;
    private double _checkedHeight;
    private double _checkedWidth;
    private long _lastCheckTime;

    public bool IsReset
    {
      get => (bool) this.GetValue(TimelineVirtualizedCanvas.IsResetProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.IsResetProperty, (object) value);
    }

    public TimeLineItemList ItemsSource
    {
      get => (TimeLineItemList) this.GetValue(TimelineVirtualizedCanvas.ItemsSourceProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.ItemsSourceProperty, (object) value);
    }

    private static void OnItemsSourcePropertyChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is TimelineVirtualizedCanvas virtualizedCanvas))
        return;
      virtualizedCanvas.OnItemsSourceChanged(e.OldValue as TimeLineItemList, e.NewValue as TimeLineItemList);
    }

    private void OnItemsSourceChanged(TimeLineItemList oldValue, TimeLineItemList newValue)
    {
      if (oldValue != null)
        oldValue.ItemsChange -= new EventHandler<ListItemChangeArgs<TimelineCellViewModel>>(this.OnItemsSourceChanged);
      if (newValue == null)
        return;
      newValue.ClearEvents();
      newValue.ItemsChange += new EventHandler<ListItemChangeArgs<TimelineCellViewModel>>(this.OnItemsSourceChanged);
    }

    public double ViewHeight
    {
      get => (double) this.GetValue(TimelineVirtualizedCanvas.ViewHeightProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.ViewHeightProperty, (object) value);
    }

    public double ViewWidth
    {
      get => (double) this.GetValue(TimelineVirtualizedCanvas.ViewWidthProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.ViewWidthProperty, (object) value);
    }

    public double HOffset
    {
      get => (double) this.GetValue(TimelineVirtualizedCanvas.HOffsetProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.HOffsetProperty, (object) value);
    }

    public double VOffset
    {
      get => (double) this.GetValue(TimelineVirtualizedCanvas.VOffsetProperty);
      set => this.SetValue(TimelineVirtualizedCanvas.VOffsetProperty, (object) value);
    }

    private void OnItemsSourceChanged(object sender, ListItemChangeArgs<TimelineCellViewModel> e)
    {
      switch (e.Action)
      {
        case ListChangeAction.Add:
          using (List<TimelineCellViewModel>.Enumerator enumerator = e.Items.GetEnumerator())
          {
            while (enumerator.MoveNext())
              this.CheckAndSetCellModelVisible(enumerator.Current);
            break;
          }
        case ListChangeAction.Remove:
          if (e.Items == null)
            break;
          using (List<TimelineCellViewModel>.Enumerator enumerator = e.Items.GetEnumerator())
          {
            while (enumerator.MoveNext())
              this.HideItem(enumerator.Current);
            break;
          }
        case ListChangeAction.Clear:
          using (List<TimelineCellViewModel>.Enumerator enumerator = e.Items.GetEnumerator())
          {
            while (enumerator.MoveNext())
              this.HideItem(enumerator.Current);
            break;
          }
        case ListChangeAction.Change:
          HashSet<TimelineCellViewModel> models = new HashSet<TimelineCellViewModel>((IEnumerable<TimelineCellViewModel>) this.ItemsSource.ToList());
          using (IEnumerator<TimelineCellViewModel> enumerator = e.Items.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => models.Contains(item))).GetEnumerator())
          {
            while (enumerator.MoveNext())
              this.CheckAndSetCellModelVisible(enumerator.Current);
            break;
          }
      }
    }

    private void ShowItem(TimelineCellViewModel model)
    {
      model.SetColorAndAvatar();
      TimelineCellInline m = this._hideItemsDict.FirstOrDefault();
      if (m != null)
      {
        this._hideItemsDict.Remove(m);
        this._showItemsDict[model] = m;
        m.Opacity = 1.0;
        m.DataContext = (object) model;
      }
      else
      {
        TimelineCellInline timelineCellInline = new TimelineCellInline();
        timelineCellInline.DataContext = (object) model;
        TimelineCellInline element = timelineCellInline;
        this.Children.Add((UIElement) element);
        this._showItemsDict[model] = element;
      }
    }

    private void HideItem(TimelineCellViewModel model)
    {
      TimelineCellInline model1;
      if (model == null || !this._showItemsDict.TryRemove(model, out model1))
        return;
      model1.DataContext = (object) null;
      model1.Opacity = 0.0;
      this._hideItemsDict.Add(model1);
    }

    private async Task CheckAndSetAllCellVisible()
    {
      if (this.ItemsSource == null)
        return;
      long lastCheckTime = DateTime.Now.Ticks;
      this._lastCheckTime = lastCheckTime;
      await this._checkLocker.WaitAsync();
      try
      {
        if (lastCheckTime != this._lastCheckTime)
          return;
        this._checkedHOffset = this.HOffset;
        this._checkedVOffset = this.VOffset;
        this._checkedHeight = this.ViewHeight;
        this._checkedWidth = this.ViewWidth;
        foreach (TimelineCellViewModel timelineCellViewModel in this.ItemsSource.ToList())
        {
          if (this.CheckModelVisible(timelineCellViewModel))
          {
            if (!this._showItemsDict.ContainsKey(timelineCellViewModel))
              this.ShowItem(timelineCellViewModel);
          }
          else
            this.HideItem(timelineCellViewModel);
        }
      }
      finally
      {
        this._checkLocker.Release();
      }
    }

    private bool CheckModelVisible(TimelineCellViewModel model)
    {
      return model.Top >= 0.0 && model.Left <= this.ViewWidth + this.HOffset + 550.0 && model.Left + model.Width >= this.HOffset - 550.0 && model.Top <= this.ViewHeight + this.VOffset + 100.0 && model.Top >= this.VOffset - 100.0;
    }

    private async Task CheckAndSetCellModelVisible(TimelineCellViewModel model)
    {
      await this._checkLocker.WaitAsync();
      try
      {
        if (this.CheckModelVisible(model))
        {
          if (this._showItemsDict.ContainsKey(model))
            return;
          this.ShowItem(model);
        }
        else
          this.HideItem(model);
      }
      finally
      {
        this._checkLocker.Release();
      }
    }

    private static void OnViewSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TimelineVirtualizedCanvas virtualizedCanvas))
        return;
      switch (e.Property.Name)
      {
        case "HOffset":
          if (Math.Abs(virtualizedCanvas._checkedHOffset - virtualizedCanvas.HOffset) < 500.0)
            return;
          break;
        case "VOffset":
          if (Math.Abs(virtualizedCanvas._checkedVOffset - virtualizedCanvas.VOffset) < 50.0)
            return;
          break;
        case "ViewHeight":
          if (Math.Abs(virtualizedCanvas._checkedHeight - virtualizedCanvas.ViewHeight) < 50.0)
            return;
          break;
        case "ViewWidth":
          if (Math.Abs(virtualizedCanvas._checkedWidth - virtualizedCanvas.ViewWidth) < 500.0)
            return;
          break;
      }
      virtualizedCanvas.CheckAndSetAllCellVisible();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      this.CheckAndSetAllCellVisible();
    }

    public void TryShowDetailWindow(TimelineCellViewModel cellModel)
    {
      TimelineCellInline timelineCellInline;
      if (!this._showItemsDict.TryGetValue(cellModel, out timelineCellInline))
        return;
      timelineCellInline.ShowDetail();
    }
  }
}
