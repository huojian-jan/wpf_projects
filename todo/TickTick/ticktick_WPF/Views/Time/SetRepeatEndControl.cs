// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetRepeatEndControl
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
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetRepeatEndControl : UserControl, ITabControl, IComponentConnector
  {
    private int _repeatUntilIndex;
    internal StackPanel RepeatEndSelectPanel;
    internal OptionItemWithImageIcon NoRepeat;
    internal OptionItemWithImageIcon RepeatUntil;
    internal OptionItemWithImageIcon RepeatCount;
    internal StackPanel SelectRepeatUntilPanel;
    internal TextBox RepeatCountText;
    internal Button SaveButton;
    internal Button CancelButton;
    internal TickDatePicker SelectRepeatEndDatePicker;
    internal TextBox EmptyBox;
    private bool _contentLoaded;

    public event RepeatEndHandler RepeatEndChange;

    public event EventHandler Cancel;

    public SetRepeatEndControl()
    {
      this.InitializeComponent();
      this.SelectRepeatEndDatePicker.SetMinDate(new DateTime?(DateTime.Today));
    }

    public void Init(bool enter)
    {
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(this.TimeData.RepeatFlag);
      if (recurrenceModel.Count > 1)
        this.ShowSelectCountDialog();
      else if (Utils.IsEmptyDate(recurrenceModel.Until))
      {
        this.RepeatEndSelectPanel.Visibility = Visibility.Visible;
        this.SelectRepeatEndDatePicker.Visibility = Visibility.Collapsed;
        this.SelectRepeatUntilPanel.Visibility = Visibility.Collapsed;
        if (!enter)
          return;
        this.NoRepeat.HoverSelected = true;
      }
      else
      {
        this.ShowSelectEndPicker();
        if (!enter)
          return;
        this.SelectRepeatEndDatePicker.TabSelectCurrent();
      }
    }

    private TimeData TimeData => (TimeData) this.DataContext;

    private void OnSetNoRepeatEndClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.ClearRepeatEnd();
    }

    private void OnSelectRepeatClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.ShowSelectEndPicker();
    }

    private void ShowSelectEndPicker()
    {
      this.RepeatEndSelectPanel.Visibility = Visibility.Collapsed;
      this.SelectRepeatEndDatePicker.Visibility = Visibility.Visible;
      this.SelectRepeatUntilPanel.Visibility = Visibility.Collapsed;
      bool flag = false;
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(this.TimeData.RepeatFlag);
      DateTime? nullable1 = this.TimeData.StartDate;
      DateTime date;
      if (nullable1.HasValue)
      {
        date = nullable1.Value;
        if (!(date.Date <= DateTime.Today))
          goto label_3;
      }
      nullable1 = new DateTime?(DateTime.Today);
label_3:
      date = nullable1.Value;
      date = date.Date;
      DateTime until = date.AddDays(7.0);
      if (!Utils.IsEmptyDate(recurrenceModel.Until))
        until = recurrenceModel.Until;
      else
        flag = true;
      this.SelectRepeatEndDatePicker.ClearTabSelected();
      TickDatePicker repeatEndDatePicker = this.SelectRepeatEndDatePicker;
      DateTime? selectedDate = new DateTime?(until);
      DateTime? nullable2 = new DateTime?(DateTime.Today);
      DateTime? selectStart = new DateTime?();
      DateTime? selectEnd = new DateTime?();
      DateTime? maxDate = new DateTime?();
      DateTime? minDate = nullable2;
      repeatEndDatePicker.SetData(selectedDate, selectStart, selectEnd, maxDate: maxDate, minDate: minDate);
      if (!flag)
        return;
      this.TimeData.RepeatFlag = RepeatUtils.GetRepeatFlag(this.TimeData.RepeatFlag, until);
    }

    private void OnRepeatEndSelected(object sender, DateTime until)
    {
      DateTime dateTime = until;
      DateTime? startDate = this.TimeData.StartDate;
      if ((startDate.HasValue ? (dateTime < startDate.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        return;
      RepeatEndHandler repeatEndChange = this.RepeatEndChange;
      if (repeatEndChange == null)
        return;
      repeatEndChange(-1, until);
    }

    private void ClearRepeatEnd()
    {
      RepeatEndHandler repeatEndChange = this.RepeatEndChange;
      if (repeatEndChange == null)
        return;
      repeatEndChange(-1, new DateTime());
    }

    private void OnSelectCountClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowSelectCountDialog();
    }

    private void ShowSelectCountDialog()
    {
      this.RepeatEndSelectPanel.Visibility = Visibility.Collapsed;
      this.SelectRepeatEndDatePicker.Visibility = Visibility.Collapsed;
      this.SelectRepeatUntilPanel.Visibility = Visibility.Visible;
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(this.TimeData.RepeatFlag);
      this.RepeatCountText.Text = recurrenceModel.Count > 0 ? recurrenceModel.Count.ToString() : "2";
      this.RepeatCountText.SelectAll();
      this.RepeatCountText.Focus();
    }

    private void OnRepeatCountCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel == null)
        return;
      cancel((object) this, (EventArgs) null);
    }

    private void OnRepeatCountSaveClick(object sender, RoutedEventArgs e)
    {
      int count = int.Parse(this.RepeatCountText.Text);
      RepeatEndHandler repeatEndChange = this.RepeatEndChange;
      if (repeatEndChange == null)
        return;
      repeatEndChange(count, new DateTime());
    }

    private void OnRepeatCountPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (sender == null || !(sender is TextBox textBox))
        return;
      bool flag = int.TryParse(textBox.Text.Insert(textBox.SelectionStart, e.Text), out int _);
      e.Handled = !flag;
    }

    private void OnRepeatCountTextChanged(object sender, TextChangedEventArgs e)
    {
      DelayActionHandlerCenter.TryDoAction("CheckRepeatCountText", new EventHandler(this.CheckTextValid), 400);
    }

    private void CheckTextValid(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.CheckInputValid));
    }

    private void CheckInputValid()
    {
      int result;
      int.TryParse(this.RepeatCountText.Text, out result);
      if (result > 200)
      {
        this.RepeatCountText.Text = "200";
        this.RepeatCountText.SelectAll();
      }
      if (result > 1)
        return;
      this.RepeatCountText.Text = "2";
      this.RepeatCountText.SelectAll();
    }

    public bool HandleTab(bool shift)
    {
      if (this.SelectRepeatUntilPanel.Visibility == Visibility.Visible)
      {
        this._repeatUntilIndex += 3 + (shift ? -1 : 1);
        this._repeatUntilIndex %= 3;
        if (this._repeatUntilIndex == 0)
          this.RepeatCountText.Focus();
        else
          this.EmptyBox.Focus();
        UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._repeatUntilIndex == 1);
        UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._repeatUntilIndex == 2);
      }
      return true;
    }

    public bool HandleEnter()
    {
      if (this.RepeatEndSelectPanel.Visibility == Visibility.Visible)
      {
        if (this.NoRepeat.HoverSelected)
          this.ClearRepeatEnd();
        else if (this.RepeatUntil.HoverSelected)
        {
          this.ShowSelectEndPicker();
          this.SelectRepeatEndDatePicker.TabSelectCurrent();
        }
        else if (this.RepeatCount.HoverSelected)
          this.ShowSelectCountDialog();
        return true;
      }
      if (this.SelectRepeatUntilPanel.Visibility == Visibility.Visible)
      {
        switch (this._repeatUntilIndex)
        {
          case 1:
            int count = int.Parse(this.RepeatCountText.Text);
            RepeatEndHandler repeatEndChange = this.RepeatEndChange;
            if (repeatEndChange != null)
            {
              repeatEndChange(count, new DateTime());
              break;
            }
            break;
          case 2:
            EventHandler cancel = this.Cancel;
            if (cancel != null)
            {
              cancel((object) this, (EventArgs) null);
              break;
            }
            break;
        }
      }
      if (this.SelectRepeatEndDatePicker.Visibility == Visibility.Visible)
        this.SelectRepeatEndDatePicker.SelectTabItem();
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.RepeatEndSelectPanel.Visibility == Visibility.Visible)
      {
        if (!this.NoRepeat.HoverSelected && !this.RepeatUntil.HoverSelected && !this.RepeatCount.HoverSelected)
          this.NoRepeat.HoverSelected = true;
        else if (this.NoRepeat.HoverSelected)
        {
          this.NoRepeat.HoverSelected = false;
          this.RepeatUntil.HoverSelected = !isUp;
          this.RepeatCount.HoverSelected = isUp;
        }
        else if (this.RepeatUntil.HoverSelected)
        {
          this.NoRepeat.HoverSelected = isUp;
          this.RepeatUntil.HoverSelected = false;
          this.RepeatCount.HoverSelected = !isUp;
        }
        else
        {
          this.NoRepeat.HoverSelected = !isUp;
          this.RepeatUntil.HoverSelected = isUp;
          this.RepeatCount.HoverSelected = false;
        }
      }
      else if (this.SelectRepeatEndDatePicker.Visibility == Visibility.Visible)
        this.SelectRepeatEndDatePicker.MoveTabSelectDate(isUp ? -7 : 7);
      return true;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.SelectRepeatEndDatePicker.Visibility == Visibility.Visible)
        this.SelectRepeatEndDatePicker.MoveTabSelectDate(isLeft ? -1 : 1);
      return true;
    }

    private void OnItemMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is OptionItemWithImageIcon itemWithImageIcon))
        return;
      this.NoRepeat.HoverSelected = false;
      this.RepeatUntil.HoverSelected = false;
      this.RepeatCount.HoverSelected = false;
      itemWithImageIcon.HoverSelected = true;
    }

    private void OnRepeatCountFocused(object sender, RoutedEventArgs e)
    {
      this._repeatUntilIndex = 0;
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/setrepeatendcontrol.xaml", UriKind.Relative));
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
          this.RepeatEndSelectPanel = (StackPanel) target;
          break;
        case 2:
          this.NoRepeat = (OptionItemWithImageIcon) target;
          break;
        case 3:
          this.RepeatUntil = (OptionItemWithImageIcon) target;
          break;
        case 4:
          this.RepeatCount = (OptionItemWithImageIcon) target;
          break;
        case 5:
          this.SelectRepeatUntilPanel = (StackPanel) target;
          break;
        case 6:
          this.RepeatCountText = (TextBox) target;
          this.RepeatCountText.PreviewTextInput += new TextCompositionEventHandler(this.OnRepeatCountPreviewInput);
          this.RepeatCountText.GotFocus += new RoutedEventHandler(this.OnRepeatCountFocused);
          this.RepeatCountText.TextChanged += new TextChangedEventHandler(this.OnRepeatCountTextChanged);
          break;
        case 7:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnRepeatCountSaveClick);
          break;
        case 8:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnRepeatCountCancelClick);
          break;
        case 9:
          this.SelectRepeatEndDatePicker = (TickDatePicker) target;
          break;
        case 10:
          this.EmptyBox = (TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
