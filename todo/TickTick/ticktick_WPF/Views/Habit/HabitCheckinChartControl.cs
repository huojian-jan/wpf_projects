// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitCheckinChartControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitCheckinChartControl : UserControl, INotifyPropertyChanged, IComponentConnector
  {
    private bool _hitEnd;
    private string _unit;
    internal ItemsControl AxisY;
    internal Grid IndicatorLine;
    internal TextBlock GoalText;
    internal ScrollViewer MonthScrollViewer;
    internal ItemsControl AxisX;
    internal Border LeftButton;
    internal Border RightButton;
    private bool _contentLoaded;

    public HabitCheckinChartControl() => this.InitializeComponent();

    public bool HitEnd
    {
      get => this._hitEnd;
      set
      {
        this._hitEnd = value;
        this.OnPropertyChanged(nameof (HitEnd));
      }
    }

    public string Unit
    {
      get => this._unit;
      set
      {
        this._unit = value;
        this.OnPropertyChanged(nameof (Unit));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Load(
      HabitModel habit,
      DateTime startDate,
      DateTime endDate,
      List<HabitCheckInModel> checkIns)
    {
      int val1 = 0;
      HabitCheckInModel habitCheckInModel1 = checkIns.OrderByDescending<HabitCheckInModel, double>((Func<HabitCheckInModel, double>) (checkIn => checkIn.Value)).FirstOrDefault<HabitCheckInModel>();
      if (habitCheckInModel1 != null)
        val1 = (int) Math.Round(habitCheckInModel1.Value, 1);
      int num1 = (int) Math.Round(Math.Max((double) val1, habit.Goal), 0);
      List<OffsetInfo> offsetInfoList = new List<OffsetInfo>();
      int num2 = Math.Max((int) Math.Round((double) num1 / 5.0, 0), 1);
      int num3 = num2 * 5;
      if (num3 < val1)
      {
        ++num2;
        num3 = num2 * 5;
      }
      for (int index = 0; index <= 5; ++index)
        offsetInfoList.Add(new OffsetInfo((double) (num3 - num2 * index), 24.0));
      this.AxisY.ItemsSource = (IEnumerable) offsetInfoList;
      double bottom = habit.Goal / (double) num2 * 24.0 + 28.0;
      this.GoalText.Text = habit.Goal.ToString() ?? "";
      this.IndicatorLine.Margin = new Thickness(0.0, 0.0, 0.0, bottom);
      List<DayCheckInColumnInfo> checkInColumnInfoList = new List<DayCheckInColumnInfo>();
      for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1.0))
      {
        double height = 0.0;
        HabitCheckInModel habitCheckInModel2 = checkIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (check => check.CheckinStamp == currentDate.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
        if (habitCheckInModel2 != null)
          height = habitCheckInModel2.Value / (double) num2 * 24.0;
        checkInColumnInfoList.Add(new DayCheckInColumnInfo(currentDate, height, habitCheckInModel2 != null ? habitCheckInModel2.Value : 0.0, habitCheckInModel2 != null ? habitCheckInModel2.Goal : habit.Goal, habit.Unit));
      }
      this.AxisX.ItemsSource = (IEnumerable) checkInColumnInfoList;
      this.MoveToToday();
      this.MonthScrollViewer.ScrollChanged -= new ScrollChangedEventHandler(this.OnScrollChanged);
      this.MonthScrollViewer.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
      this.Unit = HabitUtils.GetUnitText(habit.Unit);
      this.DataContext = (object) this;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      ScrollViewer scrollViewer = (ScrollViewer) sender;
      this.HitEnd = Math.Abs(scrollViewer.HorizontalOffset - scrollViewer.ScrollableWidth) <= 0.0;
    }

    private void MoveToToday()
    {
      int num = DateTime.Today.Day * 32;
      if ((double) num <= this.Width)
        return;
      this.MonthScrollViewer.ScrollToHorizontalOffset((double) num - this.Width + this.Width * 0.6);
    }

    private void MoveRightClick(object sender, MouseButtonEventArgs e)
    {
      this.MonthScrollViewer.ScrollToHorizontalOffset(this.MonthScrollViewer.HorizontalOffset + this.Width * 0.6);
    }

    private void MoveLeftClick(object sender, MouseButtonEventArgs e)
    {
      this.MonthScrollViewer.ScrollToHorizontalOffset(this.MonthScrollViewer.HorizontalOffset - this.Width * 0.6);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.MonthScrollViewer.ScrollToHorizontalOffset(this.MonthScrollViewer.HorizontalOffset - (double) e.Delta / 3.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitcheckinchartcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.AxisY = (ItemsControl) target;
          break;
        case 2:
          this.IndicatorLine = (Grid) target;
          break;
        case 3:
          this.GoalText = (TextBlock) target;
          break;
        case 4:
          this.MonthScrollViewer = (ScrollViewer) target;
          this.MonthScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseWheel);
          break;
        case 5:
          this.AxisX = (ItemsControl) target;
          break;
        case 6:
          this.LeftButton = (Border) target;
          this.LeftButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveLeftClick);
          break;
        case 7:
          this.RightButton = (Border) target;
          this.RightButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveRightClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
