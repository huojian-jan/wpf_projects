// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.TimeInputControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class TimeInputControl : UserControl, IComponentConnector
  {
    public new static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof (Foreground), typeof (Brush), typeof (TimeInputControl), new PropertyMetadata((object) new SolidColorBrush(Colors.Black), new PropertyChangedCallback(TimeInputControl.OnForegroundChangedCallback)));
    public static readonly DependencyProperty ShowTimeZoneProperty = DependencyProperty.Register(nameof (ShowTimeZone), typeof (bool), typeof (TimeInputControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty DropWidthProperty = DependencyProperty.Register(nameof (DropWidth), typeof (double), typeof (TimeInputControl), new PropertyMetadata((object) 110.0, (PropertyChangedCallback) null));
    public static readonly DependencyProperty TabSelectedProperty = DependencyProperty.Register(nameof (TabSelected), typeof (bool), typeof (TimeInputControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty BorderThickProperty = DependencyProperty.Register(nameof (BorderThick), typeof (Thickness), typeof (TimeInputControl), new PropertyMetadata((object) new Thickness(1.0), (PropertyChangedCallback) null));
    public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register(nameof (PopupOpen), typeof (bool), typeof (TimeInputControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    private SelectTimeDropDialog _droplist;
    private DateTime? _lastToggleTime;
    private DateTime _selectedTime = DateTime.Today;
    private bool _showPopupMouseDown;
    internal TimeInputControl Root;
    internal TextBox EmptyBox;
    internal Border InputGrid;
    internal TextBox HourText;
    internal TextBlock SplitText;
    internal TextBox MinuteText;
    internal TextBox AmOrPmText;
    internal Popup ReminderPopup;
    private bool _contentLoaded;

    public bool PopupOpen
    {
      get => (bool) this.GetValue(TimeInputControl.PopupOpenProperty);
      set => this.SetValue(TimeInputControl.PopupOpenProperty, (object) value);
    }

    public TimeInputControl()
    {
      this.InitializeComponent();
      this.Cursor = Cursors.Hand;
      this.InputGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnLeftMouseDown);
      ticktick_WPF.Notifier.GlobalEventManager.TimeFormatChanged += new EventHandler(this.OnTimeFormatChanged);
    }

    private void OnTimeFormatChanged(object sender, EventArgs e)
    {
      this.AmOrPmText.Visibility = DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour24 ? Visibility.Collapsed : Visibility.Visible;
    }

    public Thickness BorderThick
    {
      get => (Thickness) this.GetValue(TimeInputControl.BorderThickProperty);
      set => this.SetValue(TimeInputControl.BorderThickProperty, (object) value);
    }

    public bool ShowTimeZone
    {
      get => (bool) this.GetValue(TimeInputControl.ShowTimeZoneProperty);
      set => this.SetValue(TimeInputControl.ShowTimeZoneProperty, (object) value);
    }

    public bool TabSelected
    {
      get => (bool) this.GetValue(TimeInputControl.TabSelectedProperty);
      set => this.SetValue(TimeInputControl.TabSelectedProperty, (object) value);
    }

    public double DropWidth
    {
      get => (double) this.GetValue(TimeInputControl.DropWidthProperty);
      set => this.SetValue(TimeInputControl.DropWidthProperty, (object) value);
    }

    public bool EnableEditTimeZone { get; set; } = true;

    public TimeZoneViewModel TimeZone { get; set; }

    public DateTime SelectedTime
    {
      get => this._selectedTime;
      set
      {
        this._selectedTime = value;
        this.SetTimeText(this._selectedTime);
      }
    }

    public new Brush Foreground
    {
      get => (Brush) this.GetValue(TimeInputControl.ForegroundProperty);
      set => this.SetValue(TimeInputControl.ForegroundProperty, (object) value);
    }

    public Popup DropdownPopup => this.ReminderPopup;

    public DateTime? BeginTime { get; set; }

    public event EventHandler<TimeZoneViewModel> TimeZoneChanged;

    private void OnInputLoaded(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is DateTime dataContext)
        this.SelectedTime = dataContext;
      this.InitEvents();
      this.Loaded -= new RoutedEventHandler(this.OnInputLoaded);
    }

    private void InitEvents()
    {
      this.HourText.TextChanged -= new TextChangedEventHandler(this.HourTextChanged);
      this.HourText.TextChanged += new TextChangedEventHandler(this.HourTextChanged);
      this.MinuteText.TextChanged -= new TextChangedEventHandler(this.MinuteTextChanged);
      this.MinuteText.TextChanged += new TextChangedEventHandler(this.MinuteTextChanged);
    }

    public event EventHandler<DateTime> SelectedTimeChanged;

    private static void OnForegroundChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is TimeInputControl timeInputControl) || e.NewValue == null)
        return;
      timeInputControl.HourText.Foreground = (Brush) e.NewValue;
      timeInputControl.SplitText.Foreground = (Brush) e.NewValue;
      timeInputControl.MinuteText.Foreground = (Brush) e.NewValue;
      timeInputControl.AmOrPmText.Foreground = (Brush) e.NewValue;
    }

    private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (PopupStateManager.CanOpenTimePopup)
        this._showPopupMouseDown = true;
      else
        PopupStateManager.SetCanOpenTimePopup(true);
    }

    private async void OnClick(object sender, MouseButtonEventArgs e)
    {
      await Task.Delay(50);
      if (!this.AmOrPmText.IsMouseOver)
      {
        this.ToggleReminderPopup();
        if (this.MinuteText.IsFocused)
        {
          this.MinuteText.Focus();
          this.MinuteText.SelectAll();
        }
        else
        {
          this.HourText.Focus();
          this.HourText.SelectAll();
        }
      }
      this._showPopupMouseDown = false;
    }

    private void OnInitialized(object sender, EventArgs e)
    {
      if (DateUtils.GetTimeDisplayFormat() != DateUtils.TimeDisplayFormat.Hour24)
        return;
      this.AmOrPmText.Visibility = Visibility.Collapsed;
    }

    public void SetTimeText(DateTime time)
    {
      this.ClearHourTextChangeHandler();
      this.ClearMinuteTextChangeHandler();
      this.ClearAmOrPmTextChangeHandler();
      this.HourText.Text = DateUtils.GetHourText(time);
      this.MinuteText.Text = DateUtils.GetMinuteText(time);
      this.AmOrPmText.Text = DateUtils.GetAmOrPm(time);
      this.HourText.TextChanged += new TextChangedEventHandler(this.HourTextChanged);
      this.MinuteText.TextChanged += new TextChangedEventHandler(this.MinuteTextChanged);
      this.AmOrPmText.TextChanged += new TextChangedEventHandler(this.AmOrPmTextChanged);
    }

    public void ClearSelection()
    {
      this.HourText.Select(0, 0);
      this.MinuteText.Select(0, 0);
      this.AmOrPmText.Select(0, 0);
    }

    public void CloseReminderPopup()
    {
      this.PopupOpen = false;
      if (!this.ReminderPopup.IsOpen)
        return;
      this.ReminderPopup.IsOpen = false;
    }

    public void ToggleReminderPopup()
    {
      if (!this.ReminderPopup.IsOpen)
      {
        SelectTimeDropDialog selectTimeDropDialog = new SelectTimeDropDialog(new DateTime?(this._selectedTime), this.ShowTimeZone && LocalSettings.Settings.EnableTimeZone, this.EnableEditTimeZone, this.TimeZone, this.BeginTime);
        selectTimeDropDialog.MinWidth = this.DropWidth;
        this._droplist = selectTimeDropDialog;
        this._droplist.Select += (EventHandler<DateTime>) ((arg, selected) =>
        {
          this.ReminderPopup.IsOpen = false;
          this._selectedTime = DateUtils.SetHourAndMinuteOnly(selected, selected.Hour, selected.Minute);
          this.SetTimeText(this._selectedTime);
          this.NotifyChanged();
        });
        this._droplist.TimeZoneChanged += (EventHandler<TimeZoneViewModel>) ((obj, model) =>
        {
          this.TimeZone = model;
          EventHandler<TimeZoneViewModel> timeZoneChanged = this.TimeZoneChanged;
          if (timeZoneChanged == null)
            return;
          timeZoneChanged(obj, model);
        });
        this.ReminderPopup.Child = (UIElement) this._droplist;
        this.ReminderPopup.IsOpen = true;
      }
      else
        this.ReminderPopup.IsOpen = false;
    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
      this.PopupOpen = true;
      PopupStateManager.SetCanOpenTimePopup(false);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      this.PopupOpen = false;
      PopupStateManager.SetCanOpenTimePopup(true, true);
    }

    private async void HourTextClick(object sender, MouseButtonEventArgs e)
    {
      await Task.Delay(150);
      this.HourText.SelectAll();
    }

    private async void MinuteTextClick(object sender, MouseButtonEventArgs e)
    {
      await Task.Delay(150);
      this.MinuteText.SelectAll();
    }

    private async void HourTextKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          this.ToggleReminderPopup();
          e.Handled = true;
          break;
        case Key.Left:
          if (this.AmOrPmText.IsVisible)
          {
            this.AmOrPmText.Focus();
            this.AmOrPmText.SelectAll();
            break;
          }
          this.MinuteText.Focus();
          this.MinuteText.SelectAll();
          break;
        case Key.Up:
          this.AddHour(-1);
          break;
        case Key.Right:
          this.MinuteText.Focus();
          this.MinuteText.SelectAll();
          break;
        case Key.Down:
          this.AddHour(1);
          break;
      }
    }

    private async void MinuteTextKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          this.ToggleReminderPopup();
          e.Handled = true;
          break;
        case Key.Left:
          this.HourText.Focus();
          this.HourText.SelectAll();
          break;
        case Key.Up:
          this.AddMinute(-15);
          break;
        case Key.Right:
          if (this.AmOrPmText.IsVisible)
          {
            this.AmOrPmText.Focus();
            this.AmOrPmText.SelectAll();
            break;
          }
          this.HourText.Focus();
          this.HourText.SelectAll();
          break;
        case Key.Down:
          this.AddMinute(15);
          break;
      }
    }

    private void AmOrPmKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Left:
          this.MinuteText.Focus();
          this.MinuteText.SelectAll();
          break;
        case Key.Up:
        case Key.Down:
        case Key.A:
        case Key.P:
          this.AmOrPmText.Text = this.AmOrPmText.Text == "AM" ? "PM" : "AM";
          break;
        case Key.Right:
          this.HourText.Focus();
          this.HourText.SelectAll();
          break;
      }
    }

    private void AddHour(int unit)
    {
      this.ClearHourTextChangeHandler();
      int result;
      int.TryParse(this.HourText.Text, out result);
      this.HourText.Text = TimeInputControl.PaddingLeftZero(result + unit);
      this._selectedTime = this._selectedTime.AddHours((double) unit);
      this.SetTimeText(this._selectedTime);
      this.NotifyChanged();
      this.HourText.SelectAll();
      this.HourText.TextChanged += new TextChangedEventHandler(this.HourTextChanged);
    }

    private void ClearHourTextChangeHandler()
    {
      this.HourText.TextChanged -= new TextChangedEventHandler(this.HourTextChanged);
      this.HourText.TextChanged -= new TextChangedEventHandler(this.HourTextChanged);
    }

    private void ClearMinuteTextChangeHandler()
    {
      this.MinuteText.TextChanged -= new TextChangedEventHandler(this.MinuteTextChanged);
      this.MinuteText.TextChanged -= new TextChangedEventHandler(this.MinuteTextChanged);
    }

    private void ClearAmOrPmTextChangeHandler()
    {
      this.AmOrPmText.TextChanged -= new TextChangedEventHandler(this.AmOrPmTextChanged);
      this.AmOrPmText.TextChanged -= new TextChangedEventHandler(this.AmOrPmTextChanged);
    }

    private static string PaddingLeftZero(int num)
    {
      return num >= 0 && num < 10 ? "0" + num.ToString() : num.ToString();
    }

    private void AddMinute(int unit)
    {
      this.ClearMinuteTextChangeHandler();
      int result;
      int.TryParse(this.MinuteText.Text, out result);
      int minute = result + unit;
      if (minute >= 60)
      {
        minute %= 60;
        this.AddHour(1);
      }
      else if (minute < 0)
      {
        minute += 60;
        this.AddHour(-1);
      }
      if (!Utils.IsEmptyDate(this._selectedTime))
      {
        this._selectedTime = DateUtils.SetMinuteOnly(this._selectedTime, minute);
        this.SetTimeText(this._selectedTime);
      }
      this.MinuteText.SelectAll();
      this.NotifyChanged();
      this.MinuteText.TextChanged += new TextChangedEventHandler(this.MinuteTextChanged);
    }

    private void HandleNumberInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private async void HourTextChanged(object sender, TextChangedEventArgs e)
    {
      TimeInputControl element = this;
      int result;
      int.TryParse(element.HourText.Text, out result);
      if (element.HourText.Text.Length <= 1)
        return;
      element.MinuteText.Focus();
      element.MinuteText.SelectAll();
      if (DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour12)
      {
        if (result < 0 || result > 12)
        {
          int num = result % 12;
          element.HourText.Text = num >= 10 ? num.ToString() : "0" + num.ToString();
        }
      }
      else if (result < 0 || result > 23)
      {
        int num = result % 12;
        element.HourText.Text = num >= 10 ? num.ToString() : "0" + num.ToString();
      }
      element.NotifyChanged();
      await Task.Delay(100);
      FocusManager.GetFocusedElement((DependencyObject) element);
    }

    private void MinuteTextChanged(object sender, TextChangedEventArgs e)
    {
      int result;
      int.TryParse(this.MinuteText.Text, out result);
      if (this.MinuteText.Text.Length <= 1)
        return;
      if (this.AmOrPmText.IsVisible)
      {
        this.AmOrPmText.Focus();
        this.AmOrPmText.SelectAll();
      }
      else
      {
        this.HourText.Focus();
        this.HourText.SelectAll();
      }
      if (result < 0 || result > 59)
        this.MinuteText.Text = "59";
      this.NotifyChanged();
    }

    private void AmOrPmTextChanged(object sender, TextChangedEventArgs e)
    {
      this.AmOrPmText.SelectAll();
      this.NotifyChanged();
    }

    private void AmOrPmTextPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.AmOrPmText.Text = this.AmOrPmText.Text == "AM" ? "PM" : "AM";
    }

    private async void NotifyChanged()
    {
      TimeInputControl sender = this;
      DateTime? selectedDate = sender.GetSelectedDate();
      if (!selectedDate.HasValue)
        return;
      sender._selectedTime = selectedDate.Value;
      EventHandler<DateTime> selectedTimeChanged = sender.SelectedTimeChanged;
      if (selectedTimeChanged == null)
        return;
      selectedTimeChanged((object) sender, sender._selectedTime);
    }

    private DateTime? GetSelectedDate()
    {
      if (this.HourText != null && this.MinuteText != null && this.AmOrPmText != null)
      {
        string text = this._selectedTime.ToString("yyyy-MM-dd ");
        string format = string.Empty;
        if (!string.IsNullOrEmpty(this.HourText.Text) && !string.IsNullOrEmpty(this.MinuteText.Text))
        {
          string str1 = this.MinuteText.Text;
          string str2 = this.HourText.Text;
          if (str1.Length == 1)
            str1 = "0" + str1;
          if (str2.Length == 1)
            str2 = "0" + str2;
          if (DateUtils.GetTimeDisplayFormat() == DateUtils.TimeDisplayFormat.Hour12)
          {
            if (!string.IsNullOrEmpty(this.AmOrPmText.Text))
            {
              string str3 = str2 + ":" + str1 + " " + this.AmOrPmText.Text;
              text += str3;
              format = "yyyy-MM-dd hh:mm tt";
            }
          }
          else
          {
            string str4 = str2 + ":" + str1;
            text += str4;
            format = "yyyy-MM-dd HH:mm";
          }
          DateTime? time = DateUtils.TryParseTime(text, format);
          if (time.HasValue)
          {
            this._selectedTime = time.Value;
            return new DateTime?(time.Value);
          }
        }
      }
      return new DateTime?();
    }

    public void HidePopup() => this.ReminderPopup.IsOpen = false;

    private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
    }

    public async void FoucusHourText()
    {
      await Task.Delay(50);
      this.FocusEmpty();
      this.HourText.Focus();
      this.HourText.SelectAll();
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      textBox.TextChanged -= new TextChangedEventHandler(this.HourTextChanged);
      textBox.TextChanged -= new TextChangedEventHandler(this.MinuteTextChanged);
      if (textBox.Text.Length == 1)
        textBox.Text = "0" + textBox.Text;
      else if (string.IsNullOrEmpty(textBox.Text))
        textBox.Text = "00";
      textBox.SelectAll();
      if (textBox == this.HourText)
        textBox.TextChanged += new TextChangedEventHandler(this.HourTextChanged);
      else
        textBox.TextChanged += new TextChangedEventHandler(this.MinuteTextChanged);
      this.NotifyChanged();
    }

    public void FocusEmpty() => this.EmptyBox.Focus();

    public void FocusHour()
    {
      this.HourText.Focus();
      this.HourText.SelectAll();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (!this.ReminderPopup.IsOpen || e.Key != Key.Escape)
        return;
      SelectTimeDropDialog droplist = this._droplist;
      if ((droplist != null ? (droplist.TryCloseTimezonePopup() ? 1 : 0) : 0) == 0)
        return;
      this.ReminderPopup.IsOpen = false;
      e.Handled = true;
    }

    public bool TryCloseTimezonePopup()
    {
      SelectTimeDropDialog droplist = this._droplist;
      return droplist != null && droplist.TryCloseTimezonePopup();
    }

    public DateTime? GetTime() => this.GetSelectedDate();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/timeinputcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TimeInputControl) target;
          this.Root.Initialized += new EventHandler(this.OnInitialized);
          this.Root.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnVisibleChanged);
          this.Root.Loaded += new RoutedEventHandler(this.OnInputLoaded);
          break;
        case 2:
          this.EmptyBox = (TextBox) target;
          break;
        case 3:
          this.InputGrid = (Border) target;
          this.InputGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          break;
        case 4:
          this.HourText = (TextBox) target;
          this.HourText.PreviewTextInput += new TextCompositionEventHandler(this.HandleNumberInput);
          this.HourText.KeyUp += new KeyEventHandler(this.HourTextKeyUp);
          this.HourText.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
          this.HourText.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 5:
          this.SplitText = (TextBlock) target;
          break;
        case 6:
          this.MinuteText = (TextBox) target;
          this.MinuteText.PreviewTextInput += new TextCompositionEventHandler(this.HandleNumberInput);
          this.MinuteText.KeyUp += new KeyEventHandler(this.MinuteTextKeyUp);
          this.MinuteText.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
          this.MinuteText.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 7:
          this.AmOrPmText = (TextBox) target;
          this.AmOrPmText.KeyUp += new KeyEventHandler(this.AmOrPmKeyUp);
          this.AmOrPmText.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
          this.AmOrPmText.TextChanged += new TextChangedEventHandler(this.AmOrPmTextChanged);
          this.AmOrPmText.PreviewMouseDown += new MouseButtonEventHandler(this.AmOrPmTextPreviewMouseDown);
          break;
        case 8:
          this.ReminderPopup = (Popup) target;
          this.ReminderPopup.Closed += new EventHandler(this.OnPopupClosed);
          this.ReminderPopup.Opened += new EventHandler(this.OnPopupOpened);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
