// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.MiniCalendarConfig
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
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class MiniCalendarConfig : UserControl, IComponentConnector
  {
    internal CheckBox MiniCalendarSwitch;
    private bool _contentLoaded;

    public MiniCalendarConfig()
    {
      this.InitializeComponent();
      this.MiniCalendarSwitch.IsChecked = new bool?(LocalSettings.Settings.MiniCalendarEnabled);
    }

    private void MiniCalendarSwitchChanged(object sender, RoutedEventArgs e)
    {
      if (!UserDao.IsPro())
      {
        ProChecker.CheckPro(ProType.MiniCalendar, Window.GetWindow((DependencyObject) this));
        Mouse.Capture((IInputElement) null);
        e.Handled = true;
      }
      else
      {
        MiniCalendarConfig.NotifyCheckBoxChanged(sender, e);
        LocalSettings.Settings.MiniCalendarEnabled = this.MiniCalendarSwitch.IsChecked.GetValueOrDefault();
        LocalSettings.Settings.Save();
      }
    }

    private static void NotifyCheckBoxChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      CheckBox checkBox1 = (CheckBox) sender;
      CheckBox checkBox2 = (CheckBox) sender;
      bool? nullable;
      if (checkBox2 == null)
      {
        nullable = new bool?();
      }
      else
      {
        bool? isChecked = checkBox2.IsChecked;
        nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      }
      checkBox1.IsChecked = nullable;
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/minicalendarconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
      {
        this.MiniCalendarSwitch = (CheckBox) target;
        this.MiniCalendarSwitch.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.MiniCalendarSwitchChanged);
      }
      else
        this._contentLoaded = true;
    }
  }
}
