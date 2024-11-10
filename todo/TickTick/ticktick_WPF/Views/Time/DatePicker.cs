// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DatePicker
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
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DatePicker : UserControl, IComponentConnector
  {
    internal TextBlock DateText;
    internal EscPopup Popup;
    internal TickDatePicker Calendar;
    private bool _contentLoaded;

    public DateTime? SelectedDate
    {
      get => this.Calendar.SelectedDate;
      set
      {
        this.Calendar.SelectedDate = value;
        if (!value.HasValue)
          return;
        this.DateText.Text = value.Value.ToString("yyyy/MM/dd");
      }
    }

    public DatePicker()
      : this(DateTime.Today)
    {
    }

    public DatePicker(DateTime selectedDate)
    {
      this.InitializeComponent();
      this.Calendar.SelectedDate = new DateTime?(selectedDate);
      this.Calendar.SelectedDateChanged += (EventHandler<DateTime>) ((sender, date) =>
      {
        this.Popup.IsOpen = false;
        this.DateText.Text = date.ToString("yyyy/MM/dd");
      });
    }

    private void OnTextClick(object sender, MouseButtonEventArgs e)
    {
      this.Popup.IsOpen = !this.Popup.IsOpen;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/datepicker.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTextClick);
          break;
        case 2:
          this.DateText = (TextBlock) target;
          break;
        case 3:
          this.Popup = (EscPopup) target;
          break;
        case 4:
          this.Calendar = (TickDatePicker) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
