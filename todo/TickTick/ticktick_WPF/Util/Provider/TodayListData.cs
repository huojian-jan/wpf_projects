// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TodayListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TodayListData : SortProjectData
  {
    public TodayListData()
    {
      this.AddTaskHint = string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) Utils.GetString("Today"), (object) Utils.GetString("Inbox"), (object) Utils.GetString("Task").ToLower());
      this.TitleInProjectGroup = Utils.GetString("Inbox");
      this.DefaultTaskDate = new DateTime?(DateTime.Now);
      this.ShowProjectSort = true;
    }

    public override void SaveSortOption(SortOption sortOption)
    {
      SortProjectData.SaveSpecialProjectSortType("SortTypeOfToday", sortOption);
      LocalSettings.Settings.SaveSmartProjectOptions("today", sortOption);
      DataChangedNotifier.NotifySortOptionChanged("today");
    }

    public override string GetTitle() => Utils.GetString("Today");

    public override async Task<string> GetEmptyTitle()
    {
      if (LocalSettings.Settings.HideComplete)
      {
        if ((await TaskDisplayService.GetDisplayTaskInToday()).Count > 0)
          return Utils.GetString("TodayA2");
      }
      return Utils.GetString("TodayA1");
    }

    public override string GetEmptyContent()
    {
      return Utils.GetString("TodayB" + TodayListData.GetEmptyType().ToString());
    }

    public override DrawingImage GetEmptyImage()
    {
      switch (TodayListData.GetEmptyType())
      {
        case 1:
        case 2:
          return Application.Current?.FindResource((object) "EmptyToday01DrawingImage") as DrawingImage;
        case 3:
          return Application.Current?.FindResource((object) "EmptyToday02DrawingImage") as DrawingImage;
        case 4:
        case 5:
          return Application.Current?.FindResource((object) "EmptyToday03DrawingImage") as DrawingImage;
        case 6:
          return Application.Current?.FindResource((object) "EmptyToday04DrawingImage") as DrawingImage;
        default:
          return Application.Current?.FindResource((object) "EmptyToday01DrawingImage") as DrawingImage;
      }
    }

    public override Geometry GetEmptyPath()
    {
      switch (TodayListData.GetEmptyType())
      {
        case 1:
        case 2:
          return Utils.GetIconData("IcTodayB1B2");
        case 3:
          return Utils.GetIconData("IcTodayB3");
        case 4:
        case 5:
          return Utils.GetIconData("IcTodayB4B5");
        case 6:
          return Utils.GetIconData("IcTodayB6");
        default:
          return Utils.GetIconData("IcTodayB1B2");
      }
    }

    private static int GetEmptyType()
    {
      int num = HolidayManager.IsNonWorkWeekend(DateTime.Today) ? 1 : 0;
      int hour = DateTime.Now.Hour;
      if (num != 0 && hour >= 5 && hour < 18)
        return 6;
      if (hour >= 5 && hour < 12)
        return 1;
      if (hour >= 12 && hour < 13)
        return 2;
      if (hour >= 13 && hour < 18)
        return 3;
      if (hour >= 18 && hour < 23)
        return 4;
      return hour >= 23 || hour < 5 ? 5 : 1;
    }
  }
}
