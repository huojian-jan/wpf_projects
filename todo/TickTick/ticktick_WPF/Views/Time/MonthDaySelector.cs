// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.MonthDaySelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class MonthDaySelector : UserControl, IComponentConnector
  {
    public List<int> SelectedDays = new List<int>();
    private int _tabIndex;
    private DayCellSelector _tabSelectItem;
    internal MonthDaySelector RootView;
    internal Grid CellsGrid;
    private bool _contentLoaded;

    public MonthDaySelector()
    {
      this.InitializeComponent();
      this.InitCells();
    }

    public void NotifySelectedChanged()
    {
      foreach (object child in this.CellsGrid.Children)
      {
        if (child is DayCellSelector dayCellSelector)
          dayCellSelector.ItemSelected = this.SelectedDays.Contains(dayCellSelector.DayOfMonth);
      }
    }

    private void InitCells()
    {
      for (int index1 = 0; index1 < 5; ++index1)
      {
        for (int index2 = 0; index2 < 7; ++index2)
        {
          int num = index1 * 7 + index2 + 1;
          if (num <= 31)
          {
            DayCellSelector element = new DayCellSelector()
            {
              DayOfMonth = num,
              MonthMode = true,
              ItemSelected = this.SelectedDays.Contains(num)
            };
            element.SelectedChanged -= new EventHandler<bool>(this.OnCellSelected);
            element.SelectedChanged += new EventHandler<bool>(this.OnCellSelected);
            this.CellsGrid.Children.Add((UIElement) element);
            Grid.SetRow((UIElement) element, index1);
            Grid.SetColumn((UIElement) element, index2);
          }
        }
      }
      DayCellSelector element1 = new DayCellSelector(66.0)
      {
        DayOfMonth = -1,
        MonthMode = true,
        ItemSelected = this.SelectedDays.Contains(-1)
      };
      element1.SelectedChanged -= new EventHandler<bool>(this.OnCellSelected);
      element1.SelectedChanged += new EventHandler<bool>(this.OnCellSelected);
      element1.HorizontalAlignment = HorizontalAlignment.Left;
      this.CellsGrid.Children.Add((UIElement) element1);
      Grid.SetRow((UIElement) element1, 4);
      Grid.SetColumn((UIElement) element1, 3);
      Grid.SetColumnSpan((UIElement) element1, 4);
    }

    private void OnCellSelected(object sender, bool selected)
    {
      DayCellSelector dayCellSelector = (DayCellSelector) sender;
      if (selected)
        this.SelectedDays.Add(dayCellSelector.DayOfMonth);
      else
        this.SelectedDays.Remove(dayCellSelector.DayOfMonth);
    }

    public void SetTabSelected(bool tab, int? step = null)
    {
      if (tab)
      {
        if (step.HasValue)
        {
          int num = this._tabIndex + step.Value;
          if (num < 0 || num != 27 && num >= this.CellsGrid.Children.Count)
            return;
          int index = Math.Min(this._tabIndex + step.Value, this.CellsGrid.Children.Count - 1);
          if (!(this.CellsGrid.Children[index] is DayCellSelector child))
            return;
          this._tabSelectItem?.SetTabSelected(false);
          this._tabSelectItem = child;
          this._tabIndex = index;
          child.SetTabSelected(true);
        }
        else
        {
          this._tabIndex = 0;
          this._tabSelectItem?.SetTabSelected(false);
          this._tabSelectItem = this.CellsGrid.Children[this._tabIndex] as DayCellSelector;
          this._tabSelectItem?.SetTabSelected(true);
        }
      }
      else
      {
        this._tabIndex = -1;
        this._tabSelectItem?.SetTabSelected(false);
        this._tabSelectItem = (DayCellSelector) null;
      }
    }

    public void HandleEnter() => this._tabSelectItem?.Select();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/monthdayselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.CellsGrid = (Grid) target;
        else
          this._contentLoaded = true;
      }
      else
        this.RootView = (MonthDaySelector) target;
    }
  }
}
