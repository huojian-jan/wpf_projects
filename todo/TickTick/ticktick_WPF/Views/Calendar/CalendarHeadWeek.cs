// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarHeadWeek
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
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarHeadWeek : UserControl, IComponentConnector
  {
    public string DefaultViewType = "0";
    private string _currentViewType;
    private bool _loaded;
    private CalendarControl _calendarControl;
    private List<CalendarHeadWeekModel> _models = new List<CalendarHeadWeekModel>();
    internal Grid CalendarHeadWeekGrid;
    internal ColumnDefinition Column6;
    internal ColumnDefinition Column7;
    internal TextBlock Mon;
    internal TextBlock Tue;
    internal TextBlock Wed;
    internal TextBlock Thu;
    internal TextBlock Fri;
    internal TextBlock Sat;
    internal TextBlock Sun;
    internal TextBlock DayMode;
    private bool _contentLoaded;

    public CalendarHeadWeek()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      DateTime dateTime = DateTime.Today;
      while (dateTime.DayOfWeek != DayOfWeek.Monday)
        dateTime = dateTime.AddDays(1.0);
      this.Mon.Text = dateTime.ToString("ddd", (IFormatProvider) App.Ci);
      this.Tue.Text = dateTime.AddDays(1.0).ToString("ddd", (IFormatProvider) App.Ci);
      this.Wed.Text = dateTime.AddDays(2.0).ToString("ddd", (IFormatProvider) App.Ci);
      this.Thu.Text = dateTime.AddDays(3.0).ToString("ddd", (IFormatProvider) App.Ci);
      this.Fri.Text = dateTime.AddDays(4.0).ToString("ddd", (IFormatProvider) App.Ci);
      this.Sat.Text = dateTime.AddDays(5.0).ToString("ddd", (IFormatProvider) App.Ci);
      this.Sun.Text = dateTime.AddDays(6.0).ToString("ddd", (IFormatProvider) App.Ci);
      this._loaded = true;
      this.OnSwitchView(string.Empty, this.DefaultViewType);
    }

    public void OnSwitchView(string from, string to)
    {
      this._currentViewType = to;
      this.UpdateViewModel();
    }

    public void UpdateViewModel()
    {
      if (!this._loaded)
        return;
      string currentViewType = this._currentViewType;
      this.Mon.Visibility = Visibility.Visible;
      this.Mon.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(1));
      this.Tue.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(2));
      this.Wed.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(3));
      this.Thu.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(4));
      this.Fri.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(5));
      this.Sat.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(6));
      this.Sun.SetValue(Grid.ColumnProperty, (object) this.GetWeekDayColumn(7));
      this.DayMode.Visibility = Visibility.Collapsed;
      this.Column6.Width = LocalSettings.Settings.ShowCalWeekend ? new GridLength(1.0, GridUnitType.Star) : new GridLength(0.0);
      this.Column7.Width = LocalSettings.Settings.ShowCalWeekend ? new GridLength(1.0, GridUnitType.Star) : new GridLength(0.0);
      this.CalendarHeadWeekGrid.Margin = new Thickness(this._currentViewType == "0" ? 0.0 : 65.0, 0.0, 0.0, 0.0);
    }

    private int GetWeekDayColumn(int dayOfWeek)
    {
      string str = LocalSettings.Settings.WeekStartFrom;
      if (!LocalSettings.Settings.ShowCalWeekend)
        str = "Monday";
      switch (str)
      {
        case "Saturday":
          ++dayOfWeek;
          break;
        case "Monday":
          dayOfWeek += 6;
          break;
      }
      return dayOfWeek % 7;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendarheadweek.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.CalendarHeadWeekGrid = (Grid) target;
          break;
        case 2:
          this.Column6 = (ColumnDefinition) target;
          break;
        case 3:
          this.Column7 = (ColumnDefinition) target;
          break;
        case 4:
          this.Mon = (TextBlock) target;
          break;
        case 5:
          this.Tue = (TextBlock) target;
          break;
        case 6:
          this.Wed = (TextBlock) target;
          break;
        case 7:
          this.Thu = (TextBlock) target;
          break;
        case 8:
          this.Fri = (TextBlock) target;
          break;
        case 9:
          this.Sat = (TextBlock) target;
          break;
        case 10:
          this.Sun = (TextBlock) target;
          break;
        case 11:
          this.DayMode = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
