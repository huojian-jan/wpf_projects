// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TaskDetailViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TaskDetailViewModel : BaseViewModel, AgendaHelper.IAgenda
  {
    private bool _isOwner;
    private bool _isNewAdd;
    private int _mode;
    private bool _checkBoxMouseOver;
    private string _tag;
    public AttachmentModel[] Attachments;
    public int Id;
    public TaskReminderModel[] Reminders;
    public BatchData BatchData;
    public string NavigateItemId;
    private int _imageMode;
    public List<string> GuideActions;
    public List<GuideProjectTaskResource> GuideResources;
    public string Label;

    public string Color => this.SourceViewModel.Color;

    public string CommentCount => this.SourceViewModel.CommentCount;

    public int Deleted => this.SourceViewModel.Deleted;

    public TaskBaseViewModel SourceViewModel { get; set; }

    public TaskDetailItemModel[] Items { get; set; }

    public BlockingList<TaskBaseViewModel> CheckItems => this.SourceViewModel.CheckItems;

    public int? Progress
    {
      get
      {
        double? repeatDiff = this.RepeatDiff;
        double num = 0.0;
        return new int?(repeatDiff.GetValueOrDefault() > num & repeatDiff.HasValue ? 0 : this.SourceViewModel.Progress);
      }
    }

    public bool Pinned => this.SourceViewModel.PinnedTime > 0L;

    public bool IsAbandoned => this.Status == -1;

    public bool IsCompleted => this.Status == 2;

    public string Tag => this.SourceViewModel.Tag;

    public string TaskId => this.SourceViewModel.Id;

    public string Permission => this.SourceViewModel.Permission;

    public string TeamId => this.SourceViewModel.TeamId;

    public TaskDetailViewModel(TaskBaseViewModel task)
    {
      this.SourceViewModel = task;
      this.AddVmPropertyChangedEvent();
      if (!string.IsNullOrEmpty(task.Actions))
        this.GuideActions = JsonConvert.DeserializeObject<List<GuideProjectTaskAction>>(task.Actions).Select<GuideProjectTaskAction, string>((Func<GuideProjectTaskAction, string>) (a => a.url)).ToList<string>();
      this.Label = task.Label;
      if (string.IsNullOrEmpty(task.Resources))
        return;
      this.GuideResources = JsonConvert.DeserializeObject<List<GuideProjectTaskResource>>(task.Resources).ToList<GuideProjectTaskResource>();
    }

    public TaskDetailViewModel(TaskModel task)
    {
      this.SourceViewModel = TaskCache.SafeGetTaskViewModel(task);
      this.AddVmPropertyChangedEvent();
      this._imageMode = task.imgMode;
      this.Reminders = task.reminders;
      this.Attachments = task.Attachments;
      this.ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>();
      if (!string.IsNullOrEmpty(task.Actions))
        this.GuideActions = JsonConvert.DeserializeObject<List<GuideProjectTaskAction>>(task.Actions).Select<GuideProjectTaskAction, string>((Func<GuideProjectTaskAction, string>) (a => a.url)).ToList<string>();
      this.Label = task.label;
      if (string.IsNullOrEmpty(task.Resources))
        return;
      this.GuideResources = JsonConvert.DeserializeObject<List<GuideProjectTaskResource>>(task.Resources).ToList<GuideProjectTaskResource>();
    }

    public string ParentId => this.SourceViewModel.ParentId;

    public string ProjectId => this.SourceViewModel.ProjectId;

    public string Assignee => this.SourceViewModel.Assignee;

    public List<string> ExDates { get; set; }

    public string AttendId { get; set; }

    public string Title => this.SourceViewModel.Title;

    public int ImageMode => this.SourceViewModel.ImageMode;

    public string TaskContent => this.SourceViewModel.Content;

    public string Desc => this.SourceViewModel.Desc;

    public bool IsFloating => this.SourceViewModel.IsFloating;

    public string TimeZoneName => this.SourceViewModel.TimeZoneName;

    public DateTime? StartDate => this.SourceViewModel.StartDate;

    public DateTime? DueDate => this.SourceViewModel.DueDate;

    public string ColumnId => this.SourceViewModel.ColumnId;

    public double? RepeatDiff { get; set; }

    public DateTime? DisplayStartDate
    {
      get
      {
        if (this.ParseData != null)
          return this.ParseData.StartDate;
        double? repeatDiff = this.RepeatDiff;
        if (!repeatDiff.HasValue)
          return this.StartDate;
        DateTime? startDate = this.StartDate;
        ref DateTime? local1 = ref startDate;
        if (!local1.HasValue)
          return new DateTime?();
        DateTime valueOrDefault = local1.GetValueOrDefault();
        ref DateTime local2 = ref valueOrDefault;
        repeatDiff = this.RepeatDiff;
        double num = repeatDiff.Value;
        return new DateTime?(local2.AddDays(num));
      }
    }

    public DateTime? DisplayDueDate
    {
      get
      {
        if (this.ParseData != null)
          return this.ParseData.DueDate;
        double? repeatDiff = this.RepeatDiff;
        if (!repeatDiff.HasValue)
          return this.DueDate;
        DateTime? dueDate = this.DueDate;
        ref DateTime? local1 = ref dueDate;
        if (!local1.HasValue)
          return new DateTime?();
        DateTime valueOrDefault = local1.GetValueOrDefault();
        ref DateTime local2 = ref valueOrDefault;
        repeatDiff = this.RepeatDiff;
        double num = repeatDiff.Value;
        return new DateTime?(local2.AddDays(num));
      }
    }

    public bool? IsAllDay
    {
      get => this.ParseData != null ? this.ParseData.IsAllDay : this.SourceViewModel.IsAllDay;
    }

    public int Priority => this.SourceViewModel.Priority;

    public int Status => this.SourceViewModel.Status;

    public string Kind => this.SourceViewModel.Kind;

    public bool Enable => this.SourceViewModel.Editable;

    public Visibility ShowSnoozeText
    {
      get
      {
        return !this.RemindTime.HasValue || !(this.RemindTime.Value > DateTime.Now) || !this.StartDate.HasValue || !(this.RemindTime.Value != this.StartDate.Value) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility ShowTimeZoneText
    {
      get
      {
        return !LocalSettings.Settings.EnableTimeZone || this.IsFloating || !(this.TimeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName) || !this.DisplayStartDate.HasValue || this.IsAllDay.HasValue && this.IsAllDay.Value ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility ProgressVisibility
    {
      get
      {
        return this.Status != 0 || this.IsNewAdd || !this.IsOwner || !(this.Kind != "NOTE") ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public SolidColorBrush CheckIconColor
    {
      get
      {
        if (this.Status == 0)
        {
          switch (this.Priority)
          {
            case 1:
              return ThemeUtil.GetColor("PriorityLowColor");
            case 3:
              return ThemeUtil.GetColor("PriorityMiddleColor");
            case 5:
              return ThemeUtil.GetColor("PriorityHighColor");
          }
        }
        return ThemeUtil.GetColor("BaseColorOpacity100", this.Element);
      }
    }

    public double CheckIconOpacity
    {
      get
      {
        return this.Status == 0 ? (this.Priority != 0 ? 1.0 : 0.4) : (!this._checkBoxMouseOver ? 0.2 : 0.4);
      }
    }

    public double CheckIconBackOpacity => this.Status != 0 || !this._checkBoxMouseOver ? 0.0 : 0.15;

    public Geometry DateIcon => Utils.GetIconData("IcCalendar");

    public double IconWidth { get; set; } = 14.0;

    public Geometry Icon
    {
      get
      {
        if (this.Status == -1)
          return Utils.GetIconData("IcAbandoned");
        if (this.Status != 0)
          return Utils.GetIconData("IcChecked");
        return string.IsNullOrEmpty(this.AttendId) ? Utils.GetIconData(this.Kind == "CHECKLIST" ? "IcCheckList" : "IcCheckBox") : Utils.GetIconData("IcAgendaItem");
      }
    }

    public Visibility ShowCheckIcon
    {
      get => this.IsNewAdd || !(this.Kind != "NOTE") ? Visibility.Collapsed : Visibility.Visible;
    }

    public bool IsNote => this.Kind == "NOTE";

    public SolidColorBrush DateIconColor
    {
      get
      {
        if (!this.DisplayStartDate.HasValue)
          return ThemeUtil.GetColor(this.BatchData != null ? "BaseColorOpacity60" : "BaseColorOpacity40", this.Element);
        return !DateUtils.IsOutDated(this.DisplayStartDate, this.DisplayDueDate, this.IsAllDay) ? ThemeUtil.GetColor("DateColorPrimary", this.Element) : ThemeUtil.GetColor("OutDateColor");
      }
    }

    public string TimeZoneText
    {
      get
      {
        string timeZoneText = "";
        DateTime? date = this.DisplayStartDate;
        DateTime? nullable = this.DisplayDueDate;
        string timeZoneName = this.TimeZoneName;
        if (!this.IsAllDay.GetValueOrDefault() && timeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName && timeZoneName != null)
        {
          if (!string.IsNullOrEmpty(timeZoneName))
          {
            TimeZoneInfo timeZoneInfo = TimeZoneUtils.GetTimeZoneInfo(timeZoneName);
            date = TimeZoneUtils.LocalToTargetZoneTime(date, timeZoneName);
            nullable = TimeZoneUtils.LocalToTargetZoneTime(nullable, timeZoneName);
            timeZoneText = TimeZoneUtils.GetTimeZoneDisplayName(timeZoneInfo);
          }
          if (date.HasValue)
            return DateUtils.FormatDateString(date.Value, nullable, this.IsAllDay, false) + " " + timeZoneText;
        }
        return timeZoneText;
      }
    }

    public string DateText
    {
      get
      {
        if (this.DisplayStartDate.HasValue)
          return DateUtils.FormatDateString(this.DisplayStartDate.Value, this.DisplayDueDate, this.IsAllDay, this._mode == 0);
        if (this.Status != 0)
          return Utils.GetString("DateNotSet");
        return !(this.Kind == "NOTE") ? Utils.GetString("DateAndReminder") : Utils.GetString("SetReminder");
      }
    }

    public string RepeatText
    {
      get
      {
        if (RepeatUtils.GetRepeatType(this.RepeatFrom, this.RepeatFlag) == RepeatFromType.Custom)
          return Utils.GetString("RepeatByCustom");
        if (string.IsNullOrEmpty(this.RepeatFlag) || this.RepeatFlag == "RRULE:FREQ=NONE")
          return string.Empty;
        return !this.RepeatFlag.Contains("FREQ=DAILY") || !this.RepeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND") || this.RepeatFlag.Contains("INTERVAL") && !this.RepeatFlag.Contains("INTERVAL=1") ? RRuleUtils.RRule2String(this.RepeatFrom, this.RepeatFlag, this.StartDate) : Utils.GetString("OfficialWorkingDays");
      }
    }

    public Visibility ShowRepeatIcon
    {
      get
      {
        return this.Status != 0 || Utils.IsEmptyRepeatFlag(this.RepeatFlag) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public DateTime? RemindTime
    {
      get
      {
        double? repeatDiff = this.RepeatDiff;
        double num = 0.0;
        return !(repeatDiff.GetValueOrDefault() > num & repeatDiff.HasValue) ? this.SourceViewModel.RemindTime : new DateTime?();
      }
    }

    public string RemindTimeText
    {
      get
      {
        DateTime valueOrDefault = this.RemindTime.GetValueOrDefault();
        if (!(valueOrDefault > DateTime.Now))
          return string.Empty;
        DateTime now = DateTime.Now;
        if (Math.Abs((now.Date - valueOrDefault.Date).TotalDays - -1.0) <= 0.001)
          return string.Format(Utils.GetString("PreviewSnoozeText"), (object) Utils.GetString("Tomorrow"));
        now = DateTime.Now;
        return Math.Abs((now.Date - valueOrDefault.Date).TotalDays) <= 0.001 ? string.Format(Utils.GetString("PreviewSnoozeText"), (object) DateUtils.FormatHourMinute(valueOrDefault)) : string.Format(Utils.GetString("PreviewSnoozeText"), (object) (DateUtils.FormatFullDate(valueOrDefault) + " " + DateUtils.FormatHourMinute(valueOrDefault)));
      }
    }

    public string RepeatFlag
    {
      get => this.ParseData != null ? this.ParseData.RepeatFlag : this.SourceViewModel.RepeatFlag;
    }

    public string RepeatFrom => this.SourceViewModel.RepeatFrom;

    public string ProjectName
    {
      get
      {
        string emojiIcon = EmojiHelper.GetEmojiIcon(this.SourceViewModel.ProjectName);
        return !string.IsNullOrEmpty(emojiIcon) && this.SourceViewModel.ProjectName.StartsWith(emojiIcon) ? this.SourceViewModel.ProjectName.Substring(emojiIcon.Length) : this.SourceViewModel.ProjectName;
      }
    }

    public string Emoji
    {
      get
      {
        string emojiIcon = EmojiHelper.GetEmojiIcon(this.SourceViewModel.ProjectName);
        return !string.IsNullOrEmpty(emojiIcon) && this.SourceViewModel.ProjectName.StartsWith(emojiIcon) ? emojiIcon : (string) null;
      }
    }

    public bool IsNewAdd
    {
      get => this._isNewAdd;
      set
      {
        this.OnPropertyChanged(nameof (IsNewAdd));
        this.OnPropertyChanged("ShowCheckIcon");
        this.OnPropertyChanged("ProgressVisibility");
        this._isNewAdd = value;
      }
    }

    public int Mode
    {
      get => this._mode;
      set
      {
        this._mode = value;
        this.OnPropertyChanged(nameof (Mode));
      }
    }

    public bool ShowSetTime => !this.DisplayStartDate.HasValue;

    public string DayText
    {
      get
      {
        DateTime? displayStartDate = this.DisplayStartDate;
        ref DateTime? local = ref displayStartDate;
        return (local.HasValue ? local.GetValueOrDefault().Day.ToString() : (string) null) ?? string.Empty;
      }
    }

    public bool IsOwner
    {
      get => this._isOwner;
      set
      {
        this._isOwner = value;
        this.OnPropertyChanged(nameof (IsOwner));
        this.OnPropertyChanged("ProgressVisibility");
      }
    }

    public bool CheckBoxMouseOver
    {
      get => this._checkBoxMouseOver;
      set
      {
        this._checkBoxMouseOver = value;
        this.OnPropertyChanged(nameof (CheckBoxMouseOver));
        this.OnPropertyChanged("CheckIconOpacity");
        this.OnPropertyChanged("CheckIconBackOpacity");
      }
    }

    public DateTime? CreateDate => this.SourceViewModel.CreatedTime;

    public DateTime? ModifiedDate => this.SourceViewModel.ModifiedTime;

    public TimeData ParseData { get; set; }

    public FrameworkElement Element { get; set; }

    public bool InCal { get; set; }

    public DrawingImage PriorityImage
    {
      get
      {
        switch (this.Priority)
        {
          case 0:
            return Utils.GetImageSource("NonePriorityThinDrawingImage", this.Element);
          case 1:
            return Utils.GetImageSource("LowPriorityDrawingImage");
          case 3:
            return Utils.GetImageSource("MidPriorityDrawingImage");
          case 5:
            return Utils.GetImageSource("HighPriorityDrawingImage");
          default:
            return (DrawingImage) null;
        }
      }
    }

    public string GetTaskId() => this.TaskId;

    public string GetAttendId() => this.AttendId;

    public void SetTimeData(TimeData data)
    {
      if (!TaskCache.ExistTask(this.TaskId))
      {
        this.SourceViewModel.StartDate = data.StartDate;
        this.SourceViewModel.DueDate = data.DueDate;
        this.SourceViewModel.IsAllDay = data.IsAllDay;
        this.SourceViewModel.RepeatFlag = data.RepeatFlag;
        this.SourceViewModel.RepeatFrom = data.RepeatFrom;
        this.SourceViewModel.ExDates = ExDateSerilizer.ToString(data.ExDates?.ToArray());
        TaskBaseViewModel sourceViewModel = this.SourceViewModel;
        TimeZoneViewModel timeZone = data.TimeZone;
        int num = timeZone != null ? (timeZone.IsFloat ? 1 : 0) : 0;
        sourceViewModel.IsFloating = num != 0;
        this.SourceViewModel.TimeZoneName = data.TimeZone?.TimeZoneName;
      }
      this.Reminders = data.Reminders?.ToArray();
    }

    public void SetParseData(TimeData data)
    {
      if (this.ParseData == null && data == null)
        return;
      this.ParseData = data;
      this.OnPropertyChanged("DayText");
      this.OnPropertyChanged("DateText");
      this.OnPropertyChanged("DateIcon");
      this.OnPropertyChanged("DateIconColor");
      this.OnPropertyChanged("ShowSetTime");
      this.OnPropertyChanged("RepeatText");
      this.OnPropertyChanged("ShowRepeatIcon");
    }

    public static TaskDetailViewModel BuildInitModel(TaskBaseViewModel baseModel, bool isAllDay = true)
    {
      TaskBaseViewModel taskBaseViewModel = baseModel;
      if (taskBaseViewModel == null)
        taskBaseViewModel = new TaskBaseViewModel()
        {
          Id = Utils.GetGuid(),
          Title = string.Empty,
          ProjectId = Utils.GetInboxId(),
          Priority = TaskDefaultDao.GetDefaultSafely().Priority,
          Tag = TaskDefaultDao.GetDefaultSafely().TagString,
          Tags = TaskDefaultDao.GetDefaultSafely().Tags?.ToArray(),
          Kind = "TEXT",
          IsAllDay = new bool?(isAllDay)
        };
      baseModel = taskBaseViewModel;
      return new TaskDetailViewModel(baseModel)
      {
        IsNewAdd = true,
        Reminders = ((int) baseModel.IsAllDay ?? 1) == 0 ? TimeData.GetDefaultTimeReminders().ToArray() : TimeData.GetDefaultAllDayReminders().ToArray()
      };
    }

    public static async Task<TaskDetailViewModel> Build(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        return (TaskDetailViewModel) null;
      task.content = Regex.Replace(task.content ?? string.Empty, "(?<!\r)\n", "\r\n");
      TaskModel taskModel = await TaskDao.AssembleFullTask(task);
      return new TaskDetailViewModel(task);
    }

    public static async Task<TaskModel> ToTaskModel(TaskDetailViewModel viewModel)
    {
      TaskModel taskModel1;
      if (!string.IsNullOrEmpty(viewModel?.TaskId))
      {
        TaskModel taskModel2 = await TaskDao.GetThinTaskById(viewModel.TaskId);
        if (taskModel2 == null)
          taskModel2 = new TaskModel()
          {
            id = viewModel.TaskId
          };
        taskModel1 = taskModel2;
      }
      else
        taskModel1 = new TaskModel();
      if (viewModel != null)
      {
        taskModel1.title = viewModel.Title;
        taskModel1.content = viewModel.TaskContent;
        taskModel1.desc = viewModel.Desc;
        taskModel1.content = viewModel.TaskContent;
        taskModel1.startDate = viewModel.StartDate;
        taskModel1.dueDate = viewModel.DueDate;
        taskModel1.remindTime = viewModel.RemindTime;
        taskModel1.isAllDay = viewModel.IsAllDay;
        taskModel1.status = viewModel.Status;
        taskModel1.kind = viewModel.Kind;
        taskModel1.priority = viewModel.Priority;
        taskModel1.repeatFlag = viewModel.RepeatFlag;
        taskModel1.repeatFrom = viewModel.RepeatFrom;
        taskModel1.projectId = viewModel.ProjectId;
        taskModel1.assignee = viewModel.Assignee;
        taskModel1.progress = viewModel.Progress;
        taskModel1.deleted = viewModel.Deleted;
        taskModel1.commentCount = viewModel.CommentCount;
        taskModel1.reminders = viewModel.Reminders;
        taskModel1.Attachments = viewModel.Attachments;
        taskModel1.Color = viewModel.Color;
        taskModel1.tag = viewModel.Tag;
        taskModel1.exDates = viewModel.ExDates != null ? ExDateSerilizer.ToString(viewModel.ExDates.ToArray()) : (string) null;
        taskModel1.attendId = viewModel.AttendId;
        taskModel1.isFloating = new bool?(viewModel.IsFloating);
        taskModel1.timeZone = viewModel.TimeZoneName;
        taskModel1.imgMode = viewModel.ImageMode;
        taskModel1.columnId = viewModel.ColumnId;
        TaskModel taskModel3 = taskModel1;
        TaskBaseViewModel sourceViewModel = viewModel.SourceViewModel;
        long sortOrder = sourceViewModel != null ? sourceViewModel.SortOrder : 0L;
        taskModel3.sortOrder = sortOrder;
      }
      return taskModel1;
    }

    private void AddVmPropertyChangedEvent()
    {
      if (this.SourceViewModel == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
    }

    private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Tag"))
            break;
          this.OnPropertyChanged("Tag");
          break;
        case 4:
          switch (propertyName[0])
          {
            case 'D':
              if (!(propertyName == "Desc"))
                return;
              this.OnPropertyChanged("Desc");
              return;
            case 'K':
              if (!(propertyName == "Kind"))
                return;
              this.OnPropertyChanged("Kind");
              this.OnPropertyChanged("IsNote");
              this.OnPropertyChanged("DateText");
              this.OnPropertyChanged("ShowCheckIcon");
              this.OnPropertyChanged("Icon");
              this.OnPropertyChanged("ProgressVisibility");
              return;
            case 'T':
              if (!(propertyName == "Type"))
                return;
              this.OnPropertyChanged("Type");
              return;
            default:
              return;
          }
        case 5:
          if (!(propertyName == "Title"))
            break;
          this.OnPropertyChanged("Title");
          break;
        case 6:
          if (!(propertyName == "Status"))
            break;
          this.OnPropertyChanged("Status");
          this.OnPropertyChanged("IsAbandoned");
          this.OnPropertyChanged("IsCompleted");
          this.OnPropertyChanged("CheckIconColor");
          this.OnPropertyChanged("Icon");
          this.OnPropertyChanged("CheckIconOpacity");
          this.OnPropertyChanged("CheckIconBackOpacity");
          this.OnPropertyChanged("ShowRepeatIcon");
          this.OnPropertyChanged("ProgressVisibility");
          break;
        case 7:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "Content"))
                return;
              this.OnPropertyChanged("TaskContent");
              return;
            case 'D':
              if (!(propertyName == "DueDate"))
                return;
              this.OnPropertyChanged("DisplayDueDate");
              this.OnPropertyChanged("DateText");
              this.OnPropertyChanged("TimeZoneText");
              return;
            default:
              return;
          }
        case 8:
          switch (propertyName[3])
          {
            case 'g':
              if (!(propertyName == "Progress"))
                return;
              this.OnPropertyChanged("Progress");
              return;
            case 'h':
              return;
            case 'i':
              if (!(propertyName == "Assignee"))
                return;
              this.OnPropertyChanged("Assignee");
              return;
            case 'j':
              return;
            case 'k':
              return;
            case 'l':
              if (!(propertyName == "IsAllDay"))
                return;
              this.OnPropertyChanged("IsAllDay");
              this.OnPropertyChanged("DateText");
              this.OnPropertyChanged("TimeZoneText");
              return;
            case 'm':
              if (!(propertyName == "Comments"))
                return;
              this.OnPropertyChanged("Comments");
              return;
            case 'n':
              return;
            case 'o':
              if (!(propertyName == "Priority"))
                return;
              this.OnPropertyChanged("Priority");
              this.OnPropertyChanged("PriorityImage");
              this.OnPropertyChanged("CheckIconColor");
              this.OnPropertyChanged("CheckIconOpacity");
              return;
            case 't':
              if (!(propertyName == "Editable"))
                return;
              this.OnPropertyChanged("Enable");
              return;
            case 'u':
              if (!(propertyName == "ColumnId"))
                return;
              this.OnPropertyChanged("ColumnId");
              return;
            default:
              return;
          }
        case 9:
          switch (propertyName[0])
          {
            case 'I':
              if (!(propertyName == "ImageMode"))
                return;
              this.OnPropertyChanged("ImageMode");
              return;
            case 'P':
              if (!(propertyName == "ProjectId"))
                return;
              this.OnPropertyChanged("ProjectId");
              return;
            case 'S':
              if (!(propertyName == "StartDate"))
                return;
              this.OnPropertyChanged("DisplayStartDate");
              this.OnPropertyChanged("DayText");
              this.OnPropertyChanged("DateText");
              this.OnPropertyChanged("DateIcon");
              this.OnPropertyChanged("DateIconColor");
              this.OnPropertyChanged("ShowSetTime");
              this.OnPropertyChanged("TimeZoneText");
              this.OnPropertyChanged("RepeatText");
              return;
            default:
              return;
          }
        case 10:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "CheckItems"))
                return;
              this.OnPropertyChanged("CheckItems");
              return;
            case 'I':
              if (!(propertyName == "IsFloating"))
                return;
              this.OnPropertyChanged("IsFloating");
              return;
            case 'R':
              if (!(propertyName == "RepeatFlag"))
                return;
              this.OnPropertyChanged("ShowRepeatIcon");
              this.OnPropertyChanged("RepeatText");
              return;
            default:
              return;
          }
        case 11:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "CreatedTime"))
                return;
              this.OnPropertyChanged("CreateDate");
              return;
            case 'P':
              if (!(propertyName == "ProjectName"))
                return;
              this.OnPropertyChanged("ProjectName");
              this.OnPropertyChanged("Emoji");
              return;
            default:
              return;
          }
        case 12:
          switch (propertyName[0])
          {
            case 'M':
              if (!(propertyName == "ModifiedTime"))
                return;
              this.OnPropertyChanged("ModifiedDate");
              return;
            case 'T':
              if (!(propertyName == "TimeZoneName"))
                return;
              this.OnPropertyChanged("TimeZoneName");
              this.OnPropertyChanged("ShowTimeZoneText");
              this.OnPropertyChanged("TimeZoneText");
              return;
            default:
              return;
          }
      }
    }

    public void CheckBoxMouseEnterCommand() => this.CheckBoxMouseOver = true;

    public void CheckBoxMouseLeaveCommand() => this.CheckBoxMouseOver = false;

    public void OnThemeChanged()
    {
      this.OnPropertyChanged("CheckIconColor");
      this.OnPropertyChanged("DateIconColor");
      this.OnPropertyChanged("Priority");
    }

    public void SetPropertyChanged(string property) => this.OnPropertyChanged(property);
  }
}
