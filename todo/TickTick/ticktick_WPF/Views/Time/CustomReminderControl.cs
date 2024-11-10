// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.CustomReminderControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class CustomReminderControl : UserControl, ITabControl, IComponentConnector
  {
    private DateTime _date;
    private int _tabIndex;
    internal TextBox RemindDayTextBox;
    internal TextBlock RemindDayTextBlock;
    internal TextBox RemindHourTextBox;
    internal TextBlock RemindHourTextBlock;
    internal TextBox RemindMinuteTextBox;
    internal TextBlock RemindMinuteTextBlock;
    internal TextBlock RemindTextBlock;
    internal TextBlock RemindTimeTextBlock;
    internal Button CancelButton;
    internal Button SaveButton;
    internal TextBox EmptyBox;
    private bool _contentLoaded;

    public event EventHandler OnCancel;

    public event EventHandler<string> OnSave;

    public CustomReminderControl(DateTime date)
    {
      this.InitializeComponent();
      this._date = date;
      if (Utils.IsEmptyDate(this._date))
        this.RemindTimeTextBlock.Visibility = Visibility.Collapsed;
      this.InitChangEvent();
      this.UpdateRemindTextBlock();
      this.Loaded += (RoutedEventHandler) (async (o, e) =>
      {
        await Task.Delay(150);
        this.RemindDayTextBox.Focus();
        this.RemindDayTextBox.SelectAll();
      });
    }

    private void InitChangEvent()
    {
      this.RemindDayTextBox.TextChanged -= new TextChangedEventHandler(this.RemindDayTextBox_TextChanged);
      this.RemindDayTextBox.TextChanged += new TextChangedEventHandler(this.RemindDayTextBox_TextChanged);
      this.RemindHourTextBox.TextChanged -= new TextChangedEventHandler(this.RemindHourTextBox_TextChanged);
      this.RemindHourTextBox.TextChanged += new TextChangedEventHandler(this.RemindHourTextBox_TextChanged);
      this.RemindMinuteTextBox.TextChanged -= new TextChangedEventHandler(this.RemindMinuteTextBox_TextChanged);
      this.RemindMinuteTextBox.TextChanged += new TextChangedEventHandler(this.RemindMinuteTextBox_TextChanged);
    }

    private void UpdateRemindTextBlock()
    {
      int result1;
      int.TryParse(this.RemindDayTextBox.Text, out result1);
      int result2;
      int.TryParse(this.RemindHourTextBox.Text, out result2);
      int result3;
      int.TryParse(this.RemindMinuteTextBox.Text, out result3);
      bool flag1 = result1 > 0;
      bool flag2 = result2 > 0;
      bool flag3 = result3 > 0;
      string str = Utils.IsCn() ? "" : " ";
      if (!flag1 && !flag2 && !flag3)
        this.RemindTextBlock.Text = Utils.GetString("OnTime");
      else
        this.RemindTextBlock.Text = string.Format(Utils.GetString("ReminderBefore"), flag1 ? (object) (this.RemindDayTextBox.Text + str) : (object) "", flag1 ? (object) (this.RemindDayTextBlock.Text + str) : (object) "", flag2 ? (object) (this.RemindHourTextBox.Text + str) : (object) "", flag2 ? (object) (this.RemindHourTextBlock.Text + str) : (object) "", flag3 ? (object) (this.RemindMinuteTextBox.Text + str) : (object) "", flag3 ? (object) (this.RemindMinuteTextBlock.Text + str) : (object) "");
      if (Utils.IsEmptyDate(this._date))
        return;
      this.SetRemindTimeTextBlockText(result1, result2, result3);
    }

    private void SetRemindTimeTextBlockText(int days, int hours, int minutes)
    {
      DateTime dateTime1 = this._date.AddDays((double) (days * -1));
      dateTime1 = dateTime1.AddHours((double) (hours * -1));
      DateTime dateTime2 = dateTime1.AddMinutes((double) (minutes * -1));
      string str;
      try
      {
        str = dateTime2.ToString("D", (IFormatProvider) App.Ci) + dateTime2.ToString(" " + DateUtils.GetTimeFormatString(), (IFormatProvider) App.Ci);
      }
      catch (FormatException ex)
      {
        str = dateTime2.ToString("MM/dd/yyyy HH:mm:ss");
      }
      this.RemindTimeTextBlock.Text = string.Format(Utils.GetString("ReminderTime"), (object) str);
    }

    private void customChooseRemindPopupCancelButton_Click(object sender, RoutedEventArgs e)
    {
      EventHandler onCancel = this.OnCancel;
      if (onCancel == null)
        return;
      onCancel((object) this, (EventArgs) null);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      UtilLog.Info("CustomReminder m " + this.RemindMinuteTextBox.Text + ", h" + this.RemindHourTextBox.Text + ", d" + this.RemindDayTextBox.Text);
      Dictionary<string, int> dict = new Dictionary<string, int>();
      int result1;
      int.TryParse(this.RemindMinuteTextBox.Text, out result1);
      dict.Add("M", result1);
      int result2;
      int.TryParse(this.RemindHourTextBox.Text, out result2);
      dict.Add("H", result2);
      int result3;
      int.TryParse(this.RemindDayTextBox.Text, out result3);
      dict.Add("D", result3);
      string triggerValue = Utils.GetTriggerValue(dict);
      EventHandler<string> onSave = this.OnSave;
      if (onSave == null)
        return;
      onSave((object) this, triggerValue);
    }

    private void RemindDayTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.RemindDayTextBox.Text == "")
      {
        this.UpdateRemindTextBlock();
      }
      else
      {
        int result;
        int.TryParse(this.RemindDayTextBox.Text, out result);
        this.RemindDayTextBox.Text = result.ToString();
        if (result > 60)
        {
          this.RemindDayTextBox.Text = "60";
          this.RemindDayTextBox.SelectAll();
        }
        if (result <= 0)
        {
          this.RemindDayTextBox.Text = "0";
          this.RemindDayTextBox.SelectAll();
        }
        this.RemindDayTextBlock.Text = Utils.GetString(result > 1 ? "PublicDays" : "PublicDay");
        this.UpdateRemindTextBlock();
      }
    }

    private void RemindHourTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.RemindHourTextBox.Text == "")
      {
        this.UpdateRemindTextBlock();
      }
      else
      {
        this.RemindHourTextBox.TextChanged -= new TextChangedEventHandler(this.RemindHourTextBox_TextChanged);
        int result;
        int.TryParse(this.RemindHourTextBox.Text, out result);
        this.RemindHourTextBox.Text = result.ToString();
        if (result > 23)
        {
          this.RemindHourTextBox.Text = "23";
          this.RemindHourTextBox.SelectAll();
        }
        if (result <= 0)
        {
          this.RemindHourTextBox.Text = "0";
          this.RemindHourTextBox.SelectAll();
        }
        this.RemindHourTextBlock.Text = Utils.GetString(result > 1 ? "PublicHours" : "PublicHour");
        this.RemindHourTextBox.TextChanged += new TextChangedEventHandler(this.RemindHourTextBox_TextChanged);
        this.UpdateRemindTextBlock();
      }
    }

    private void RemindMinuteTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.RemindMinuteTextBox.Text == "")
      {
        this.UpdateRemindTextBlock();
      }
      else
      {
        this.RemindMinuteTextBox.TextChanged -= new TextChangedEventHandler(this.RemindMinuteTextBox_TextChanged);
        int result;
        int.TryParse(this.RemindMinuteTextBox.Text, out result);
        this.RemindMinuteTextBox.Text = result.ToString();
        if (result > 59)
        {
          this.RemindMinuteTextBox.Text = "59";
          this.RemindMinuteTextBox.SelectAll();
        }
        if (result <= 0)
        {
          this.RemindMinuteTextBox.Text = "0";
          this.RemindMinuteTextBox.SelectAll();
        }
        this.RemindMinuteTextBlock.Text = Utils.GetString(result > 1 ? "PublicMinutes" : "PublicMinute");
        this.RemindMinuteTextBox.TextChanged += new TextChangedEventHandler(this.RemindMinuteTextBox_TextChanged);
        this.UpdateRemindTextBlock();
      }
    }

    private void OnlyNumberInput(object sender, KeyEventArgs e)
    {
      if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
      {
        e.Handled = true;
      }
      else
      {
        if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
          return;
        e.Handled = true;
      }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox textBox) || !(textBox.Text == ""))
        return;
      textBox.Text = "0";
    }

    public bool HandleTab(bool shift)
    {
      this._tabIndex += 5 + (shift ? -1 : 1);
      this._tabIndex %= 5;
      switch (this._tabIndex)
      {
        case 0:
          this.RemindDayTextBox.Focus();
          this.RemindDayTextBox.SelectAll();
          break;
        case 1:
          this.RemindHourTextBox.Focus();
          this.RemindHourTextBox.SelectAll();
          break;
        case 2:
          this.RemindMinuteTextBox.Focus();
          this.RemindMinuteTextBox.SelectAll();
          break;
        case 3:
        case 4:
          this.EmptyBox.Focus();
          break;
      }
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 3);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 4);
      return true;
    }

    public bool HandleEsc()
    {
      EventHandler onCancel = this.OnCancel;
      if (onCancel != null)
        onCancel((object) this, (EventArgs) null);
      return true;
    }

    public bool UpDownSelect(bool isUp) => false;

    public bool LeftRightSelect(bool isLeft) => false;

    public bool HandleEnter()
    {
      if (this._tabIndex == 3)
      {
        this.OnSaveClick((object) null, (RoutedEventArgs) null);
        return true;
      }
      if (this._tabIndex != 4)
        return false;
      EventHandler onCancel = this.OnCancel;
      if (onCancel != null)
        onCancel((object) this, (EventArgs) null);
      return true;
    }

    private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
    {
      if (sender.Equals((object) this.RemindDayTextBox))
        this._tabIndex = 0;
      else if (sender.Equals((object) this.RemindHourTextBox))
        this._tabIndex = 1;
      else if (sender.Equals((object) this.RemindMinuteTextBox))
        this._tabIndex = 2;
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/customremindercontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RemindDayTextBox = (TextBox) target;
          this.RemindDayTextBox.GotFocus += new RoutedEventHandler(this.OnTextBoxGotFocus);
          this.RemindDayTextBox.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
          this.RemindDayTextBox.PreviewKeyDown += new KeyEventHandler(this.OnlyNumberInput);
          break;
        case 2:
          this.RemindDayTextBlock = (TextBlock) target;
          break;
        case 3:
          this.RemindHourTextBox = (TextBox) target;
          this.RemindHourTextBox.GotFocus += new RoutedEventHandler(this.OnTextBoxGotFocus);
          this.RemindHourTextBox.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
          this.RemindHourTextBox.PreviewKeyDown += new KeyEventHandler(this.OnlyNumberInput);
          break;
        case 4:
          this.RemindHourTextBlock = (TextBlock) target;
          break;
        case 5:
          this.RemindMinuteTextBox = (TextBox) target;
          this.RemindMinuteTextBox.GotFocus += new RoutedEventHandler(this.OnTextBoxGotFocus);
          this.RemindMinuteTextBox.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
          this.RemindMinuteTextBox.PreviewKeyDown += new KeyEventHandler(this.OnlyNumberInput);
          break;
        case 6:
          this.RemindMinuteTextBlock = (TextBlock) target;
          break;
        case 7:
          this.RemindTextBlock = (TextBlock) target;
          break;
        case 8:
          this.RemindTimeTextBlock = (TextBlock) target;
          break;
        case 9:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.customChooseRemindPopupCancelButton_Click);
          break;
        case 10:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 11:
          this.EmptyBox = (TextBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
