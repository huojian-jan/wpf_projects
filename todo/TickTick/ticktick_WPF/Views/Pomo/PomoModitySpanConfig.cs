// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoModitySpanConfig
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoModitySpanConfig : ContentControl, IComponentConnector
  {
    private Popup _popup;
    private bool _needResetPomoDuraion;
    private bool _collectEvent;
    internal TextBox PomoDurationText;
    internal TextBlock DurationHintText;
    private bool _contentLoaded;

    public PomoModitySpanConfig(Popup popup, bool collectEvent = false)
    {
      this.InitializeComponent();
      this._collectEvent = collectEvent;
      this._popup = popup;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      switch (TickFocusManager.Status)
      {
        case PomoStatus.WaitingWork:
          this.PomoDurationText.Tag = (object) "PomoDuration";
          this.DurationHintText.Text = string.Format(Utils.GetString("PomoEstimateSpan"), (object) "5~180");
          this.PomoDurationText.Text = LocalSettings.Settings.PomoDuration.ToString();
          break;
        case PomoStatus.WaitingRelax:
          if (LocalSettings.Settings.LongBreakEvery <= 1 || TickFocusManager.Config.PomoCount >= 1 && TickFocusManager.Config.PomoCount % LocalSettings.Settings.LongBreakEvery == 0)
          {
            this.PomoDurationText.Tag = (object) "LongBreakDuration";
            this.DurationHintText.Text = string.Format(Utils.GetString("LongBreakSpan"), (object) "1~60");
            this.PomoDurationText.Text = LocalSettings.Settings.LongBreakDuration.ToString();
            break;
          }
          this.PomoDurationText.Tag = (object) "ShortBreakDuration";
          this.DurationHintText.Text = string.Format(Utils.GetString("ShortBreakSpan"), (object) "1~60");
          this.PomoDurationText.Text = LocalSettings.Settings.ShortBreakDuration.ToString();
          break;
      }
      await Task.Delay(100);
      this.PomoDurationText.Focus();
      this.PomoDurationText.SelectAll();
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private void OnSettingsKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        this.OnSaveClick((object) null, (RoutedEventArgs) null);
      if (e.Key != Key.Up && e.Key != Key.Down)
        return;
      string key = this.PomoDurationText.Tag.ToString();
      if (!PomoSettings.PomoSettingsData.ContainsKey(key))
        return;
      Tuple<int, int, int> tuple = PomoSettings.PomoSettingsData[key];
      this.PomoDurationText.TextChanged -= new TextChangedEventHandler(this.OnSettingsTextChanged);
      int result;
      if (int.TryParse(this.PomoDurationText.Text, out result))
      {
        int num1 = tuple.Item2;
        int num2 = tuple.Item3;
        int num3 = e.Key == Key.Up ? result + 1 : result - 1;
        if (num3 >= num1 && num3 <= num2)
        {
          this.PomoDurationText.Text = num3.ToString();
          this.PomoDurationText.SelectAll();
        }
      }
      this.PomoDurationText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
    }

    private async void OnSettingsTextChanged(object sender, TextChangedEventArgs e)
    {
      Dictionary<string, Tuple<int, int, int>> pomoSettingsData = PomoSettings.PomoSettingsData;
      string key = this.PomoDurationText.Tag.ToString();
      if (!pomoSettingsData.ContainsKey(key))
        return;
      Tuple<int, int, int> tuple = pomoSettingsData[key];
      int result;
      int.TryParse(this.PomoDurationText.Text, out result);
      if (result > tuple.Item3)
      {
        this.PomoDurationText.Text = tuple.Item3.ToString();
        this.PomoDurationText.SelectAll();
      }
      else if (result < tuple.Item2)
      {
        this._needResetPomoDuraion = true;
        this.TryResetPomoDuration(tuple.Item2.ToString());
      }
      else
        this._needResetPomoDuraion = false;
    }

    private async void TryResetPomoDuration(string text)
    {
      await Task.Delay(1500);
      if (!this._needResetPomoDuraion)
        return;
      this.PomoDurationText.Text = text;
      this.PomoDurationText.SelectAll();
    }

    private void OnSaveClick(object sender, RoutedEventArgs routedEventArgs)
    {
      int result;
      if (int.TryParse(this.PomoDurationText.Text, out result))
      {
        Dictionary<string, Tuple<int, int, int>> pomoSettingsData = PomoSettings.PomoSettingsData;
        string key = this.PomoDurationText.Tag.ToString();
        if (result < pomoSettingsData[key].Item2)
          result = pomoSettingsData[key].Item2;
        LocalSettings.Settings[this.PomoDurationText.Tag.ToString()] = (object) result;
      }
      if (TickFocusManager.Status == PomoStatus.WaitingWork)
        TickFocusManager.Config.SetPomoSeconds();
      if (this._popup != null)
        this._popup.IsOpen = false;
      this._popup = (Popup) null;
      PomoSettings.SyncRemoteSettings();
      if (!this._collectEvent)
        return;
      UserActCollectUtils.AddClickEvent("focus_mini", "om", "modify_pomo");
    }

    private void OnCancelClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this._popup.IsOpen = false;
      this._popup = (Popup) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomomodityspanconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.PomoDurationText = (TextBox) target;
          this.PomoDurationText.PreviewTextInput += new TextCompositionEventHandler(this.OnTextInput);
          this.PomoDurationText.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.PomoDurationText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 2:
          this.DurationHintText = (TextBlock) target;
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
