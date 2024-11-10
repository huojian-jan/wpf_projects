// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SettingViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SettingViewModel : BaseViewModel
  {
    public SettingViewModel(SettingsType settingsType)
    {
      ObservableCollection<SettingMenuItem> observableCollection = new ObservableCollection<SettingMenuItem>();
      observableCollection.Add(new SettingMenuItem("Account1", SettingsType.Account));
      observableCollection.Add(new SettingMenuItem("Premium", SettingsType.Premium));
      observableCollection.Add(new SettingMenuItem());
      observableCollection.Add(new SettingMenuItem("FeatureModule", SettingsType.Feature)
      {
        IsSelected = settingsType == SettingsType.Feature
      });
      observableCollection.Add(new SettingMenuItem("SmartList", SettingsType.SmartList));
      observableCollection.Add(new SettingMenuItem("Notification", SettingsType.Notification));
      observableCollection.Add(new SettingMenuItem("DateAndTime", SettingsType.DateTime));
      observableCollection.Add(new SettingMenuItem("Appearance", SettingsType.Theme)
      {
        IsSelected = settingsType == SettingsType.Theme
      });
      observableCollection.Add(new SettingMenuItem("MoreSettings", SettingsType.More)
      {
        IsSelected = settingsType == SettingsType.More
      });
      observableCollection.Add(new SettingMenuItem());
      observableCollection.Add(new SettingMenuItem("CalendarSubscription", SettingsType.Calendar)
      {
        IsSelected = settingsType == SettingsType.Calendar
      });
      observableCollection.Add(new SettingMenuItem("Share", SettingsType.Share));
      observableCollection.Add(new SettingMenuItem("StickyNote", SettingsType.StickyNote));
      observableCollection.Add(new SettingMenuItem("DesktopWidgets", SettingsType.Widget));
      observableCollection.Add(new SettingMenuItem("Shortcut", SettingsType.Shortcuts));
      observableCollection.Add(new SettingMenuItem());
      observableCollection.Add(new SettingMenuItem("About", SettingsType.About));
      this.MenuTitleList = observableCollection;
    }

    public ObservableCollection<SettingMenuItem> MenuTitleList { get; set; }

    public static void ExpandSubItems(IList<SettingMenuItem> data, SettingMenuItem model)
    {
      int index1 = data.IndexOf(model) + 1;
      for (int index2 = model.Children.Count - 1; index2 >= 0; --index2)
      {
        SettingMenuItem child = model.Children[index2];
        data.Insert(index1, child);
      }
    }

    public static void RemoveSubItems(ObservableCollection<SettingMenuItem> data)
    {
      SettingMenuItem settingMenuItem = data.FirstOrDefault<SettingMenuItem>((Func<SettingMenuItem, bool>) (item => item.Type == SettingsType.Preferences));
      if (settingMenuItem == null)
        return;
      settingMenuItem.Expanded = false;
      foreach (SettingMenuItem child in settingMenuItem.Children)
        data.Remove(child);
    }
  }
}
