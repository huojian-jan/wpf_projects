// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanDisplaySettingWindow
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
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanDisplaySettingWindow : Window, IComponentConnector
  {
    internal CustomSimpleComboBox KanbanSizeComboBox;
    internal CheckBox ShowInputBoxCheckBox;
    internal Grid ShowCheckItemGrid;
    internal CheckBox ShowCheckItemCheckBox;
    private bool _contentLoaded;

    public KanbanDisplaySettingWindow(bool showSetCheckItem)
    {
      this.InitializeComponent();
      if (!showSetCheckItem)
        this.ShowCheckItemGrid.Visibility = Visibility.Collapsed;
      this.InitSettings();
    }

    protected override void OnClosed(EventArgs e)
    {
      this.Owner.Activate();
      LocalSettings.Settings.Save();
      base.OnClosed(e);
    }

    private void InitSettings()
    {
      this.KanbanSizeComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("Small"),
        Utils.GetString("medium"),
        Utils.GetString("Large")
      };
      this.KanbanSizeComboBox.SelectedIndex = LocalSettings.Settings.ExtraSettings.KbSize;
      this.ShowInputBoxCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.KbShowAdd);
      this.ShowCheckItemCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowSubtasks);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnKanbanSizeChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.ExtraSettings.KbSize = Math.Max(0, this.KanbanSizeComboBox.SelectedIndex);
      LocalSettings.Settings.SetChanged();
      ResourceUtils.SetKanbanColumnWidth();
      LocalSettings.Settings.NotifyPropertyChanged("KbSize");
    }

    private void ShowInputBoxClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      LocalSettings.Settings.ExtraSettings.KbShowAdd = !LocalSettings.Settings.ExtraSettings.KbShowAdd;
      this.ShowInputBoxCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.KbShowAdd);
      LocalSettings.Settings.NotifyPropertyChanged("KbShowAdd");
    }

    private void ShowCheckItemClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      LocalSettings.Settings.ShowSubtasks = !LocalSettings.Settings.ShowSubtasks;
      this.ShowCheckItemCheckBox.IsChecked = new bool?(LocalSettings.Settings.ShowSubtasks);
      SettingsHelper.PushLocalSettings();
      UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_checklist");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/kanbandisplaysettingwindow.xaml", UriKind.Relative));
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
          this.KanbanSizeComboBox = (CustomSimpleComboBox) target;
          break;
        case 2:
          this.ShowInputBoxCheckBox = (CheckBox) target;
          this.ShowInputBoxCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowInputBoxClick);
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
