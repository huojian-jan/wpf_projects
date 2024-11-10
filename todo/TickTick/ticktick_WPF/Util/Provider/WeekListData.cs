// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.WeekListData
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

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class WeekListData : SortProjectData
  {
    public WeekListData()
    {
      this.EmptyTitle = Utils.GetString("Next7DaysA");
      this.EmptyContent = Utils.GetString("Next7DaysB");
      this.EmptyPath = Utils.GetIconData("IcNext7Days");
      this.AddTaskHint = string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) Utils.GetString("Tomorrow"), (object) Utils.GetString("Inbox"), (object) Utils.GetString("Task").ToLower());
      this.TitleInProjectGroup = Utils.GetString("Inbox");
      this.DefaultTaskDate = new DateTime?(DateTime.Now);
      this.ShowProjectSort = true;
    }

    public override string GetTitle() => Utils.GetString("Next7Day");

    public override void SaveSortOption(SortOption sortOption)
    {
      SortProjectData.SaveSpecialProjectSortType("sortTypeOfWeek", sortOption);
      LocalSettings.Settings.SaveSmartProjectOptions("week", sortOption);
      DataChangedNotifier.NotifySortOptionChanged("n7ds");
    }

    public virtual async Task<bool> IsTaskInProject(TaskModel task)
    {
      int num;
      if (task.startDate.HasValue)
      {
        DateTime now = task.startDate.Value;
        DateTime date1 = now.Date;
        now = DateTime.Now;
        DateTime date2 = now.AddDays(6.0).Date;
        num = date1 <= date2 ? 1 : 0;
      }
      else
        num = 0;
      return num != 0;
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyNext7DayDrawingImage") as DrawingImage;
    }
  }
}
