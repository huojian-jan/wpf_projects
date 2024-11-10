// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.MultiDayAllDayItems
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
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class MultiDayAllDayItems : Canvas
  {
    private static int _initOffset = 4000;
    private MultiDayView _parent;
    private readonly SemaphoreSlim _checkLocker = new SemaphoreSlim(1, 1);
    private List<WeekEventModel> _itemsSource;
    public double ViewHeight;
    public double ViewWidth;
    public double HOffset;
    public double VOffset;
    private readonly BlockingSet<TaskBar> _hideItems = new BlockingSet<TaskBar>();
    private readonly ConcurrentDictionary<WeekEventModel, TaskBar> _showItemsDict = new ConcurrentDictionary<WeekEventModel, TaskBar>();
    private long _lastCheckTime;
    private double _dayWidth;

    public MultiDayAllDayItems(MultiDayView parent)
    {
      this._parent = parent;
      this.Width = 8000.0;
      this._itemsSource = new List<WeekEventModel>();
    }

    public async void SetItemsSource(List<WeekEventModel> items)
    {
      MultiDayAllDayItems multiDayAllDayItems = this;
      await multiDayAllDayItems._checkLocker.WaitAsync();
      try
      {
        multiDayAllDayItems.HideAllItems();
        multiDayAllDayItems._itemsSource = items;
        foreach (WeekEventModel weekEventModel in items)
        {
          if (multiDayAllDayItems.CheckModelVisible(weekEventModel))
          {
            if (!multiDayAllDayItems._showItemsDict.ContainsKey(weekEventModel))
              multiDayAllDayItems.ShowItem(weekEventModel);
          }
          else
            multiDayAllDayItems.HideItem(weekEventModel);
        }
        // ISSUE: explicit non-virtual call
        __nonvirtual (multiDayAllDayItems.Height) = multiDayAllDayItems._itemsSource.Any<WeekEventModel>() ? (double) (multiDayAllDayItems._itemsSource.Max<WeekEventModel>((Func<WeekEventModel, int>) (m => m.Row)) * 18 + 20) : 0.0;
      }
      finally
      {
        multiDayAllDayItems._checkLocker.Release();
      }
    }

    private void ShowItem(WeekEventModel model)
    {
      TaskBar taskBar1 = this._hideItems.FirstOrDefault();
      if (taskBar1 != null)
      {
        this._hideItems.Remove(taskBar1);
        this._showItemsDict[model] = taskBar1;
        taskBar1.Visibility = Visibility.Visible;
        taskBar1.DataContext = (object) model.Data;
      }
      else
      {
        TaskBar taskBar2 = new TaskBar();
        taskBar2.DataContext = (object) model.Data;
        TaskBar element = taskBar2;
        this.Children.Add((UIElement) element);
        this._showItemsDict[model] = element;
        taskBar1 = element;
      }
      Canvas.SetTop((UIElement) taskBar1, (double) (model.Row * 18));
      Canvas.SetLeft((UIElement) taskBar1, (double) MultiDayAllDayItems._initOffset + (double) model.Column * this._parent.DayViewWidth + 1.0);
      taskBar1.Width = (double) model.ColumnSpan * this._parent.DayViewWidth;
      taskBar1.SetTitlePosition(model.Column, model.ColumnSpan, this._parent.DayViewWidth);
    }

    private void HideAllItems()
    {
      foreach (WeekEventModel key in this._showItemsDict.Keys.ToList<WeekEventModel>())
      {
        TaskBar model;
        if (this._showItemsDict.TryRemove(key, out model))
        {
          model.DataContext = (object) null;
          model.Visibility = Visibility.Hidden;
          this._hideItems.Add(model);
        }
      }
    }

    private void HideItem(WeekEventModel model)
    {
      TaskBar model1;
      if (model == null || !this._showItemsDict.TryRemove(model, out model1))
        return;
      model1.DataContext = (object) null;
      model1.Visibility = Visibility.Hidden;
      this._hideItems.Add(model1);
    }

    private async Task CheckAndSetAllCellVisible()
    {
      if (this._itemsSource == null)
        return;
      long lastCheckTime = DateTime.Now.Ticks;
      this._lastCheckTime = lastCheckTime;
      await this._checkLocker.WaitAsync();
      try
      {
        if (lastCheckTime != this._lastCheckTime)
          return;
        foreach (WeekEventModel weekEventModel in this._itemsSource)
        {
          if (this.CheckModelVisible(weekEventModel))
          {
            if (!this._showItemsDict.ContainsKey(weekEventModel))
              this.ShowItem(weekEventModel);
          }
          else
            this.HideItem(weekEventModel);
        }
      }
      finally
      {
        this._checkLocker.Release();
      }
    }

    private bool CheckModelVisible(WeekEventModel model)
    {
      return (double) MultiDayAllDayItems._initOffset + (double) model.Column * this._dayWidth <= this.ViewWidth + this.HOffset + 20.0 && (double) MultiDayAllDayItems._initOffset + (double) (model.Column + model.ColumnSpan) * this._dayWidth >= this.HOffset - 20.0 && (double) (model.Row * 18) <= this.ViewHeight + this.VOffset + 30.0 && (double) (model.Row * 18) >= this.VOffset - 30.0;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      this.CheckAndSetAllCellVisible();
    }

    public void SetDayWidth(double dayWidth) => this._dayWidth = dayWidth;

    public void SetSize(Size size)
    {
      this.ViewHeight = size.Height;
      this.ViewWidth = size.Width;
    }

    public void SetScrollOffset(double vOffset, bool checkVisible = false)
    {
      this.HOffset = (double) MultiDayAllDayItems._initOffset - (this.RenderTransform is TranslateTransform renderTransform ? renderTransform.X : 0.0);
      this.VOffset = vOffset;
      if (!checkVisible)
        return;
      this.CheckAndSetAllCellVisible();
    }

    public void Clear()
    {
      this.Children.Clear();
      this._hideItems.Clear();
      this._showItemsDict.Clear();
    }
  }
}
