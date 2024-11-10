// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ShareConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.Team;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class ShareConfig : UserControl, IComponentConnector
  {
    internal CheckBox AutoAcceptCheckbox;
    internal TeamNotification NotificationSetting;
    private bool _contentLoaded;

    public ShareConfig()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.InitCheckBox();
      this.InitCheckEvent();
      this.AutoAcceptCheckbox.IsChecked = new bool?(LocalSettings.Settings.AutoAcceptShare);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void InitCheckBox()
    {
      string notificationOptions = LocalSettings.Settings.NotificationOptions;
      string[] strArray;
      if (notificationOptions == null)
        strArray = (string[]) null;
      else
        strArray = notificationOptions.Split(';');
      string[] source = strArray;
      if (source == null || source.Length == 0)
        return;
      this.NotificationSetting.DoOrUndo.IsChecked = new bool?(((IEnumerable<string>) source).Contains<string>("DONE") || ((IEnumerable<string>) source).Contains<string>("UNDONE"));
      this.NotificationSetting.Create.IsChecked = new bool?(((IEnumerable<string>) source).Contains<string>("CREATE"));
      this.NotificationSetting.Delete.IsChecked = new bool?(((IEnumerable<string>) source).Contains<string>("DELETE") || ((IEnumerable<string>) source).Contains<string>("MOVE_OUT"));
    }

    private void InitCheckEvent()
    {
      this.NotificationSetting.DoCheck += new EventHandler<bool>(this.OnDoCheck);
      this.NotificationSetting.CreateCheck += new EventHandler<bool>(this.OnCreateCheck);
      this.NotificationSetting.DelCheck += new EventHandler<bool>(this.OnDelCheck);
    }

    private void OnDelCheck(object sender, bool Checked)
    {
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.NotificationOptions.Split(';')).ToList<string>();
      list.Remove("");
      list.Remove("DELETE");
      list.Remove("MOVE_OUT");
      if (Checked)
      {
        list.Add("DELETE");
        list.Add("MOVE_OUT");
      }
      else
      {
        list.Remove("DELETE");
        list.Remove("MOVE_OUT");
      }
      this.SaveReminderConfig(list);
    }

    private void OnCreateCheck(object sender, bool Checked)
    {
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.NotificationOptions.Split(';')).ToList<string>();
      list.Remove("");
      list.Remove("CREATE");
      if (Checked)
        list.Add("CREATE");
      else
        list.Remove("CREATE");
      this.SaveReminderConfig(list);
    }

    private void OnDoCheck(object sender, bool Checked)
    {
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.NotificationOptions.Split(';')).ToList<string>();
      list.Remove("");
      list.Remove("DONE");
      list.Remove("UNDONE");
      if (Checked)
      {
        list.Add("DONE");
        list.Add("UNDONE");
      }
      else
      {
        list.Remove("DONE");
        list.Remove("UNDONE");
      }
      this.SaveReminderConfig(list);
    }

    private void SaveReminderConfig(List<string> notificationOptions)
    {
      LocalSettings.Settings.NotificationOptions = ((IEnumerable<string>) notificationOptions.ToArray()).Join<string>(";");
      LocalSettings.Settings.Save();
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void AutoAcceptClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this.AutoAcceptCheckbox.IsChecked = new bool?(!this.AutoAcceptCheckbox.IsChecked.GetValueOrDefault());
      LocalSettings.Settings.SetAutoAcceptShare(this.AutoAcceptCheckbox.IsChecked.GetValueOrDefault());
      SettingsHelper.PushLocalPreference();
      UserActCollectUtils.AddClickEvent("collaborate", "auto_accept_invites", this.AutoAcceptCheckbox.IsChecked.GetValueOrDefault() ? "enable" : "disable");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/shareconfig.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.NotificationSetting = (TeamNotification) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.AutoAcceptCheckbox = (CheckBox) target;
        this.AutoAcceptCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.AutoAcceptClick);
      }
    }
  }
}
