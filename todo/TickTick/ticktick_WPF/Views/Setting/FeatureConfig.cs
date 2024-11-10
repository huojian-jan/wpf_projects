// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.FeatureConfig
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
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Pomo;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class FeatureConfig : UserControl, IComponentConnector
  {
    internal CheckBox MatrixCheckBox;
    internal CheckBox HabitCheckBox;
    internal TextBlock HabitSetting;
    internal CheckBox FocusCheckBox;
    internal TextBlock FocusSetting;
    private bool _contentLoaded;

    public FeatureConfig()
    {
      this.InitializeComponent();
      this.HabitSetting.Visibility = LocalSettings.Settings.ShowHabit ? Visibility.Visible : Visibility.Collapsed;
      this.FocusSetting.Visibility = LocalSettings.Settings.EnableFocus ? Visibility.Visible : Visibility.Collapsed;
      this.MatrixCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowMatrix);
      this.HabitCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowHabit);
      this.FocusCheckBox.IsChecked = new bool?(LocalSettings.Settings.EnableFocus);
    }

    private void OnSettingTextClick(object sender, MouseButtonEventArgs e)
    {
      if (object.Equals(sender, (object) this.HabitSetting))
      {
        HabitSettingWindow habitSettingWindow = new HabitSettingWindow();
        habitSettingWindow.Owner = Window.GetWindow((DependencyObject) this);
        habitSettingWindow.ShowDialog();
      }
      else
      {
        if (!object.Equals(sender, (object) this.FocusSetting))
          return;
        PomoSettings.ShowInstance(Window.GetWindow((DependencyObject) this));
      }
    }

    protected virtual void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox1))
        return;
      CheckBox checkBox2 = checkBox1;
      bool? isChecked1 = checkBox1.IsChecked;
      bool? nullable = isChecked1.HasValue ? new bool?(!isChecked1.GetValueOrDefault()) : new bool?();
      checkBox2.IsChecked = nullable;
      if (object.Equals(sender, (object) this.HabitCheckBox))
      {
        LocalSettings settings = LocalSettings.Settings;
        bool? isChecked2 = checkBox1.IsChecked;
        int num1 = isChecked2.GetValueOrDefault() ? 1 : 0;
        settings.ShowOrHideTabBar("HABIT", num1 != 0);
        TextBlock habitSetting = this.HabitSetting;
        isChecked2 = this.HabitCheckBox.IsChecked;
        int num2 = isChecked2.GetValueOrDefault() ? 0 : 2;
        habitSetting.Visibility = (Visibility) num2;
      }
      else if (object.Equals(sender, (object) this.FocusCheckBox))
      {
        LocalSettings.Settings.PomoLocalSetting.PomoWindowLeft = -1;
        LocalSettings.Settings.PomoLocalSetting.PomoWindowTop = -1;
        LocalSettings.Settings.ShowOrHideTabBar("POMO", checkBox1.IsChecked.GetValueOrDefault());
        this.FocusSetting.Visibility = this.FocusCheckBox.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
      }
      else
      {
        if (!object.Equals(sender, (object) this.MatrixCheckBox))
          return;
        LocalSettings.Settings.ShowOrHideTabBar("MATRIX", checkBox1.IsChecked.GetValueOrDefault());
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/featureconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.MatrixCheckBox = (CheckBox) target;
          this.MatrixCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          break;
        case 2:
          this.HabitCheckBox = (CheckBox) target;
          this.HabitCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          break;
        case 3:
          this.HabitSetting = (TextBlock) target;
          this.HabitSetting.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSettingTextClick);
          break;
        case 4:
          this.FocusCheckBox = (CheckBox) target;
          this.FocusCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          break;
        case 5:
          this.FocusSetting = (TextBlock) target;
          this.FocusSetting.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSettingTextClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
