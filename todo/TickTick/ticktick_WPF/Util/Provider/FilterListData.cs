// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.FilterListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class FilterListData : SortProjectData
  {
    public FilterListData(FilterModel filter)
    {
      this.Filter = filter;
      this.TaskDefault = FilterViewModel.CalculateTaskDefault(filter.rule);
      this.DefaultProjectModel = this.TaskDefault.ProjectModel;
      this.DefaultTaskDate = this.TaskDefault.DefaultDate;
      this.AddTaskHint = FilterListData.GetTaskHint(this.TaskDefault);
      this.ShowProjectSort = true;
      this.ShowCustomSort = false;
      this.ShowLoadMore = false;
      this.EmptyTitle = Utils.GetString("FilterA");
      this.EmptyContent = Utils.GetString(Parser.GetFilterRuleVersion(this.Filter.rule) > 6 ? "FilterVersionUpdate" : (!Parser.CheckFilterRuleExpired(this.Filter.rule) ? "FilterB1" : "FilterB2"));
      this.EmptyPath = Utils.GetIconData("IcEmptyFilter");
      FilterTaskDefault taskDefault = this.TaskDefault;
      DateTime? defaultDate;
      int num;
      if (taskDefault == null)
      {
        num = 0;
      }
      else
      {
        defaultDate = taskDefault.DefaultDate;
        num = defaultDate.HasValue ? 1 : 0;
      }
      if (num == 0)
        return;
      defaultDate = this.TaskDefault.DefaultDate;
      DateTime dateTime = defaultDate.Value;
      DateTime date1 = dateTime.Date;
      dateTime = DateTime.Today;
      dateTime = dateTime.AddDays(1.0);
      DateTime date2 = dateTime.Date;
      if (!(date1 == date2))
        return;
      this.IgnoreTodaySection = true;
    }

    public FilterModel Filter { get; set; }

    public FilterTaskDefault TaskDefault { get; set; }

    public override bool ShowFilterExpired() => Parser.CheckFilterRuleExpired(this.Filter.rule);

    private static string GetTaskHint(FilterTaskDefault taskDefault)
    {
      if (taskDefault.DefaultTags != null && taskDefault.DefaultTags.Count > 0)
        return taskDefault.DefaultTags.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, tag) => current + "#" + tag + " "));
      string empty = string.Empty;
      string str = string.Empty;
      ProjectModel projectModel = taskDefault.ProjectModel;
      if (taskDefault.ProjectModel != null)
        str = projectModel.Isinbox ? Utils.GetString("Inbox") : projectModel.name;
      DateTime? defaultDate = taskDefault.DefaultDate;
      if (defaultDate.HasValue)
      {
        defaultDate = taskDefault.DefaultDate;
        DateTime date1 = defaultDate.Value.Date;
        DateTime dateTime1 = DateTime.Now;
        DateTime date2 = dateTime1.Date;
        if (date1 == date2)
        {
          empty = Utils.GetString("Today");
        }
        else
        {
          defaultDate = taskDefault.DefaultDate;
          dateTime1 = defaultDate.Value;
          DateTime date3 = dateTime1.Date;
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          DateTime dateTime2 = dateTime1.AddDays(1.0);
          if (date3 == dateTime2)
          {
            empty = Utils.GetString("Tomorrow");
          }
          else
          {
            defaultDate = taskDefault.DefaultDate;
            dateTime1 = defaultDate.Value;
            DateTime date4 = dateTime1.Date;
            dateTime1 = DateTime.Now;
            dateTime1 = dateTime1.Date;
            DateTime dateTime3 = dateTime1.AddDays(-1.0);
            if (date4 == dateTime3)
            {
              empty = Utils.GetString("PublicYesterday");
            }
            else
            {
              defaultDate = taskDefault.DefaultDate;
              dateTime1 = defaultDate.Value;
              DateTime date5 = dateTime1.Date;
              dateTime1 = DateTime.Now;
              DateTime date6 = dateTime1.Date;
              if ((date5 - date6).Days <= 7)
              {
                defaultDate = taskDefault.DefaultDate;
                dateTime1 = defaultDate.Value;
                empty = dateTime1.ToString("ddd");
              }
            }
          }
        }
      }
      if (string.IsNullOrEmpty(str))
        return string.Empty;
      return string.IsNullOrEmpty(empty) ? string.Format(Utils.GetString("CenterAddTaskTextBoxPreviewText"), (object) str, (object) Utils.GetString(projectModel.IsNote ? "Notes" : "Task").ToLower()) : string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) empty, (object) str, (object) Utils.GetString(projectModel.IsNote ? "Notes" : "Task").ToLower());
    }

    public override string GetTitle() => this.Filter.name;

    public override async void SaveSortOption(SortOption sortOption)
    {
      this.Filter.SortOption = sortOption;
      this.Filter.sortType = sortOption.groupBy == "none" ? sortOption.orderBy : sortOption.groupBy;
      await FilterDao.UpdateFilter(this.Filter);
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyCSLDrawingImage") as DrawingImage;
    }
  }
}
