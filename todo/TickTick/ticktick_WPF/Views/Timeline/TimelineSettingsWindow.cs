// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineSettingsWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineSettingsWindow : Window, IComponentConnector
  {
    internal CustomSimpleComboBox TaskBlocksColorComboBox;
    internal CheckBox ShowWeekCheckBox;
    internal Grid ShowCheckItemGrid;
    internal CheckBox ShowCheckItemCheckBox;
    private bool _contentLoaded;

    public TimelineSettingsWindow(TimelineViewModel vm)
    {
      this.InitializeComponent();
      this.DataContext = (object) vm;
      this.ShowWeekCheckBox.IsChecked = new bool?(vm.ShowWeek);
      int colorType = (int) vm.ColorType;
      this.TaskBlocksColorComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("TimelineNoColor"),
        Utils.GetString("lists"),
        Utils.GetString("Tags"),
        Utils.GetString("priority")
      };
      this.TaskBlocksColorComboBox.SelectedIndex = colorType;
      if (!vm.CanShowCheckItem)
        this.ShowCheckItemGrid.Visibility = Visibility.Collapsed;
      this.ShowCheckItemCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowSubtasks);
    }

    protected override void OnClosed(EventArgs e)
    {
      this.Owner.Activate();
      base.OnClosed(e);
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.IsSetting = false;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnTaskBlocksColorChanged(object sender, SimpleComboBoxViewModel e)
    {
      string data = "none";
      switch (this.TaskBlocksColorComboBox.SelectedIndex)
      {
        case 1:
          data = "list";
          break;
        case 2:
          data = "tag";
          break;
        case 3:
          data = "priority";
          break;
      }
      UserActCollectUtils.AddClickEvent("timeline", "view_options", data);
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.ColorType = (TimelineColorType) this.TaskBlocksColorComboBox.SelectedIndex;
    }

    private void ShowWeekClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.ShowWeek = !dataContext.ShowWeek;
      this.ShowWeekCheckBox.IsChecked = new bool?(dataContext.ShowWeek);
    }

    private void ShowCheckItemClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      UserActCollectUtils.AddClickEvent("timeline", "om", "show_hide_completed");
      LocalSettings.Settings.ShowSubtasks = !LocalSettings.Settings.ShowSubtasks;
      LocalSettings.Settings.Save();
      SettingsHelper.PushLocalSettings();
      this.ShowCheckItemCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowSubtasks);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinesettingswindow.xaml", UriKind.Relative));
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
          this.TaskBlocksColorComboBox = (CustomSimpleComboBox) target;
          break;
        case 2:
          this.ShowWeekCheckBox = (CheckBox) target;
          this.ShowWeekCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowWeekClick);
          break;
        case 3:
          this.ShowCheckItemGrid = (Grid) target;
          break;
        case 4:
          this.ShowCheckItemCheckBox = (CheckBox) target;
          this.ShowCheckItemCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowCheckItemClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
