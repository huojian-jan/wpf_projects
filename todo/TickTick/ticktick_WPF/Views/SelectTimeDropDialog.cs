// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SelectTimeDropDialog
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SelectTimeDropDialog : UserControl, IComponentConnector
  {
    private readonly int _hour;
    private readonly int _minute;
    private DateTime _date;
    private TimeZoneSelectControl _tz;
    private DateTime? _beginTime;
    internal ListView ReminderListView;
    internal Grid TimeZoneGrid;
    private bool _contentLoaded;

    public event EventHandler<TimeZoneViewModel> TimeZoneChanged;

    public SelectTimeDropDialog()
    {
      this.InitializeComponent();
      DateTime now = DateTime.Now;
      this._date = now;
      this._hour = now.Hour;
      this._minute = now.Minute;
      this.TimeZoneGrid.Visibility = Visibility.Collapsed;
      this.Loaded += (RoutedEventHandler) ((sender, e) => this.InitView());
    }

    public SelectTimeDropDialog(
      DateTime? date,
      bool showTimeZone,
      bool enableEditTimeZone,
      TimeZoneViewModel tzModel = null,
      DateTime? beginTime = null)
    {
      this.InitializeComponent();
      DateTime dateTime = date ?? DateTime.Now;
      this._date = dateTime;
      this._beginTime = beginTime;
      this._hour = dateTime.Hour;
      this._minute = dateTime.Minute;
      this.InitTimeZoneControl(showTimeZone, tzModel, enableEditTimeZone);
      this.Loaded += (RoutedEventHandler) ((sender, e) => this.InitView());
    }

    private void InitTimeZoneControl(
      bool showTimeZone,
      TimeZoneViewModel tzModel,
      bool enableEditTimeZone)
    {
      if (showTimeZone)
      {
        if (!enableEditTimeZone && tzModel.TimeZone.Equals(TimeZoneInfo.Local) && !tzModel.IsFloat)
        {
          this._tz = (TimeZoneSelectControl) null;
          this.TimeZoneGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
          TimeZoneSelectControl zoneSelectControl = new TimeZoneSelectControl(tzModel);
          zoneSelectControl.HorizontalAlignment = HorizontalAlignment.Stretch;
          zoneSelectControl.Height = 24.0;
          zoneSelectControl.Margin = new Thickness(16.0, 3.0, 16.0, 3.0);
          zoneSelectControl.VerticalAlignment = VerticalAlignment.Center;
          this._tz = zoneSelectControl;
          this._tz.OnTimeZoneChanged += (EventHandler<TimeZoneViewModel>) ((obj, model) =>
          {
            EventHandler<TimeZoneViewModel> timeZoneChanged = this.TimeZoneChanged;
            if (timeZoneChanged == null)
              return;
            timeZoneChanged(obj, model);
          });
          this._tz.IsEnabled = enableEditTimeZone;
          this._tz.Opacity = enableEditTimeZone ? 1.0 : 0.56;
          this._tz.DropArrow.Visibility = enableEditTimeZone ? Visibility.Visible : Visibility.Collapsed;
          this._tz.SetDropContentWidth(266.0);
          this.TimeZoneGrid.Children.Clear();
          this.TimeZoneGrid.Children.Add((UIElement) this._tz);
        }
      }
      else
      {
        this._tz = (TimeZoneSelectControl) null;
        this.TimeZoneGrid.Visibility = Visibility.Collapsed;
      }
    }

    public event EventHandler<DateTime> Select;

    private void InitView()
    {
      this.ReminderListView.ItemsSource = (IEnumerable) SelectTimeDropDialog.InitPickerItems(this._date, this._beginTime);
      this.ScrollToSelectedItem();
      this.ReminderListView.Height = 220.0;
    }

    private async void ScrollToSelectedItem()
    {
      DateTime dateTime = new DateTime(this._date.Year, this._date.Month, this._date.Day, this._hour, this._minute, 0);
      for (int index = 0; index < this.ReminderListView.Items.Count; ++index)
      {
        if (this.ReminderListView.Items[index] is TimeViewModel timeViewModel)
        {
          double totalMinutes = (timeViewModel.Value - dateTime).TotalMinutes;
          if (Math.Abs(totalMinutes) <= 0.0)
          {
            timeViewModel.Selected = true;
            timeViewModel.Suggested = false;
            if (!(this.ReminderListView.Template.FindName("ScrollViewer", (FrameworkElement) this.ReminderListView) is ScrollViewer name))
              break;
            name.ScrollToVerticalOffset((double) (index * 32));
            await Task.Delay(300);
            break;
          }
          if (totalMinutes > 0.0 && totalMinutes < 30.0)
          {
            timeViewModel.Suggested = true;
            timeViewModel.Selected = false;
            if (!(this.ReminderListView.Template.FindName("ScrollViewer", (FrameworkElement) this.ReminderListView) is ScrollViewer name))
              break;
            name.ScrollToVerticalOffset((double) (index * 32));
            break;
          }
          timeViewModel.Suggested = false;
          timeViewModel.Selected = false;
        }
      }
    }

    private static IEnumerable<TimeViewModel> InitPickerItems(DateTime date, DateTime? beginTime)
    {
      List<TimeViewModel> timeViewModelList = new List<TimeViewModel>();
      DateTime dateTime1 = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
      if (!beginTime.HasValue || beginTime.Value.Date != date.Date)
      {
        for (int index = 0; index < 48; ++index)
        {
          DateTime dateTime2 = dateTime1;
          TimeViewModel timeViewModel = new TimeViewModel()
          {
            DisplayText = dateTime2.ToString(DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour12 ? "hh:mm  tt" : "HH:mm", (IFormatProvider) new CultureInfo("en-US")),
            Value = dateTime1
          };
          timeViewModelList.Add(timeViewModel);
          dateTime1 = dateTime1.AddMinutes(30.0);
        }
      }
      else
      {
        DateTime dateTime3 = beginTime.Value;
        for (int index = 0; index < 49; ++index)
        {
          dateTime3 = dateTime3.AddMinutes(index < 4 ? 15.0 : 30.0);
          int totalMinutes = (int) (dateTime3 - beginTime.Value).TotalMinutes;
          TimeViewModel timeViewModel = new TimeViewModel()
          {
            DisplayText = dateTime3.ToString(DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour12 ? "hh:mm  tt" : "HH:mm", (IFormatProvider) new CultureInfo("en-US")),
            Value = dateTime3,
            TimeSpanText = totalMinutes >= 60 ? (totalMinutes % 60 == 0 ? (totalMinutes / 60).ToString() + " " : ((float) totalMinutes / 60f).ToString() + " ") + Utils.GetString(totalMinutes > 60 ? "PublicHours" : "PublicHour") : totalMinutes.ToString() + " " + Utils.GetString("PublicMinutes"),
            ShowSpanText = true
          };
          timeViewModelList.Add(timeViewModel);
        }
      }
      return (IEnumerable<TimeViewModel>) timeViewModelList;
    }

    private void OnReminderClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is ListView listView1 && listView1.SelectedItem == null)
        return;
      foreach (object obj in (IEnumerable) this.ReminderListView.Items)
      {
        if (obj is TimeViewModel timeViewModel)
          timeViewModel.Selected = false;
      }
      if (!(sender is ListView listView2))
        return;
      TimeViewModel selectedItem = (TimeViewModel) listView2.SelectedItem;
      selectedItem.Selected = true;
      EventHandler<DateTime> select = this.Select;
      if (select == null)
        return;
      select((object) this, selectedItem.Value);
    }

    public bool TryCloseTimezonePopup()
    {
      TimeZoneSelectControl tz = this._tz;
      return tz != null && tz.ClosePopup();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/selecttimedropdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.TimeZoneGrid = (Grid) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.ReminderListView = (ListView) target;
        this.ReminderListView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnReminderClick);
      }
    }
  }
}
