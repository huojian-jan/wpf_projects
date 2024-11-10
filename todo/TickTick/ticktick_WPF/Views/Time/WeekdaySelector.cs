// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.WeekdaySelector
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
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class WeekdaySelector : UserControl, IComponentConnector
  {
    public List<DayOfWeek> SelectedDays = new List<DayOfWeek>();
    public static readonly DependencyProperty AtLeastOneProperty = DependencyProperty.Register(nameof (AtLeastOne), typeof (bool), typeof (DayCellSelector), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    internal Grid CellsGrid;
    private bool _contentLoaded;

    public event EventHandler SelectedDaysChanged;

    public bool AtLeastOne
    {
      get => (bool) this.GetValue(WeekdaySelector.AtLeastOneProperty);
      set => this.SetValue(WeekdaySelector.AtLeastOneProperty, (object) value);
    }

    public WeekdaySelector()
    {
      this.InitializeComponent();
      this.InitCells();
    }

    private void InitCells()
    {
      int num = 0;
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Monday":
          num = 1;
          break;
        case "Saturday":
          num = 6;
          break;
      }
      for (int index = 0; index < 7; ++index)
      {
        DayOfWeek dayOfWeek = (DayOfWeek) Enum.ToObject(typeof (DayOfWeek), (index + num) % 7);
        DayCellSelector element = new DayCellSelector()
        {
          Day = dayOfWeek,
          ItemSelected = this.SelectedDays.Contains(dayOfWeek)
        };
        element.SelectedChanged -= new EventHandler<bool>(this.OnCellSelectedChanged);
        element.SelectedChanged += new EventHandler<bool>(this.OnCellSelectedChanged);
        this.CellsGrid.Children.Add((UIElement) element);
        Grid.SetColumn((UIElement) element, index);
      }
    }

    public void NotifySelectedChanged()
    {
      if (this.SelectedDays == null)
        this.SelectedDays = new List<DayOfWeek>();
      foreach (DayCellSelector child in this.CellsGrid.Children)
        child.ItemSelected = this.SelectedDays.Contains(child.Day);
    }

    private void OnCellSelectedChanged(object sender, bool selected)
    {
      DayCellSelector dayCellSelector = (DayCellSelector) sender;
      if (dayCellSelector == null)
        return;
      if (selected)
      {
        this.SelectedDays.Add(dayCellSelector.Day);
        EventHandler selectedDaysChanged = this.SelectedDaysChanged;
        if (selectedDaysChanged == null)
          return;
        selectedDaysChanged((object) this, (EventArgs) null);
      }
      else if (this.AtLeastOne && this.SelectedDays.Count <= 1)
      {
        dayCellSelector.ItemSelected = true;
      }
      else
      {
        this.SelectedDays.Remove(dayCellSelector.Day);
        EventHandler selectedDaysChanged = this.SelectedDaysChanged;
        if (selectedDaysChanged == null)
          return;
        selectedDaysChanged((object) this, (EventArgs) null);
      }
    }

    public void SetTabIndex(int tabIndex)
    {
      for (int index = 0; index < this.CellsGrid.Children.Count; ++index)
        ((DayCellSelector) this.CellsGrid.Children[index]).SetTabSelected(tabIndex == index);
    }

    public void HandleEnter()
    {
      for (int index = 0; index < this.CellsGrid.Children.Count; ++index)
      {
        DayCellSelector child = (DayCellSelector) this.CellsGrid.Children[index];
        if (child.TabSelected)
          child.Select();
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/weekdayselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.CellsGrid = (Grid) target;
      else
        this._contentLoaded = true;
    }
  }
}
