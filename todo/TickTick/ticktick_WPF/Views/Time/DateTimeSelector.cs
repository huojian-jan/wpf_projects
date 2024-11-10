// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DateTimeSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DateTimeSelector : ContentControl, IComponentConnector
  {
    private DateTime _time;
    internal TickDatePicker Calendar;
    internal TimeInputControl TimePointControl;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    public event EventHandler<DateTime> TimeSelected;

    public event EventHandler Cancel;

    public DateTimeSelector(DateTime time, DateTime? maxDate = null, DateTime? minDate = null)
    {
      this.InitializeComponent();
      this._time = time;
      TickDatePicker calendar = this.Calendar;
      DateTime? selectedDate = new DateTime?(time);
      DateTime? nullable1 = maxDate;
      DateTime? nullable2 = minDate;
      DateTime? selectStart = new DateTime?();
      DateTime? selectEnd = new DateTime?();
      DateTime? maxDate1 = nullable1;
      DateTime? minDate1 = nullable2;
      calendar.SetData(selectedDate, selectStart, selectEnd, maxDate: maxDate1, minDate: minDate1);
      this.TimePointControl.SelectedTime = time;
      this.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape || !this.TimePointControl.ReminderPopup.IsOpen)
        return;
      this.TimePointControl.HidePopup();
      e.Handled = true;
    }

    private void OnSelectedDateChanged(object sender, DateTime e)
    {
      this._time = DateUtils.SetDateOnly(this._time, e);
    }

    private void OnTimeChanged(object sender, DateTime e)
    {
      this._time = DateUtils.SetHourAndMinuteOnly(this._time, e.Hour, e.Minute);
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel == null)
        return;
      cancel((object) this, (EventArgs) null);
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
      DateTime? time1 = this.TimePointControl.GetTime();
      if (time1.HasValue)
      {
        DateTime time2 = this._time;
        DateTime dateTime = time1.Value;
        int hour = dateTime.Hour;
        dateTime = time1.Value;
        int minute = dateTime.Minute;
        this._time = DateUtils.SetHourAndMinuteOnly(time2, hour, minute);
      }
      EventHandler<DateTime> timeSelected = this.TimeSelected;
      if (timeSelected == null)
        return;
      timeSelected((object) this, this._time);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/datetimeselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Calendar = (TickDatePicker) target;
          break;
        case 2:
          this.TimePointControl = (TimeInputControl) target;
          break;
        case 3:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveClick);
          break;
        case 4:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.CancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
