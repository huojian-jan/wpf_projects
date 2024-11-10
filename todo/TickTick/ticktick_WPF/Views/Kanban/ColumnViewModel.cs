// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.ColumnViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class ColumnViewModel : BaseViewModel
  {
    private bool _canDrop;
    private bool _dragging;
    private bool _isPinned;
    public const string ColumnNoDate = "date:no";
    public const string ColumnOutDate = "date:-1";
    public const string ColumnToday = "date:0";
    public const string ColumnTomorrow = "date:1";
    public const string ColumnWeek = "date:week";
    public const string ColumnLater = "date:later";
    public const string ColumnDate = "date:";
    public const string ColumnPriority = "priority:";
    public const string ColumnTag = "tag:";
    public const string ColumnNoTag = "tag:#notag";
    public const string ColumnAssign = "assign:";
    public const string ColumnNoAssign = "assign:-1";
    public const string ColumnProject = "project:";
    public const string ColumnCalendar = "calendar";
    public const string ColumnCourse = "course";
    public const string ColumnHabit = "habit";
    public const string ColumnNote = "note";
    private string _name;
    private int _taskCount;
    private TaskListViewModel _taskListViewModel;
    public List<TaskBaseViewModel> SourceItems = new List<TaskBaseViewModel>();

    public ColumnViewModel()
    {
    }

    public ColumnViewModel(ColumnModel model, int ordinal)
      : this(model)
    {
    }

    public ColumnViewModel(ColumnModel model)
    {
      this.Name = model.name;
      this.ColumnId = model.id;
      this.SortOrder = model.sortOrder.GetValueOrDefault();
    }

    public long SortOrder { get; set; }

    public string ColumnId { get; set; }

    public string ProjectId => this.Identity.GetProjectId();

    public bool NewAdd { get; set; }

    public bool Enable { get; set; }

    public bool Editing { get; set; }

    public bool IsPinned
    {
      get => this._isPinned;
      set
      {
        this._isPinned = value;
        this.OnPropertyChanged(nameof (IsPinned));
      }
    }

    public ColumnProjectIdentity Identity { get; set; }

    public bool CanDropDown { get; set; } = true;

    public bool CanDrop
    {
      get => this._canDrop;
      set
      {
        bool flag = value && this.CanDropDown;
        if (this._canDrop == flag)
          return;
        this._canDrop = flag;
        this.OnPropertyChanged(nameof (CanDrop));
      }
    }

    public bool Dragging
    {
      get => this._dragging;
      set
      {
        this._dragging = value;
        this.OnPropertyChanged(nameof (Dragging));
      }
    }

    public string Name
    {
      get => this._name;
      set
      {
        this._name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public int TaskCount
    {
      get => this._taskCount;
      set
      {
        this._taskCount = value;
        this.OnPropertyChanged(nameof (TaskCount));
      }
    }

    public bool MouseOver { get; set; }

    public bool CanAdd { get; set; } = true;

    public ColumnViewModel Clone() => (ColumnViewModel) this.MemberwiseClone();

    public void NotifyReload() => this.OnPropertyChanged("Reload");

    public static List<ColumnViewModel> GetPriorityColumnViewModels()
    {
      return new List<ColumnViewModel>()
      {
        new ColumnViewModel()
        {
          Name = Utils.GetString("PriorityHigh"),
          SortOrder = 0L,
          ColumnId = "priority:" + 5.ToString()
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("PriorityMedium"),
          SortOrder = 1L,
          ColumnId = "priority:" + 3.ToString()
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("PriorityLow"),
          SortOrder = 2L,
          ColumnId = "priority:" + 1.ToString()
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("PriorityNull"),
          SortOrder = 3L,
          ColumnId = "priority:" + 0.ToString()
        }
      };
    }

    public static List<ColumnViewModel> GetWeekDateColumnViewModels()
    {
      DateTime dateTime = Utils.GetWeekStart(DateTime.Today).AddDays(7.0);
      List<ColumnViewModel> columnViewModels = new List<ColumnViewModel>()
      {
        new ColumnViewModel()
        {
          Name = Utils.GetString("overdue"),
          SortOrder = LocalSettings.Settings.PosOfOverdue == 0 ? 0L : 100L,
          ColumnId = "date:-1"
        },
        new ColumnViewModel()
        {
          Name = string.Format(Utils.GetString("7DayThisWeek"), (object) Utils.GetString("Today"), (object) DateTime.Today.ToString("ddd", (IFormatProvider) App.Ci)),
          SortOrder = 1L,
          ColumnId = "date:0"
        },
        new ColumnViewModel()
        {
          Name = string.Format(Utils.GetString("7DayThisWeek"), (object) Utils.GetString("Tomorrow"), (object) DateTime.Today.AddDays(1.0).ToString("ddd", (IFormatProvider) App.Ci)),
          SortOrder = 4L,
          ColumnId = "date:1"
        }
      };
      for (int index = 2; index < 7; ++index)
      {
        DateTime date = DateTime.Today.AddDays((double) index);
        columnViewModels.Add(new ColumnViewModel()
        {
          Name = string.Format(date >= dateTime ? Utils.GetString("7DayTodayNextWeek") : Utils.GetString("7DayThisWeek"), (object) DateUtils.FormatShortDate(date), (object) DateUtils.FormatWeekDayName(date)),
          SortOrder = (long) (3 + index),
          ColumnId = "date:" + index.ToString()
        });
      }
      return columnViewModels;
    }

    public static List<ColumnViewModel> GetDateColumnViewModels()
    {
      return new List<ColumnViewModel>()
      {
        new ColumnViewModel()
        {
          Name = Utils.GetString("overdue"),
          SortOrder = LocalSettings.Settings.PosOfOverdue == 0 ? 0L : 100L,
          ColumnId = "date:-1"
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("Today"),
          SortOrder = 1L,
          ColumnId = "date:0"
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("Tomorrow"),
          SortOrder = 4L,
          ColumnId = "date:1"
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("Next7Day"),
          SortOrder = 5L,
          ColumnId = "date:week"
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("later"),
          SortOrder = 6L,
          ColumnId = "date:later"
        },
        new ColumnViewModel()
        {
          Name = Utils.GetString("NoDate"),
          SortOrder = 7L,
          ColumnId = "date:no"
        }
      };
    }

    public void SetTaskListViewModel(TaskListViewModel taskListViewModel)
    {
      this._taskListViewModel = taskListViewModel;
    }

    public TaskListViewModel GetListViewModel() => this._taskListViewModel;

    public IEnumerable<string> GetSelectedTask()
    {
      TaskListViewModel taskListViewModel = this._taskListViewModel;
      return taskListViewModel == null ? (IEnumerable<string>) null : taskListViewModel.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (task => task.Selected)).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (task => task.Id));
    }

    public DateTime? GetDate() => ColumnViewModel.GetDate(this.ColumnId);

    public static DateTime? GetDate(string columnId)
    {
      if (!columnId.StartsWith("date:"))
        return new DateTime?();
      int result;
      if (int.TryParse(columnId.Replace("date:", ""), out result))
        return new DateTime?(DateTime.Today.AddDays((double) result));
      switch (columnId)
      {
        case "date:week":
          return new DateTime?(DateTime.Today.AddDays(2.0));
        case "date:later":
          return new DateTime?(DateTime.Today.AddDays(8.0));
        default:
          return new DateTime?();
      }
    }

    public TagModel GetTag() => ColumnViewModel.GetTag(this.ColumnId);

    public static TagModel GetTag(string columnId)
    {
      return columnId.StartsWith("tag:") ? CacheManager.GetTagByName(columnId.Replace("tag:", "")) : (TagModel) null;
    }

    public int GetPriority() => ColumnViewModel.GetPriority(this.ColumnId);

    public static int GetPriority(string columnId)
    {
      int result;
      return columnId.StartsWith("priority:") && int.TryParse(columnId.Replace("priority:", ""), out result) ? result : 0;
    }

    public string GetProject() => ColumnViewModel.GetProject(this.ColumnId);

    public static string GetProject(string columnId)
    {
      return columnId.StartsWith("project:") ? columnId.Replace("project:", "") : (string) null;
    }

    public string GetAssignee() => ColumnViewModel.GetAssignee(this.ColumnId);

    public static string GetAssignee(string columnId)
    {
      return columnId.StartsWith("assign:") ? columnId.Replace("assign:", "") : "-1";
    }

    public void Clear() => this._taskListViewModel?.Clear();

    public void Dispose() => this._taskListViewModel?.Dispose();

    public static ColumnViewModel GetHabitColumn(bool useInWeek = false)
    {
      ColumnViewModel habitColumn = new ColumnViewModel()
      {
        Name = Utils.GetString("Habit"),
        SortOrder = long.MaxValue,
        ColumnId = "habit",
        CanDropDown = false,
        CanAdd = false
      };
      if (useInWeek)
        habitColumn.Name = Utils.IsCn() || Utils.IsJp() ? Utils.GetString("TodayHabit") + ", " + DateUtils.FormatWeekDayName(DateTime.Today) : DateUtils.FormatWeekDayName(DateTime.Today) + ", " + Utils.GetString("TodayHabit");
      return habitColumn;
    }

    public static ColumnViewModel GetNoteColumn()
    {
      return new ColumnViewModel()
      {
        Name = Utils.GetString("Notes"),
        SortOrder = 1000,
        ColumnId = "note",
        CanDropDown = false
      };
    }

    public static string GetTaskSortKey(SortOption sortOption, string columnId)
    {
      if (sortOption == null)
        return (string) null;
      if (sortOption.orderBy == "sortOrder")
        return (string) null;
      if (columnId == "date:later" || columnId == "date:-1" || columnId == "date:week" || columnId == "note")
        return (string) null;
      string sortKey = sortOption.GetSortKey();
      if (string.IsNullOrEmpty(sortKey))
        return (string) null;
      switch (columnId)
      {
        case "tag:#notag":
          return string.Format(sortKey, (object) "noTag");
        case "date:no":
          return string.Format(sortKey, (object) "noDate");
        case "assign:-1":
          return string.Format(sortKey, (object) "-1");
        default:
          if (columnId.StartsWith("assign:"))
            return string.Format(sortKey, (object) ColumnViewModel.GetAssignee(columnId));
          if (columnId.StartsWith("tag:"))
          {
            TagModel tag = ColumnViewModel.GetTag(columnId);
            if (tag != null)
              return string.Format(sortKey, (object) tag.name);
          }
          if (columnId.StartsWith("priority:"))
            return string.Format(sortKey, (object) ColumnViewModel.GetPriority(columnId));
          if (columnId.StartsWith("project:"))
            return string.Format(sortKey, (object) ColumnViewModel.GetProject(columnId));
          if (columnId.StartsWith("date:"))
          {
            DateTime? date = ColumnViewModel.GetDate(columnId);
            if (date.HasValue)
              return string.Format(sortKey, (object) date.Value.ToString("yyyyMMdd", (IFormatProvider) App.Ci));
          }
          return string.Format(sortKey, (object) columnId);
      }
    }

    public static bool CanTaskSortInColumn(string columnId)
    {
      return !(columnId == "date:later") && !(columnId == "date:-1") && !(columnId == "date:week") && !(columnId == "note");
    }

    public void SetColumnIdentity(ProjectIdentity identity)
    {
      ColumnProjectIdentity columnProjectIdentity = new ColumnProjectIdentity(identity, this.ColumnId);
      this.Identity = columnProjectIdentity;
      this.Enable = false;
      this.CanAdd = this.CanAdd && !(identity is AssignToMeProjectIdentity) && columnProjectIdentity.CanEdit && this.ColumnId != "habit" && this.ColumnId != "calendar" && this.ColumnId != "course";
    }

    public static ColumnViewModel GetDefaultColumn(ProjectIdentity identity, string groupBy)
    {
      return (ColumnViewModel) null;
    }

    public void RemoveSelectedId(string id)
    {
      TaskListViewModel taskListViewModel = this._taskListViewModel;
      DisplayItemModel displayItemModel1;
      if (taskListViewModel == null)
      {
        displayItemModel1 = (DisplayItemModel) null;
      }
      else
      {
        ObservableCollection<DisplayItemModel> items = taskListViewModel.Items;
        displayItemModel1 = items != null ? items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == id)) : (DisplayItemModel) null;
      }
      DisplayItemModel displayItemModel2 = displayItemModel1;
      if (displayItemModel2 == null)
        return;
      displayItemModel2.Selected = false;
    }
  }
}
