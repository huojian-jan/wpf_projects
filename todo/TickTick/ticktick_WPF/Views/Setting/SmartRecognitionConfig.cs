// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.SmartRecognitionConfig
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
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class SmartRecognitionConfig : UserControl, IComponentConnector
  {
    internal CheckBox DateToggleCheckbox;
    internal CheckBox RemoveDateTextCheckbox;
    internal CheckBox RemoteTagTextCheckbox;
    internal CheckBox UrlParseCheckbox;
    private bool _contentLoaded;

    public SmartRecognitionConfig()
    {
      this.InitializeComponent();
      this.InitKeepTags();
      this.InitDateParsingSettings();
      this.InitParseUrl();
    }

    private void RemoveDateTextChecked(object sender, RoutedEventArgs e)
    {
      LocalSettings.Settings.RemoveTimeText = ((int) this.RemoveDateTextCheckbox.IsChecked ?? 1) != 0;
      LocalSettings.Settings.Save();
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void DateParsingClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (sender is CheckBox checkBox1)
      {
        CheckBox checkBox = checkBox1;
        bool? isChecked = checkBox1.IsChecked;
        bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
        checkBox.IsChecked = nullable;
        LocalSettings.Settings.DateParsing = ((int) checkBox1.IsChecked ?? 1) != 0;
        LocalSettings.Settings.Save();
      }
      this.RemoveDateTextCheckbox.Visibility = LocalSettings.Settings.DateParsing ? Visibility.Visible : Visibility.Collapsed;
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void RemoveDateTextClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (sender is CheckBox checkBox1)
      {
        CheckBox checkBox = checkBox1;
        bool? isChecked = checkBox1.IsChecked;
        bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
        checkBox.IsChecked = nullable;
        LocalSettings.Settings.RemoveTimeText = ((int) checkBox1.IsChecked ?? 1) != 0;
        LocalSettings.Settings.Save();
      }
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void InitDateParsingSettings()
    {
      this.RemoveDateTextCheckbox.IsChecked = new bool?(LocalSettings.Settings.RemoveTimeText);
      this.DateToggleCheckbox.IsChecked = new bool?(LocalSettings.Settings.DateParsing);
      this.RemoveDateTextCheckbox.Visibility = LocalSettings.Settings.DateParsing ? Visibility.Visible : Visibility.Collapsed;
      this.RemoveDateTextCheckbox.Checked += new RoutedEventHandler(this.RemoveDateTextChecked);
      this.RemoveDateTextCheckbox.Unchecked += new RoutedEventHandler(this.RemoveDateTextChecked);
    }

    private void InitKeepTags()
    {
      this.RemoteTagTextCheckbox.IsChecked = new bool?(!LocalSettings.Settings.KeepTagsInText);
    }

    private void RemoveTagTextClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (sender is CheckBox checkBox1)
      {
        CheckBox checkBox = checkBox1;
        bool? isChecked = checkBox1.IsChecked;
        bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
        checkBox.IsChecked = nullable;
        LocalSettings.Settings.KeepTagsInText = !checkBox1.IsChecked.GetValueOrDefault();
        LocalSettings.Settings.Save();
      }
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void InitParseUrl()
    {
      this.UrlParseCheckbox.IsChecked = new bool?(((int) LocalSettings.Settings.UserPreference?.GeneralConfig?.urlParseEnabled ?? 1) != 0);
    }

    private void UrlParseClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (sender is CheckBox checkBox1)
      {
        CheckBox checkBox = checkBox1;
        bool? isChecked = checkBox1.IsChecked;
        bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
        checkBox.IsChecked = nullable;
        LocalSettings.Settings.SetUrlParseEnable(checkBox1.IsChecked.GetValueOrDefault());
        LocalSettings.Settings.Save();
      }
      SettingsHelper.PushLocalPreference();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/smartrecognitionconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.DateToggleCheckbox = (CheckBox) target;
          this.DateToggleCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.DateParsingClick);
          break;
        case 2:
          this.RemoveDateTextCheckbox = (CheckBox) target;
          this.RemoveDateTextCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.RemoveDateTextClick);
          break;
        case 3:
          this.RemoteTagTextCheckbox = (CheckBox) target;
          this.RemoteTagTextCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.RemoveTagTextClick);
          break;
        case 4:
          this.UrlParseCheckbox = (CheckBox) target;
          this.UrlParseCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.UrlParseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
