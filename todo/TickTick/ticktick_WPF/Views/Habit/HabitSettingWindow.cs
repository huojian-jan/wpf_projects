// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitSettingWindow
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitSettingWindow : Window, IComponentConnector
  {
    internal StackPanel HabitConfig;
    internal CheckBox ShowInTodCheckBox;
    private bool _contentLoaded;

    public HabitSettingWindow()
    {
      this.InitializeComponent();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      this.Closing += (CancelEventHandler) ((sender, e) => this.Owner?.Activate());
    }

    private void ShowInTodayClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      CheckBox showInTodCheckBox = this.ShowInTodCheckBox;
      bool? isChecked = this.ShowInTodCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showInTodCheckBox.IsChecked = nullable;
      UserActCollectUtils.AddClickEvent("habit", "settings", this.ShowInTodCheckBox.IsChecked.GetValueOrDefault() ? "enable_showintoday" : "disable_showindtoday");
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      LocalSettings.Settings.HabitInToday = this.ShowInTodCheckBox.IsChecked.GetValueOrDefault();
      HabitService.PushHabitSettings();
      this.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitsettingwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.HabitConfig = (StackPanel) target;
          break;
        case 2:
          this.ShowInTodCheckBox = (CheckBox) target;
          this.ShowInTodCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowInTodayClick);
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
