// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryTaskViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryTaskViewModel
  {
    private static readonly Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>> TaskBuilders = new Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>()
    {
      {
        "project",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildProject)
      },
      {
        "dueDate",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTaskDate)
      },
      {
        "tag",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTag)
      },
      {
        "progress",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildProgress)
      },
      {
        "completedTime",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildCompletedDate)
      },
      {
        "focus",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildFocus)
      },
      {
        "title",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTitle)
      },
      {
        "detail",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildContent)
      }
    };
    private static readonly Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>> HabitBuilders = new Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>()
    {
      {
        "title",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildSummaryTitle)
      },
      {
        "dueDate",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTaskDate)
      },
      {
        "project",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildHabitProject)
      },
      {
        "completedTime",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildCompletedDate)
      },
      {
        "progress",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildHabitProgress)
      },
      {
        "detail",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildDesc)
      }
    };
    private static readonly Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>> CalendarBuilders = new Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>()
    {
      {
        "title",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildSummaryTitle)
      },
      {
        "dueDate",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTaskDate)
      },
      {
        "project",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildCalendarProject)
      },
      {
        "completedTime",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildCalendarCompletedDate)
      },
      {
        "detail",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildDesc)
      }
    };
    private static readonly Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>> CheckItemBuilders = new Dictionary<string, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>()
    {
      {
        "title",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildSummaryTitle)
      },
      {
        "dueDate",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildTaskDate)
      },
      {
        "completedTime",
        new Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>(SummaryTaskViewModel.BuildCompletedDate)
      }
    };

    public string EntityId { get; set; }

    public TaskBaseViewModel SourceViewModel { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string Content { get; set; }

    public ProjectModel Project { get; set; }

    public List<string> Tags { get; set; }

    public int Status { get; set; }

    public int? Progress { get; set; }

    public string ProgressText { get; set; }

    public bool? IsAllDay { get; set; }

    public int Priority { get; set; }

    public long SortOrder { get; set; }

    public string GroupName { get; set; }

    public DateTime? GroupDate { get; set; }

    public List<SummaryTaskViewModel> Children { get; set; } = new List<SummaryTaskViewModel>();

    public string Assignee { get; set; }

    public TagModel PrimaryTag { get; set; }

    public string ParentTitle { get; set; }

    public string Title { get; set; }

    public string ColumnName { get; set; }

    public string Desc { get; set; }

    public string Category { get; set; }

    public object Entity { get; set; }

    public int Level { get; set; }

    public List<SummaryDisplayItemModel> DisplayItems { get; set; } = new List<SummaryDisplayItemModel>();

    private static Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> GetCheckItemBuildProcessorByKey(
      string key)
    {
      Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> func;
      return !SummaryTaskViewModel.CheckItemBuilders.TryGetValue(key, out func) ? (Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>) (async (a, b) => await Task.CompletedTask) : func;
    }

    private static Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> GetCalendarBuildProcessorByKey(
      string key)
    {
      Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> func;
      return !SummaryTaskViewModel.CalendarBuilders.TryGetValue(key, out func) ? (Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>) (async (a, b) => await Task.CompletedTask) : func;
    }

    private static Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> GetHabitBuildProcessorByKey(
      string key)
    {
      Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> func;
      return !SummaryTaskViewModel.HabitBuilders.TryGetValue(key, out func) ? (Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>) (async (a, b) => await Task.CompletedTask) : func;
    }

    private static Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> GetTaskBuildProcessorByKey(
      string key)
    {
      Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> func;
      return !SummaryTaskViewModel.TaskBuilders.TryGetValue(key, out func) ? (Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>) (async (a, b) => await Task.CompletedTask) : func;
    }

    private static async Task BuildTaskDate(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowTaskDate)
        return;
      DateTime? startDate = model.StartDate;
      if (!startDate.HasValue)
        return;
      bool withTime = filter.ShowTaskTime && !model.IsAllDay.GetValueOrDefault();
      if (filter.SortBy == SummarySortType.dueDate)
      {
        if (!withTime)
          return;
        SummaryTaskViewModel summaryTaskViewModel = model;
        string content = summaryTaskViewModel.Content;
        startDate = model.StartDate;
        string str = DateUtils.FormatSummaryTimeOnly(startDate.Value);
        summaryTaskViewModel.Content = content + "[" + str + "] ";
      }
      else
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        string content = summaryTaskViewModel.Content;
        startDate = model.StartDate;
        string str = DateUtils.FormatSummaryTime(startDate.Value, withTime);
        summaryTaskViewModel.Content = content + "[" + str + "] ";
      }
    }

    private static async Task BuildTag(SummaryTaskViewModel model, SummaryFilterViewModel filter)
    {
      if (!filter.ShowTag || model.Tags == null || model.Tags.Count <= 0)
        return;
      List<TagModel> tags = CacheManager.GetTags();
      List<string> list = model.Tags.Select<string, TagModel>((Func<string, TagModel>) (name => tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (tag => tag.name.Equals(name, StringComparison.OrdinalIgnoreCase))))).Where<TagModel>((Func<TagModel, bool>) (tag => tag != null)).OrderBy<TagModel, long>((Func<TagModel, long>) (tag => tag.sortOrder)).Select<TagModel, string>((Func<TagModel, string>) (tag => tag.label ?? tag.name)).ToList<string>();
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + string.Join(" ", list.Select<string, string>((Func<string, string>) (tag => "#" + tag))) + " ";
    }

    private static async Task BuildHabitProgress(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowProgress || string.IsNullOrEmpty(model.ProgressText))
        return;
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + "[" + model.ProgressText + "] ";
    }

    private static async Task BuildProgress(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (model.Status != 0 || !filter.ShowProgress)
        return;
      int? progress = model.Progress;
      if (!progress.HasValue)
        return;
      progress = model.Progress;
      int num = 0;
      if (progress.GetValueOrDefault() == num & progress.HasValue)
        return;
      model.Content += string.Format("[{0}%] ", (object) model.Progress);
    }

    private static async Task BuildCalendarCompletedDate(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowCompleteDate)
        return;
      DateTime? completedDate = model.CompletedDate;
      if (!completedDate.HasValue || filter.SortBy == SummarySortType.completedTime)
        return;
      SummaryTaskViewModel summaryTaskViewModel = model;
      string content = summaryTaskViewModel.Content;
      completedDate = model.CompletedDate;
      string str = DateUtils.FormatSummaryTime(completedDate.Value, false);
      summaryTaskViewModel.Content = content + "[" + str + "] ";
    }

    private static async Task BuildCompletedDate(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowCompleteDate)
        return;
      DateTime? completedDate = model.CompletedDate;
      if (!completedDate.HasValue)
        return;
      if (filter.SortBy == SummarySortType.completedTime)
      {
        if (!filter.ShowCompletedTime)
          return;
        SummaryTaskViewModel summaryTaskViewModel = model;
        string content = summaryTaskViewModel.Content;
        completedDate = model.CompletedDate;
        string str = DateUtils.FormatSummaryTimeOnly(completedDate.Value);
        summaryTaskViewModel.Content = content + "[" + str + "] ";
      }
      else
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        string content = summaryTaskViewModel.Content;
        completedDate = model.CompletedDate;
        string str = DateUtils.FormatSummaryTime(completedDate.Value, filter.ShowCompletedTime);
        summaryTaskViewModel.Content = content + "[" + str + "] ";
      }
    }

    private static async Task BuildFocus(SummaryTaskViewModel model, SummaryFilterViewModel filter)
    {
      if (!filter.ShowPomo)
        return;
      PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(model.SourceViewModel?.Id);
      (int num, long duration1, long duration2) = pomoByTaskId != null ? pomoByTaskId.GetPomoFocusSummary() : (0, 0L, 0L);
      if (num <= 0 && duration2 <= 0L)
        return;
      List<string> values = new List<string>();
      if (num > 0)
        values.Add(Utils.GetString("Pomo") + "×" + num.ToString() + " " + Utils.GetDurationString(duration1, true, showSpan: false));
      if (duration2 > 0L)
        values.Add(Utils.GetString("Timing") + ": " + Utils.GetDurationString(duration2, true, showSpan: false));
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + "(" + string.Join(" ", (IEnumerable<string>) values) + ") ";
    }

    private static async Task BuildHabitProject(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowProject)
        return;
      string str = Utils.GetString("statistics_habit");
      string columnName = model.ColumnName;
      if (columnName.StartsWith("_") && (columnName == "_morning" || columnName == "_afternoon" || columnName == "_night"))
        columnName = Utils.GetString("HabitSection" + columnName.Substring(1, 1).ToUpper() + columnName.Substring(2));
      if (filter.SortBy == SummarySortType.project)
      {
        if (!filter.ShowColumn || string.IsNullOrEmpty(columnName))
          return;
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + columnName + "> ";
      }
      else if (filter.ShowColumn && !string.IsNullOrEmpty(columnName))
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + str + "-" + columnName + "> ";
      }
      else
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + str + "> ";
      }
    }

    private static async Task BuildCalendarProject(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowProject)
        return;
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + model.Project.name + "> ";
    }

    private static async Task BuildProject(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowProject || model.Project == null || model.Level != 0)
        return;
      string str1 = model.Project.Isinbox ? Utils.GetString("Inbox") : model.Project.name;
      string str2 = model.SourceViewModel.ColumnName ?? Utils.GetString("NotSectioned");
      if (filter.SortBy == SummarySortType.project)
      {
        if (!filter.ShowColumn || string.IsNullOrEmpty(str2))
          return;
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + str2 + "> ";
      }
      else if (filter.ShowColumn && !string.IsNullOrEmpty(str2))
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + str1 + "-" + str2 + "> ";
      }
      else
      {
        SummaryTaskViewModel summaryTaskViewModel = model;
        summaryTaskViewModel.Content = summaryTaskViewModel.Content + "<" + str1 + "> ";
      }
    }

    private static async Task BuildSummaryTitle(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + model.Title + " ";
    }

    private static async Task BuildTitle(SummaryTaskViewModel model, SummaryFilterViewModel filter)
    {
      TaskBaseViewModel sourceViewModel = model.SourceViewModel;
      string title = sourceViewModel.Title;
      string str = (title != null ? (title.Length >= 300 ? 1 : 0) : 0) != 0 ? sourceViewModel.Title.Substring(0, 300) : sourceViewModel.Title;
      if (filter.ShowParent)
      {
        string parentTitle = model.ParentTitle;
        if (!string.IsNullOrEmpty(parentTitle))
          str = !filter.ParentPrefix ? str + " /" + parentTitle : parentTitle + "/ " + str;
      }
      SummaryTaskViewModel summaryTaskViewModel = model;
      summaryTaskViewModel.Content = summaryTaskViewModel.Content + str + " ";
    }

    private static async Task BuildDesc(SummaryTaskViewModel model, SummaryFilterViewModel filter)
    {
      if (!filter.ShowDetail || string.IsNullOrEmpty(model.Desc))
        return;
      string desc = model.Desc;
      string str1 = !string.IsNullOrEmpty(desc) ? "\r\n    [&tabs]" + desc.Replace("\r", " ").Replace("\n", " ") : "";
      string str2 = str1.Length >= 300 ? str1.Substring(0, 300) : str1;
      model.Content += str2;
    }

    private static async Task BuildContent(
      SummaryTaskViewModel model,
      SummaryFilterViewModel filter)
    {
      if (!filter.ShowDetail)
        return;
      string str1 = "[&tabs]";
      TaskBaseViewModel sourceViewModel = model.SourceViewModel;
      string str2 = "    " + str1;
      if (sourceViewModel.Kind == "TEXT")
      {
        string str3 = TaskUtils.ReplaceAttachmentTextInString(sourceViewModel.Content, false);
        string str4 = !string.IsNullOrEmpty(str3) ? "\r\n" + str2 + str3.Replace("\r", " ").Replace("\n", " ") : "";
        string str5 = str4.Length >= 300 ? str4.Substring(0, 300) : str4;
        model.Content += str5;
      }
      else
      {
        string str6 = !string.IsNullOrEmpty(sourceViewModel.Desc) ? "\r\n" + str2 + sourceViewModel.Desc.Replace("\r", " ").Replace("\n", " ") : "";
        model.Content += str6.Length >= 300 ? str6.Substring(0, 300) : str6;
        List<TaskBaseViewModel> checkItemsByTaskId = TaskDetailItemCache.GetCheckItemsByTaskId(sourceViewModel.Id);
        // ISSUE: explicit non-virtual call
        if (checkItemsByTaskId == null || __nonvirtual (checkItemsByTaskId.Count) <= 0)
          return;
        TaskDetailItemDao.SortItems(checkItemsByTaskId);
        foreach (TaskBaseViewModel taskBaseViewModel in checkItemsByTaskId)
        {
          SummaryTaskViewModel summaryTaskViewModel1 = SummaryTaskViewModel.BuildCheckItem(taskBaseViewModel, filter);
          SummaryTaskViewModel summaryTaskViewModel2 = model;
          summaryTaskViewModel2.Content = summaryTaskViewModel2.Content + "\r\n" + str2 + summaryTaskViewModel1.Content;
        }
      }
    }

    public bool IsHabitModel() => this.GetEntityType() == "habit";

    public bool IsCalendarModel() => this.GetEntityType() == "calendar";

    public bool IsTaskModel() => this.GetEntityType() == "task";

    public string GetEntityType()
    {
      switch (this.Entity)
      {
        case TaskModel _:
          return "task";
        case HabitModel _:
          return "habit";
        case CalendarEventModel _:
          return "calendar";
        default:
          return "task";
      }
    }

    public static SummaryTaskViewModel BuildHabit(
      HabitModel habit,
      HabitCheckInModel habitCheckIn,
      Dictionary<string, HabitRecordModel> habit2Note,
      Dictionary<string, string> sectionId2Name,
      SummaryFilterViewModel filter)
    {
      string str1 = "[&tabs]";
      SummaryTaskViewModel summaryTaskViewModel1 = new SummaryTaskViewModel()
      {
        Progress = new int?((int) (habitCheckIn.Value * 1.0 / habit.Goal * 1.0 * 100.0))
      };
      if (habitCheckIn.Value > 0.0)
      {
        int? progress = summaryTaskViewModel1.Progress;
        int num = 0;
        if (progress.GetValueOrDefault() == num & progress.HasValue)
          summaryTaskViewModel1.Progress = new int?(1);
      }
      summaryTaskViewModel1.Entity = (object) habit;
      switch (habitCheckIn.CheckStatus)
      {
        case 1:
          summaryTaskViewModel1.Status = -1;
          break;
        case 2:
          if (habitCheckIn.Value >= habitCheckIn.Goal)
          {
            summaryTaskViewModel1.Status = 2;
            break;
          }
          break;
        default:
          summaryTaskViewModel1.Status = 0;
          break;
      }
      summaryTaskViewModel1.StartDate = new DateTime?(DateTime.ParseExact(habitCheckIn.CheckinStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
      summaryTaskViewModel1.IsAllDay = new bool?(true);
      summaryTaskViewModel1.DueDate = new DateTime?();
      summaryTaskViewModel1.CompletedDate = new DateTime?();
      DateTime? nullable1;
      if (habit.Reminders != null && habit.Reminders.Length != 0)
      {
        nullable1 = summaryTaskViewModel1.StartDate;
        DateTime today = DateTime.Today;
        if ((nullable1.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == today ? 1 : 0) : 1) : 0) != 0)
        {
          summaryTaskViewModel1.IsAllDay = new bool?(false);
          summaryTaskViewModel1.StartDate = new DateTime?(DateTime.Today);
          summaryTaskViewModel1.Entity = (object) habit;
          HabitModel habit1 = habit;
          nullable1 = summaryTaskViewModel1.StartDate;
          DateTime startDate = nullable1.Value;
          nullable1 = new DateTime?();
          DateTime? completeTime = nullable1;
          CalendarDisplayModel calendarDisplayModel = CalendarDisplayModel.Build(habit1, 0, startDate, completeTime);
          summaryTaskViewModel1.StartDate = calendarDisplayModel.StartDate;
          summaryTaskViewModel1.IsAllDay = calendarDisplayModel.IsAllDay;
        }
      }
      SummaryTaskViewModel summaryTaskViewModel2 = summaryTaskViewModel1;
      HabitBaseViewModel habitBaseViewModel = new HabitBaseViewModel(habit);
      habitBaseViewModel.StartDate = summaryTaskViewModel1.StartDate;
      nullable1 = new DateTime?();
      habitBaseViewModel.DueDate = nullable1;
      habitBaseViewModel.IsAllDay = summaryTaskViewModel1.IsAllDay;
      summaryTaskViewModel2.SourceViewModel = (TaskBaseViewModel) habitBaseViewModel;
      string key = habitCheckIn.HabitId + "_" + habitCheckIn.CheckinStamp;
      HabitRecordModel habitRecordModel;
      if (habit2Note.TryGetValue(key, out habitRecordModel))
        summaryTaskViewModel1.Desc = habitRecordModel.Content;
      if (habitCheckIn.IsComplete() || habitCheckIn.IsUnComplete())
      {
        nullable1 = habitCheckIn.CheckinTime;
        if (nullable1.HasValue)
        {
          SummaryTaskViewModel summaryTaskViewModel3 = summaryTaskViewModel1;
          nullable1 = habitCheckIn.CheckinTime;
          DateTime? nullable2 = new DateTime?(nullable1.Value);
          summaryTaskViewModel3.CompletedDate = nullable2;
        }
      }
      double d = habitCheckIn.Value;
      if (string.Equals(habit.Type.ToLower(), "real") && d > 0.0)
        summaryTaskViewModel1.ProgressText = Math.Abs(d - Math.Floor(d)) <= 1E-06 ? string.Format("{0}/{1}", (object) (int) d, (object) (int) habitCheckIn.Goal) : string.Format("{0:F2}/{1:F2}", (object) d, (object) habitCheckIn.Goal);
      summaryTaskViewModel1.Priority = 0;
      summaryTaskViewModel1.Title = habit.Name;
      summaryTaskViewModel1.Content = "\r\n" + str1;
      summaryTaskViewModel1.Project = new ProjectModel()
      {
        id = nameof (habit),
        name = Utils.GetString("statistics_habit"),
        sortOrder = long.MaxValue
      };
      string str2 = "- ";
      if (filter.ShowStatus)
        str2 = habitCheckIn.IsComplete() || habitCheckIn.IsUnComplete() ? "- [x] " : "- [ ] ";
      summaryTaskViewModel1.Content += str2;
      summaryTaskViewModel1.ColumnName = Utils.GetString("Others");
      string str3;
      if (sectionId2Name != null && sectionId2Name.TryGetValue(habit.SectionId ?? "", out str3))
        summaryTaskViewModel1.ColumnName = str3;
      foreach (SummaryDisplayItemViewModel displayItemViewModel in filter.DisplayItems.OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (item => item.SortOrder)).ToList<SummaryDisplayItemViewModel>())
      {
        Task task = SummaryTaskViewModel.GetHabitBuildProcessorByKey(displayItemViewModel.Key)(summaryTaskViewModel1, filter);
      }
      return summaryTaskViewModel1;
    }

    public static SummaryTaskViewModel BuildHabit(
      HabitModel habit,
      Dictionary<string, string> sectionId2Name,
      SummaryFilterViewModel filter)
    {
      string str1 = "[&tabs]";
      SummaryTaskViewModel summaryTaskViewModel1 = new SummaryTaskViewModel();
      summaryTaskViewModel1.StartDate = new DateTime?(DateTime.Today);
      summaryTaskViewModel1.Entity = (object) habit;
      HabitModel habit1 = habit;
      DateTime? nullable = summaryTaskViewModel1.StartDate;
      DateTime startDate = nullable.Value;
      nullable = new DateTime?();
      DateTime? completeTime = nullable;
      CalendarDisplayModel calendarDisplayModel = CalendarDisplayModel.Build(habit1, 0, startDate, completeTime);
      summaryTaskViewModel1.StartDate = calendarDisplayModel.StartDate;
      summaryTaskViewModel1.IsAllDay = calendarDisplayModel.IsAllDay;
      SummaryTaskViewModel summaryTaskViewModel2 = summaryTaskViewModel1;
      HabitBaseViewModel habitBaseViewModel = new HabitBaseViewModel(habit);
      habitBaseViewModel.StartDate = summaryTaskViewModel1.StartDate;
      habitBaseViewModel.DueDate = new DateTime?();
      habitBaseViewModel.IsAllDay = summaryTaskViewModel1.IsAllDay;
      summaryTaskViewModel2.SourceViewModel = (TaskBaseViewModel) habitBaseViewModel;
      summaryTaskViewModel1.DueDate = new DateTime?();
      summaryTaskViewModel1.CompletedDate = new DateTime?();
      summaryTaskViewModel1.Priority = 0;
      summaryTaskViewModel1.Title = habit.Name;
      summaryTaskViewModel1.Content = "\r\n" + str1;
      summaryTaskViewModel1.Project = new ProjectModel()
      {
        id = nameof (habit),
        name = Utils.GetString("statistics_habit"),
        sortOrder = long.MaxValue
      };
      string str2 = "- ";
      if (filter.ShowStatus)
        str2 = "- [ ] ";
      summaryTaskViewModel1.Content += str2;
      summaryTaskViewModel1.ColumnName = Utils.GetString("Others");
      string str3;
      if (sectionId2Name != null && sectionId2Name.TryGetValue(habit.SectionId ?? "", out str3))
        summaryTaskViewModel1.ColumnName = str3;
      foreach (SummaryDisplayItemViewModel displayItemViewModel in filter.DisplayItems.OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (item => item.SortOrder)).ToList<SummaryDisplayItemViewModel>())
      {
        Task task = SummaryTaskViewModel.GetHabitBuildProcessorByKey(displayItemViewModel.Key)(summaryTaskViewModel1, filter);
      }
      return summaryTaskViewModel1;
    }

    public static SummaryTaskViewModel BuildCheckItem(
      TaskBaseViewModel item,
      SummaryFilterViewModel filter)
    {
      SummaryTaskViewModel summaryTaskViewModel = new SummaryTaskViewModel()
      {
        Entity = (object) item,
        Title = item.Title,
        StartDate = item.StartDate,
        IsAllDay = item.IsAllDay,
        CompletedDate = item.CompletedTime,
        Status = item.Status
      };
      string str = "- ";
      if (filter.ShowStatus)
        str = summaryTaskViewModel.Status == 0 ? "- [ ] " : "- [x] ";
      summaryTaskViewModel.Content += str;
      foreach (SummaryDisplayItemViewModel displayItemViewModel in filter.DisplayItems.OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (it => item.SortOrder)).ToList<SummaryDisplayItemViewModel>())
      {
        Task task = SummaryTaskViewModel.GetCheckItemBuildProcessorByKey(displayItemViewModel.Key)(summaryTaskViewModel, filter);
      }
      return summaryTaskViewModel;
    }

    public static SummaryTaskViewModel BuildCalendar(
      CalendarDisplayModel calendarEvent,
      Dictionary<string, string> id2Name,
      SummaryFilterViewModel filter)
    {
      string str1 = "[&tabs]";
      SummaryTaskViewModel summaryTaskViewModel1 = new SummaryTaskViewModel()
      {
        Entity = (object) calendarEvent,
        StartDate = calendarEvent.StartDate,
        DueDate = calendarEvent.DueDate,
        Desc = calendarEvent.SourceViewModel?.Desc,
        IsAllDay = calendarEvent.IsAllDay,
        Priority = 0,
        Title = calendarEvent.Title,
        Content = "\r\n" + str1
      };
      summaryTaskViewModel1.SourceViewModel = new TaskBaseViewModel()
      {
        StartDate = summaryTaskViewModel1.StartDate,
        DueDate = summaryTaskViewModel1.DueDate,
        IsAllDay = summaryTaskViewModel1.IsAllDay
      };
      string str2;
      string str3 = id2Name.TryGetValue(calendarEvent.SourceViewModel?.CalendarId ?? "", out str2) ? str2 : Utils.GetString("Calendar");
      summaryTaskViewModel1.Project = new ProjectModel()
      {
        id = "calendar",
        name = str3,
        sortOrder = 9223372036854775806L
      };
      DateTime? nullable1 = calendarEvent.DueDate;
      DateTime today;
      if (nullable1.HasValue)
      {
        nullable1 = calendarEvent.DueDate;
        today = nullable1.Value;
        if (today.Date < DateTime.Today)
          goto label_6;
      }
      nullable1 = calendarEvent.DueDate;
      if (!nullable1.HasValue)
      {
        nullable1 = calendarEvent.StartDate;
        if (nullable1.HasValue)
        {
          nullable1 = calendarEvent.StartDate;
          today = DateTime.Today;
          if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
            goto label_6;
        }
      }
      if (calendarEvent.Status != 2)
      {
        summaryTaskViewModel1.Status = 0;
        goto label_11;
      }
label_6:
      summaryTaskViewModel1.Status = 2;
      nullable1 = calendarEvent.DueDate;
      if (nullable1.HasValue)
      {
        SummaryTaskViewModel summaryTaskViewModel2 = summaryTaskViewModel1;
        nullable1 = calendarEvent.DueDate;
        today = nullable1.Value;
        DateTime? nullable2 = new DateTime?(today.Date);
        summaryTaskViewModel2.CompletedDate = nullable2;
      }
      else
      {
        nullable1 = calendarEvent.StartDate;
        if (nullable1.HasValue)
        {
          SummaryTaskViewModel summaryTaskViewModel3 = summaryTaskViewModel1;
          nullable1 = calendarEvent.StartDate;
          today = nullable1.Value;
          DateTime? nullable3 = new DateTime?(today.Date);
          summaryTaskViewModel3.CompletedDate = nullable3;
        }
      }
label_11:
      string str4 = "- ";
      if (filter.ShowStatus)
        str4 = summaryTaskViewModel1.Status == 0 ? "- [ ] " : "- [x] ";
      summaryTaskViewModel1.Content += str4;
      foreach (SummaryDisplayItemViewModel displayItemViewModel in filter.DisplayItems.OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (item => item.SortOrder)).ToList<SummaryDisplayItemViewModel>())
      {
        Task task = SummaryTaskViewModel.GetCalendarBuildProcessorByKey(displayItemViewModel.Key)(summaryTaskViewModel1, filter);
      }
      return summaryTaskViewModel1;
    }

    public static async Task<SummaryTaskViewModel> Build(
      Node<TaskBaseViewModel> node,
      SummaryFilterViewModel filter,
      int level = 0)
    {
      string tabs = "[&tabs]";
      for (int index = 0; index < level; ++index)
        tabs += "    ";
      TaskBaseViewModel task = node.Value;
      SummaryTaskViewModel model = new SummaryTaskViewModel();
      model.Level = level;
      model.SourceViewModel = task;
      model.Entity = (object) task;
      model.StartDate = task.StartDate;
      model.DueDate = task.DueDate;
      model.CompletedDate = task.CompletedTime;
      model.Status = task.Status;
      model.Priority = task.Priority;
      model.Progress = new int?(task.Progress);
      model.SortOrder = task.SortOrder;
      model.IsAllDay = task.IsAllDay;
      model.Assignee = task.Assignee;
      SummaryTaskViewModel summaryTaskViewModel1 = model;
      string[] tags = task.Tags;
      List<string> list = tags != null ? ((IEnumerable<string>) tags).ToList<string>() : (List<string>) null;
      summaryTaskViewModel1.Tags = list;
      if (level == 0 && !string.IsNullOrEmpty(task.ParentId))
      {
        TaskModel taskById = await TaskDao.GetTaskById(task.ParentId);
        if (taskById != null)
          model.ParentTitle = taskById.title;
      }
      model.Project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.ProjectId)) ?? CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.Isinbox)) ?? new ProjectModel();
      if (model.Project.Isinbox)
        model.Project.sortOrder = long.MinValue;
      model.Content = "\r\n" + tabs;
      string str = "- ";
      if (filter.ShowStatus)
        str = model.Status == 0 ? "- [ ] " : "- [x] ";
      model.Content += str;
      foreach (Func<SummaryTaskViewModel, SummaryFilterViewModel, Task> func in filter.DisplayItems.OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (item => item.SortOrder)).ToList<SummaryDisplayItemViewModel>().Select<SummaryDisplayItemViewModel, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>((Func<SummaryDisplayItemViewModel, Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>) (item => SummaryTaskViewModel.GetTaskBuildProcessorByKey(item.Key))).Where<Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>>((Func<Func<SummaryTaskViewModel, SummaryFilterViewModel, Task>, bool>) (taskBuilder => taskBuilder != null)))
        await func(model, filter);
      if (node.Children != null)
      {
        foreach (Node<TaskBaseViewModel> child in node.Children)
          model.Children.Add(await SummaryTaskViewModel.Build(child, filter, level + 1));
      }
      SummaryTaskViewModel summaryTaskViewModel2 = model;
      tabs = (string) null;
      model = (SummaryTaskViewModel) null;
      return summaryTaskViewModel2;
    }

    public string GetContent(bool withSpace)
    {
      return this.Content.Replace("[&tabs]", withSpace ? "    " : "");
    }
  }
}
