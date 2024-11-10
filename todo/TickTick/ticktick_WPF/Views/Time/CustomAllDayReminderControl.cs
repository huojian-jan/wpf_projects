// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.CustomAllDayReminderControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class CustomAllDayReminderControl : UserControl, ITabControl, IComponentConnector
  {
    private string _currentType;
    private EscPopup _popup;
    private ObservableCollection<ComboBoxViewModel> _dayOrWeekItems;
    private string _advanceByDay;
    private string _advanceByWeek;
    private int _tabIndex = -1;
    internal CustomComboBox DayOrWeekComboBox;
    internal TextBlock AdvanceText;
    internal TextBox DayOrWeekText;
    internal TimeInputControl TimeInput;
    internal TextBlock ReminderText;
    internal Button CancelButton;
    internal Button SaveButton;
    internal TextBox EmptyWidth;
    private bool _contentLoaded;

    public event EventHandler<string> Save;

    public CustomAllDayReminderControl(AdvanceDateModel model, EscPopup popup, bool inTab)
    {
      this.InitializeComponent();
      this.DataContext = (object) model;
      this.InitTimeInput(model);
      this._popup = popup;
      this._popup.Child = (UIElement) this;
      this._tabIndex = !inTab ? 1 : 0;
      this.DayOrWeekText.TextChanged -= new TextChangedEventHandler(this.OnDayOrWeekTextChanged);
      this.DayOrWeekText.TextChanged += new TextChangedEventHandler(this.OnDayOrWeekTextChanged);
      this._currentType = "ByDay";
      this.InitDayOrWeekComboBox();
    }

    private void InitDayOrWeekComboBox()
    {
      this._advanceByDay = Utils.GetString("AdvanceByDay");
      this._advanceByWeek = Utils.GetString("AdvanceByWeek");
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) this._advanceByDay, this._advanceByDay, 32.0);
      comboBoxViewModel.Selected = true;
      observableCollection.Add(comboBoxViewModel);
      observableCollection.Add(new ComboBoxViewModel((object) this._advanceByWeek, this._advanceByWeek, 32.0));
      this._dayOrWeekItems = observableCollection;
      this.DayOrWeekComboBox.Init<ComboBoxViewModel>(this._dayOrWeekItems, this._dayOrWeekItems[0]);
    }

    private void InitTimeInput(AdvanceDateModel model)
    {
      this.TimeInput.SelectedTime = DateUtils.SetHourAndMinuteOnly(DateTime.Today, model.Hour, model.Minute);
      this.TimeInput.SelectedTimeChanged -= new EventHandler<DateTime>(this.OnTimeChanged);
      this.TimeInput.SelectedTimeChanged += new EventHandler<DateTime>(this.OnTimeChanged);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      if (this._popup == null)
        return;
      this._popup.IsOpen = false;
    }

    private void OnDayOrWeekPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (sender == null || !(sender is TextBox textBox))
        return;
      bool flag = int.TryParse(textBox.Text.Insert(textBox.SelectionStart, e.Text), out int _);
      e.Handled = !flag;
    }

    private async void OnWeekTextChanged()
    {
      CustomAllDayReminderControl dayReminderControl = this;
      int result;
      int.TryParse(dayReminderControl.DayOrWeekText.Text, out result);
      if (result > 12)
      {
        dayReminderControl.DayOrWeekText.Text = "12";
        dayReminderControl.FocusDayOrWeekText();
      }
      else if (result < 0)
      {
        dayReminderControl.DayOrWeekText.Text = "0";
        dayReminderControl.FocusDayOrWeekText();
      }
      else
      {
        dayReminderControl.DayOrWeekText.Text = result.ToString();
        dayReminderControl.AdvanceText.Text = Utils.GetString(result > 1 ? "WeeksInAdvance" : "WeekInAdvance");
        if (result == 0)
          dayReminderControl.FocusDayOrWeekText();
        if (dayReminderControl.DataContext == null || !(dayReminderControl.DataContext is AdvanceDateModel dataContext))
          return;
        dataContext.AdvanceDays = result * 7;
        dataContext.Trigger = dataContext.ToRule();
      }
    }

    private void FocusDayOrWeekText()
    {
      if (this._tabIndex != 1)
        return;
      this.DayOrWeekText.Focus();
      this.DayOrWeekText.SelectAll();
    }

    private async Task OnDayTextChanged()
    {
      CustomAllDayReminderControl dayReminderControl = this;
      int result;
      int.TryParse(dayReminderControl.DayOrWeekText.Text, out result);
      if (result > 60)
      {
        dayReminderControl.DayOrWeekText.Text = "60";
        dayReminderControl.DayOrWeekText.SelectAll();
      }
      else if (result < 0)
      {
        dayReminderControl.DayOrWeekText.Text = "0";
        dayReminderControl.DayOrWeekText.SelectAll();
      }
      else
      {
        dayReminderControl.DayOrWeekText.Text = result.ToString();
        dayReminderControl.AdvanceText.Text = Utils.GetString(result > 1 ? "DaysInAdvance" : "DayInAdvance");
        if (result == 0)
          dayReminderControl.DayOrWeekText.SelectAll();
        if (dayReminderControl.DataContext == null || !(dayReminderControl.DataContext is AdvanceDateModel dataContext))
          return;
        dataContext.AdvanceDays = result;
        dataContext.Trigger = dataContext.ToRule();
      }
    }

    private async void OnDayOrWeekTextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.DayOrWeekComboBox.SelectedItem.Value == (object) this._advanceByDay)
        this.OnDayTextChanged();
      else
        this.OnWeekTextChanged();
    }

    private void TrySaveAndClose()
    {
      if (this.DataContext == null || !(this.DataContext is AdvanceDateModel dataContext))
        return;
      if (this._popup != null)
        this._popup.IsOpen = false;
      EventHandler<string> save = this.Save;
      if (save == null)
        return;
      save((object) this, dataContext.ToRule());
    }

    private async void OnInit(object sender, EventArgs e)
    {
      await Task.Delay(100);
      if (this._tabIndex >= 0)
        this.DayOrWeekComboBox.TabSelected = true;
      this.FocusDayOrWeekText();
    }

    private void TimeClick(object sender, MouseButtonEventArgs e)
    {
      this.DayOrWeekComboBox.TabSelected = false;
      this.TimeInput.TabSelected = false;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, false);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, false);
    }

    private void OnTimeChanged(object sender, DateTime date)
    {
      if (this.DataContext == null || !(this.DataContext is AdvanceDateModel dataContext))
        return;
      dataContext.Hour = date.Hour;
      dataContext.Minute = date.Minute;
      dataContext.Trigger = dataContext.ToRule();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.TrySaveAndClose();

    private void DayOrWeekComboBox_SelectionChanged(object sender, ComboBoxViewModel model)
    {
      this.TimeInput.HourText.Text = "09";
      this.TimeInput.MinuteText.Text = "00";
      this.TimeInput.AmOrPmText.Text = "AM";
      bool flag = model.Value == (object) this._advanceByDay;
      if (this.DayOrWeekText.Text == "1")
      {
        if (flag)
          this.OnDayTextChanged();
        else
          this.OnWeekTextChanged();
      }
      else
      {
        this.DayOrWeekText.Text = "1";
        this.AdvanceText.Text = Utils.GetString(flag ? "DayInAdvance" : "WeekInAdvance");
      }
    }

    public async void Show() => this._popup.IsOpen = true;

    public bool HandleEsc()
    {
      if (!this.TimeInput.ReminderPopup.IsOpen)
        return false;
      this.TimeInput.ReminderPopup.IsOpen = false;
      return true;
    }

    public bool HandleTab(bool shift)
    {
      if (this.DayOrWeekComboBox.IsOpen || this.TimeInput.ReminderPopup.IsOpen)
        return true;
      this._tabIndex += 5 + (shift ? -1 : 1);
      this._tabIndex %= 5;
      if (this._tabIndex == 1)
      {
        this.DayOrWeekText.Focus();
        this.DayOrWeekText.SelectAll();
      }
      else
        this.EmptyWidth.Focus();
      this.DayOrWeekComboBox.TabSelected = this._tabIndex == 0;
      this.TimeInput.TabSelected = this._tabIndex == 2;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 3);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 4);
      return true;
    }

    public bool UpDownSelect(bool isUp)
    {
      if (this.DayOrWeekComboBox.IsOpen)
        this.DayOrWeekComboBox.UpDownSelect(isUp);
      return this.DayOrWeekComboBox.IsOpen;
    }

    public bool LeftRightSelect(bool isLeft) => false;

    public bool HandleEnter()
    {
      switch (this._tabIndex)
      {
        case 0:
          if (this.DayOrWeekComboBox.TabSelected)
            return this.DayOrWeekComboBox.HandleEnter();
          break;
        case 2:
          this.TimeInput.ToggleReminderPopup();
          this.TimeInput.FocusHour();
          break;
        case 3:
          this.TrySaveAndClose();
          break;
        case 4:
          this.OnCancelClick((object) null, (RoutedEventArgs) null);
          break;
      }
      return true;
    }

    private void OnTextGotFocus(object sender, RoutedEventArgs e)
    {
      this._tabIndex = 1;
      this.DayOrWeekText.Focus();
      this.DayOrWeekText.SelectAll();
      this.DayOrWeekComboBox.TabSelected = false;
      this.TimeInput.TabSelected = false;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, false);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, false);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/customalldayremindercontrol.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Initialized += new EventHandler(this.OnInit);
          break;
        case 2:
          this.DayOrWeekComboBox = (CustomComboBox) target;
          break;
        case 3:
          this.AdvanceText = (TextBlock) target;
          break;
        case 4:
          this.DayOrWeekText = (TextBox) target;
          this.DayOrWeekText.PreviewTextInput += new TextCompositionEventHandler(this.OnDayOrWeekPreviewInput);
          this.DayOrWeekText.GotFocus += new RoutedEventHandler(this.OnTextGotFocus);
          break;
        case 5:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.TimeClick);
          break;
        case 6:
          this.TimeInput = (TimeInputControl) target;
          break;
        case 7:
          this.ReminderText = (TextBlock) target;
          break;
        case 8:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 9:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 10:
          this.EmptyWidth = (TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
