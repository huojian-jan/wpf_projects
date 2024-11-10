// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SelectDateDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SelectDateDialog : UserControl, IComponentConnector
  {
    private bool _isStart;
    private DurationModel _model;
    private readonly Popup _popup;
    private TickDatePicker _calendar;
    internal Grid CalendarGrid;
    private bool _contentLoaded;

    public bool IsOpen => this._popup.IsOpen;

    public SelectDateDialog()
    {
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = true;
      escPopup.Placement = PlacementMode.MousePoint;
      escPopup.VerticalOffset = -20.0;
      escPopup.HorizontalOffset = -20.0;
      // ISSUE: explicit constructor call
      this.\u002Ector((Popup) escPopup);
    }

    private SelectDateDialog(Popup popup)
    {
      Popup popup1 = popup;
      DurationModel model = new DurationModel();
      model.SelectedDate = new DateTime?(DateTime.Now.Date);
      DateTime? maxDate = new DateTime?();
      DateTime? minDate = new DateTime?();
      // ISSUE: explicit constructor call
      this.\u002Ector(popup1, model, true, maxDate, minDate);
    }

    public SelectDateDialog(
      Popup popup,
      DurationModel model,
      bool isStart,
      DateTime? maxDate = null,
      DateTime? minDate = null,
      bool isChooseDate = false)
    {
      this._popup = popup;
      this._model = model;
      this._isStart = isStart;
      this.InitializeComponent();
      this.InitCalendar(maxDate, isStart ? new DateTime?() : minDate, isChooseDate);
    }

    public DateTime? SelectStart => this._calendar.SelectStart;

    public DateTime? SelectEnd => this._calendar.SelectEnd;

    public event EventHandler<DateTime> SelectDate;

    public event EventHandler<DateTime> SelectDateAndSave;

    private void InitCalendar(DateTime? maxDate = null, DateTime? minDate = null, bool isChooseDate = false)
    {
      this._calendar = new TickDatePicker(new DateTime?(this._model.SelectedDate ?? DateTime.Now.Date), this._model.SelectionStart, this._model.SelectionEnd, this._isStart, maxDate: maxDate, minDate: minDate)
      {
        IsChooseDate = isChooseDate
      };
      this.CalendarGrid.Children.Add((UIElement) this._calendar);
      this._calendar.SelectedDateChanged -= new EventHandler<DateTime>(this.OnDateSelect);
      this._calendar.SelectedDateChanged += new EventHandler<DateTime>(this.OnDateSelect);
      this._calendar.DateSelected -= new EventHandler<DateTime>(this.OnDateSelectAndSave);
      this._calendar.DateSelected += new EventHandler<DateTime>(this.OnDateSelectAndSave);
    }

    private void OnDateSelectAndSave(object sender, DateTime selectedDate)
    {
      this._popup.IsOpen = false;
      EventHandler<DateTime> selectDateAndSave = this.SelectDateAndSave;
      if (selectDateAndSave == null)
        return;
      selectDateAndSave((object) this, selectedDate);
    }

    private void OnDateSelect(object sender, DateTime selectedDate)
    {
      this._popup.IsOpen = false;
      EventHandler<DateTime> selectDate = this.SelectDate;
      if (selectDate == null)
        return;
      selectDate((object) this, selectedDate);
    }

    public void Show()
    {
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
      this._popup.Focus();
    }

    public void SetData(DurationModel model, bool isStart, DateTime? maxDate = null, DateTime? minDate = null)
    {
      this._model = model;
      this._isStart = isStart;
      if (this._calendar == null)
        this.InitCalendar(maxDate, minDate);
      else
        this._calendar.SetData(new DateTime?(this._model.SelectedDate ?? DateTime.Now.Date), this._model.SelectionStart, this._model.SelectionEnd, this._isStart, maxDate: maxDate, minDate: minDate);
    }

    public void HandleLeftRight(bool isLeft) => this._calendar?.MoveTabSelectDate(isLeft ? -1 : 1);

    public void HandleUpDown(bool isUp) => this._calendar?.MoveTabSelectDate(isUp ? -7 : 7);

    public void SetCurrentTab(bool inTab)
    {
      if (inTab)
      {
        this._calendar?.ClearTabSelected();
        this._calendar?.TabSelectCurrent();
      }
      else
        this._calendar?.ClearTabSelected();
    }

    public void EnterSelect() => this._calendar?.SelectTabItem();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/selectdatedialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.CalendarGrid = (Grid) target;
      else
        this._contentLoaded = true;
    }
  }
}
