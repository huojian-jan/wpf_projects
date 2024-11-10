// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TomorrowListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TomorrowListData : SortProjectData
  {
    public TomorrowListData()
    {
      this.EmptyTitle = Utils.GetString("TomorrowA");
      this.AddTaskHint = string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) Utils.GetString("Tomorrow"), (object) Utils.GetString("Inbox"), (object) Utils.GetString("Task").ToLower());
      this.TitleInProjectGroup = Utils.GetString("Inbox");
      this.DefaultTaskDate = new DateTime?(DateTime.Now.AddDays(1.0));
      this.ShowProjectSort = true;
    }

    public override string GetTitle() => Utils.GetString("Tomorrow");

    public override void SaveSortOption(SortOption sortOption)
    {
      SortProjectData.SaveSpecialProjectSortType("SortTypeOfTomorrow", sortOption);
      LocalSettings.Settings.SaveSmartProjectOptions("tomorrow", sortOption);
      DataChangedNotifier.NotifySortOptionChanged("tomorrow");
    }

    public override string GetEmptyContent()
    {
      return Utils.GetString("TomorrowB" + TomorrowListData.GetEmptyType().ToString());
    }

    public override Thickness GetEmptyMargin()
    {
      return TomorrowListData.GetEmptyType() == 1 ? new Thickness(53.0, 78.0, 0.0, 0.0) : new Thickness(0.0);
    }

    public override DrawingImage GetEmptyImage()
    {
      return TomorrowListData.GetEmptyType() == 1 ? Application.Current?.FindResource((object) "EmptyTomorrow01DrawingImage") as DrawingImage : Application.Current?.FindResource((object) "EmptyTomorrow02DrawingImage") as DrawingImage;
    }

    public override Geometry GetEmptyPath()
    {
      return TomorrowListData.GetEmptyType() == 1 ? Utils.GetIconData("IcTomorrowB1") : Utils.GetIconData("IcTomorrowB2");
    }

    private static int GetEmptyType()
    {
      return HolidayManager.IsWorkDay(DateTime.Today.AddDays(1.0)) ? 1 : 2;
    }
  }
}
