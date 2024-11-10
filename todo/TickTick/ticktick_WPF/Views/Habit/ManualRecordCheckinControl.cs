// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.ManualRecordCheckinControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class ManualRecordCheckinControl : UserControl, IComponentConnector
  {
    internal TextBlock DateText;
    internal TextBlock HintText;
    internal TextBlock LeftLabel;
    internal TextBox CheckInText;
    internal TextBlock UnitText;
    private bool _contentLoaded;

    public ManualRecordCheckinControl() => this.InitializeComponent();

    public event EventHandler<double> Save;

    public event EventHandler Cancel;

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this.SaveAmount(sender, (EventArgs) e);
    }

    private void SaveAmount(object sender, EventArgs e)
    {
      double db;
      if (MathUtil.TryParseString2Double(this.CheckInText.Text, out db))
      {
        if (0.0 < db && db <= 99999.99)
        {
          EventHandler<double> save = this.Save;
          if (save == null)
            return;
          save(sender, db);
        }
        else
        {
          if (this.HintText.Visibility != Visibility.Visible || db != 0.0)
            return;
          EventHandler<double> save = this.Save;
          if (save == null)
            return;
          save(sender, db);
        }
      }
      else
      {
        EventHandler cancel = this.Cancel;
        if (cancel == null)
          return;
        cancel(sender, e);
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel == null)
        return;
      cancel(sender, (EventArgs) e);
    }

    public async void Init(string habitUnit)
    {
      this.UnitText.Text = HabitUtils.GetUnitText(habitUnit);
      this.CheckInText.Text = string.Empty;
      await Task.Delay(100);
      this.CheckInText.Focus();
    }

    public async void Init(double value, string habitUnit, bool isToday)
    {
      this.UnitText.Text = HabitUtils.GetUnitText(habitUnit);
      this.CheckInText.Text = value.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this.HintText.Text = string.Format(Utils.GetString("CompleteHabitCount"), (object) Utils.GetString(isToday ? "Today" : "ThisDay"));
      this.HintText.Visibility = Visibility.Visible;
      await Task.Delay(1);
      this.CheckInText.SelectAll();
      this.CheckInText.Focus();
    }

    private void OnTextPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length >= 1 && !char.IsDigit(e.Text, e.Text.Length - 1) && e.Text != ".")
        e.Handled = true;
      if (!(sender is TextBox textBox))
        return;
      if (textBox.Text.Contains("."))
      {
        if (e.Text == ".")
          e.Handled = true;
        int length = textBox.Text.IndexOf(".", StringComparison.Ordinal);
        if (textBox.CaretIndex > length)
        {
          if (textBox.Text.Substring(length + 1, textBox.Text.Length - length - 1).Length < 2)
            return;
          e.Handled = true;
        }
        else
        {
          if (textBox.Text.Substring(0, length).Length < 5)
            return;
          e.Handled = true;
        }
      }
      else
      {
        if (textBox.Text.Length >= 5 && e.Text != ".")
          e.Handled = true;
        if (!(e.Text == ".") || textBox.CaretIndex != 0)
          return;
        e.Handled = true;
      }
    }

    private void OnInputKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        this.SaveAmount(sender, (EventArgs) e);
      }
      else
      {
        if (e.Key != Key.Escape)
          return;
        EventHandler cancel = this.Cancel;
        if (cancel == null)
          return;
        cancel(sender, (EventArgs) e);
      }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      bool flag = false;
      string str = textBox.Text;
      int caretIndex = textBox.CaretIndex;
      int num = 0;
      if (str.Contains(" "))
      {
        foreach (char ch in str)
        {
          if (ch == ' ')
            ++num;
        }
        str = str.Replace(" ", "");
      }
      double db;
      if (!MathUtil.TryParseString2Double(str, out db) || Math.Abs(db) < 0.0)
      {
        str = "0";
        flag = true;
      }
      if (!(textBox.Text != str))
        return;
      textBox.Text = str;
      if (flag)
        textBox.SelectAll();
      else
        textBox.CaretIndex = Math.Min(caretIndex - num, str.Length);
    }

    public void ClearSaveHandler()
    {
      if (this.Save == null)
        return;
      foreach (Delegate invocation in this.Save.GetInvocationList())
      {
        if (invocation is EventHandler<double> eventHandler)
          this.Save -= eventHandler;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/manualrecordcheckincontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.DateText = (TextBlock) target;
          break;
        case 2:
          this.HintText = (TextBlock) target;
          break;
        case 3:
          this.LeftLabel = (TextBlock) target;
          break;
        case 4:
          this.CheckInText = (TextBox) target;
          this.CheckInText.PreviewTextInput += new TextCompositionEventHandler(this.OnTextPreviewInput);
          this.CheckInText.KeyUp += new KeyEventHandler(this.OnInputKeyUp);
          this.CheckInText.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 5:
          this.UnitText = (TextBlock) target;
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
