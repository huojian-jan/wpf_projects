// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.EnsureTimingDialog
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class EnsureTimingDialog : MyWindow, IComponentConnector
  {
    private long _time;
    private long _hour;
    private long _minute;
    private long _second;
    internal StackPanel EstimatePanel;
    internal TextBox HourCount;
    internal TextBox MinuteCount;
    internal TextBlock DescText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public event EventHandler<long> OnSaveClick;

    public EnsureTimingDialog(long second = 0)
    {
      this._second = second;
      this._time = (long) Utils.SecondToMinute(second);
      this._hour = this._time / 60L;
      this._minute = this._time % 60L;
      this.InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.HourCount.Text = this._hour.ToString() ?? "";
      this.MinuteCount.Text = this._minute.ToString() ?? "";
      if (this._time != 720L)
        return;
      this.DescText.Text = Utils.GetString("EnsureTimingText2");
    }

    private void OnTextPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (sender == null || !(sender is TextBox textBox))
        return;
      bool flag = long.TryParse(textBox.Text.Insert(textBox.SelectionStart, e.Text), out long _);
      e.Handled = !flag;
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this.TrySaveAndClose();
    }

    private void TrySaveAndClose()
    {
      EventHandler<long> onSaveClick = this.OnSaveClick;
      if (onSaveClick != null)
        onSaveClick((object) this, this._hour * 60L + this._minute == this._time ? this._second : this._hour * 3600L + this._minute * 60L);
      this.Close();
    }

    private void OnSaveButtonClick(object sender, RoutedEventArgs e) => this.TrySaveAndClose();

    private async void OnHourTextChanged(object sender, TextChangedEventArgs e)
    {
      await Task.Delay(10);
      int result;
      int.TryParse(this.HourCount.Text, out result);
      if (result >= 12)
      {
        this.HourCount.Text = 12.ToString();
        this.MinuteCount.Text = "0";
        this.HourCount.SelectAll();
        this._hour = 12L;
        this._minute = 0L;
      }
      else if (result <= 0)
      {
        this.HourCount.Text = "0";
        this.HourCount.SelectAll();
        this._hour = 0L;
        if (this._minute < 5L)
        {
          this.MinuteCount.Text = "5";
          this._minute = 5L;
        }
      }
      else
        this._hour = (long) result;
      this.CheckTimeNum();
    }

    private void CheckTimeNum()
    {
      if (this._hour * 60L + this._minute > this._time)
      {
        this.DescText.Text = string.Format(Utils.GetString("EnsureTimingText3"), (object) Utils.GetDurationString(this._time * 60L));
        this.SaveButton.IsEnabled = false;
      }
      else
      {
        if (this._time < 720L)
          this.DescText.Text = Utils.GetString("EnsureTimingText1");
        this.SaveButton.IsEnabled = true;
      }
    }

    private async void OnMinuteTextChanged(object sender, TextChangedEventArgs e)
    {
      await Task.Delay(10);
      int result1;
      int.TryParse(this.MinuteCount.Text, out result1);
      if (result1 > 59)
      {
        this.MinuteCount.Text = "59";
        this.MinuteCount.SelectAll();
        this._minute = (long) result1;
      }
      else if (this._hour == 0L && result1 < 5)
      {
        await Task.Delay(1000);
        int result2;
        int.TryParse(this.MinuteCount.Text, out result2);
        if (result2 < 5)
        {
          this.MinuteCount.Text = "5";
          this.MinuteCount.SelectAll();
          this._minute = 5L;
        }
      }
      else if (result1 < 0)
      {
        this.MinuteCount.Text = "0";
        this.MinuteCount.SelectAll();
        this._minute = 0L;
      }
      else
        this._minute = (long) result1;
      this.CheckTimeNum();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/ensuretimingdialog.xaml", UriKind.Relative));
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
          this.EstimatePanel = (StackPanel) target;
          break;
        case 2:
          this.HourCount = (TextBox) target;
          this.HourCount.PreviewTextInput += new TextCompositionEventHandler(this.OnTextPreviewInput);
          this.HourCount.TextChanged += new TextChangedEventHandler(this.OnHourTextChanged);
          this.HourCount.KeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 3:
          this.MinuteCount = (TextBox) target;
          this.MinuteCount.PreviewTextInput += new TextCompositionEventHandler(this.OnTextPreviewInput);
          this.MinuteCount.TextChanged += new TextChangedEventHandler(this.OnMinuteTextChanged);
          this.MinuteCount.KeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 4:
          this.DescText = (TextBlock) target;
          break;
        case 5:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveButtonClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
