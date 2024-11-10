// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.SmartListViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class SmartListViewModel : BaseViewModel
  {
    public SmartListViewModel(SmartListType type, bool withAuto = true)
    {
      this.Type = type;
      switch (type)
      {
        case SmartListType.Week:
          this.Title = Utils.GetString("Next7Day");
          break;
        case SmartListType.Assign:
          this.Title = Utils.GetString("AssignToMe");
          break;
        case SmartListType.Abandoned:
          this.Title = Utils.GetString("Abandoned");
          break;
        case SmartListType.Tag:
          this.Title = Utils.GetString("tag");
          break;
        default:
          this.Title = Utils.GetString(type.ToString());
          break;
      }
      this.IconData = SpecialListUtils.GetIconBySmartType(type);
      ShowStatus selectedIndex = (ShowStatus) SmartListViewModel.GetSelectedIndex(type);
      string str = Utils.GetString(type == SmartListType.Tag ? "ShowOmitEmptyTags" : "ShowIfNotEmpty");
      if (withAuto)
      {
        int num;
        switch (selectedIndex)
        {
          case ShowStatus.Show:
            num = 0;
            break;
          case ShowStatus.Hide:
            num = 2;
            break;
          default:
            num = 1;
            break;
        }
        this.SelectedItemIndex = num;
        this.ShowStatusItems = new List<string>()
        {
          Utils.GetString("Show"),
          str,
          Utils.GetString("Hide")
        };
      }
      else
      {
        this.SelectedItemIndex = selectedIndex != 0 ? 1 : 0;
        this.ShowStatusItems = new List<string>()
        {
          Utils.GetString("Show"),
          Utils.GetString("Hide")
        };
      }
    }

    public SmartListType Type { get; set; }

    public int SelectedItemIndex { get; set; }

    public string Title { get; set; }

    public Geometry IconData { get; set; }

    public List<string> ShowStatusItems { get; set; }

    private static int GetSelectedIndex(SmartListType type)
    {
      LocalSettings settings = LocalSettings.Settings;
      switch (type)
      {
        case SmartListType.All:
          return settings.SmartListAll;
        case SmartListType.Today:
          return settings.SmartListToday;
        case SmartListType.Tomorrow:
          return settings.SmartListTomorrow;
        case SmartListType.Week:
          return settings.SmartList7Day;
        case SmartListType.Assign:
          return settings.SmartListForMe;
        case SmartListType.Completed:
          return settings.SmartListComplete;
        case SmartListType.Trash:
          return settings.SmartListTrash;
        case SmartListType.Summary:
          return settings.SmartListSummary;
        case SmartListType.Inbox:
          return settings.SmartListInbox;
        case SmartListType.Abandoned:
          return settings.SmartListAbandoned;
        case SmartListType.Tag:
          return settings.SmartListTag;
        case SmartListType.Filter:
          return settings.ShowCustomSmartList;
        default:
          return 0;
      }
    }
  }
}
