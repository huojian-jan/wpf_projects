// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TaskModel : BaseModel, AgendaHelper.IAgenda
  {
    private string _projectId;
    private string _completedUserId;
    private string _columnId;
    private string _userId;
    private DateTime? _startDate;
    private DateTime? _dueDate;
    private string _kind;
    private string _timeZone;

    public TaskModel()
    {
    }

    public TaskModel(TaskModel task)
    {
      this._Id = task._Id;
      this.id = task.id;
      this.projectId = task.projectId;
      this.priority = task.priority;
      this.title = task.title;
      this.content = task.content;
      this.isAllDay = task.isAllDay;
      this.repeatFlag = task.repeatFlag;
      this.completedTime = task.completedTime;
      this.createdTime = task.createdTime;
      this.deleted = task.deleted;
      this.startDate = task.startDate;
      this.dueDate = task.dueDate;
      this.status = task.status;
      this.kind = task.kind;
      this.Color = task.Color;
      this.sortOrder = task.sortOrder;
      this.desc = task.desc;
      this.timeZone = task.timeZone;
      this.reminder = task.reminder;
      this.repeatFirstDate = task.repeatFirstDate;
      this.completedUserId = task.completedUserId;
      this.repeatTaskId = task.repeatTaskId;
      this.progress = task.progress;
      this.modifiedTime = task.modifiedTime;
      this.etag = task.etag;
      this.remindTime = task.remindTime;
      this.location = task.location;
      this.repeatFrom = task.repeatFrom;
      this.Attachments = task.Attachments;
      this.commentCount = task.commentCount;
      this.assignee = task.assignee;
      this.userId = task.userId;
      this.items = task.items;
      this.reminders = task.reminders;
      this.tag = task.tag;
      this.exDates = task.exDates;
      this.exDate = task.exDate;
      this.columnId = task.columnId;
      this.isFloating = new bool?(task.Floating);
      this.parentId = task.parentId;
      this.pinnedTime = task.pinnedTime;
      this.creator = LocalSettings.Settings.LoginUserId;
    }

    [Indexed]
    public string id { get; set; }

    [Indexed]
    public string projectId
    {
      get => this._projectId;
      set => this._projectId = StringPool.GetOrCreate(value);
    }

    public int imgMode { get; set; }

    public string parentId { get; set; }

    public int priority { get; set; }

    public string title { get; set; }

    public string content { get; set; }

    public bool? isAllDay { get; set; }

    public string repeatFlag { get; set; }

    public string reminder { get; set; }

    public string completedUserId
    {
      get => this._completedUserId;
      set => this._completedUserId = StringPool.GetOrCreate(value);
    }

    public string repeatTaskId { get; set; }

    public int? progress { get; set; }

    public string etag { get; set; }

    public string attendId { get; set; }

    public int hasLocation { get; set; }

    public string columnId
    {
      get => this._columnId;
      set => this._columnId = StringPool.GetOrCreate(value);
    }

    [Ignore]
    public string[] exDate { get; set; }

    [JsonIgnore]
    public string exDates { get; set; }

    [Ignore]
    public LocationModel location { get; set; }

    public string creator { get; set; }

    public string repeatFrom { get; set; }

    [JsonProperty("attachments")]
    [Ignore]
    public AttachmentModel[] Attachments { get; set; }

    [JsonProperty("focusSummaries")]
    [Ignore]
    public PomodoroSummaryModel[] FocusSummaries { get; set; }

    public string commentCount { get; set; }

    public string assignee { get; set; }

    public string userId
    {
      get => this._userId;
      set => this._userId = StringPool.GetOrCreate(value);
    }

    [JsonIgnore]
    public string tag { get; set; }

    [Ignore]
    public TaskDetailItemModel[] items { get; set; }

    [Ignore]
    public TaskReminderModel[] reminders { get; set; }

    [Ignore]
    private DateTime? CompletedTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? completedTime
    {
      get => !Utils.IsEmptyDate(this.CompletedTime) ? this.CompletedTime : new DateTime?();
      set => this.CompletedTime = value;
    }

    [Ignore]
    private DateTime? CreatedTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? createdTime
    {
      get => !Utils.IsEmptyDate(this.CreatedTime) ? this.CreatedTime : new DateTime?();
      set => this.CreatedTime = value;
    }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? startDate
    {
      get => !Utils.IsEmptyDate(this._startDate) ? this._startDate : new DateTime?();
      set => this._startDate = Utils.IsEmptyDate(value) ? new DateTime?() : value;
    }

    [Ignore]
    private DateTime? RepeatFirstDate { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? repeatFirstDate
    {
      get => !Utils.IsEmptyDate(this.RepeatFirstDate) ? this.RepeatFirstDate : new DateTime?();
      set => this.RepeatFirstDate = value;
    }

    [Ignore]
    private DateTime? ModifiedTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? modifiedTime
    {
      get
      {
        return !Utils.IsEmptyDate(this.ModifiedTime) ? this.ModifiedTime : new DateTime?(DateTime.Now);
      }
      set => this.ModifiedTime = value;
    }

    [Ignore]
    private DateTime? RemindTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? remindTime
    {
      get => !Utils.IsEmptyDate(this.RemindTime) ? this.RemindTime : new DateTime?();
      set => this.RemindTime = value;
    }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? dueDate
    {
      get => !Utils.IsEmptyDate(this._dueDate) ? this._dueDate : new DateTime?();
      set => this._dueDate = Utils.IsEmptyDate(value) ? new DateTime?() : value;
    }

    public int deleted { get; set; }

    public int status { get; set; }

    [Ignore]
    [JsonIgnore]
    public bool isAbandoned => this.status == -1;

    [Ignore]
    [JsonIgnore]
    public bool isCompleted => this.status != 0 && !this.isAbandoned;

    public string kind
    {
      get => this._kind ?? "TEXT";
      set
      {
        switch (value)
        {
          case null:
          case "TEXT":
            this._kind = "TEXT";
            break;
          case "CHECKLIST":
            this._kind = "CHECKLIST";
            break;
          default:
            this._kind = value;
            break;
        }
      }
    }

    [Ignore]
    public string Color { get; set; }

    public long sortOrder { get; set; }

    public string desc { get; set; }

    public string timeZone
    {
      get => !string.IsNullOrEmpty(this._timeZone) ? this._timeZone : Utils.GetLocalTimeZone();
      set => this._timeZone = value == Utils.GetLocalTimeZone() ? Utils.GetLocalTimeZone() : value;
    }

    public int attachmentCount { get; set; }

    public int reminderCount { get; set; }

    [Ignore]
    public string[] tags { get; set; }

    public bool? isFloating { get; set; } = new bool?(false);

    [Ignore]
    [JsonIgnore]
    public bool Floating => this.isFloating.GetValueOrDefault();

    [JsonIgnore]
    public string childrenString { get; set; }

    [Ignore]
    public List<string> childIds { get; set; }

    [Ignore]
    public List<TaskModel> children { get; set; }

    [NotNull]
    [DefaultValue("1")]
    [JsonIgnore]
    public bool isOpen { get; set; } = true;

    public string pinnedTime { get; set; }

    [JsonIgnore]
    [Ignore]
    public long pinnedTimeStamp
    {
      get
      {
        if (string.IsNullOrEmpty(this.pinnedTime))
          return 0;
        if (this.pinnedTime == "-1")
          return -1;
        DateTime result;
        return DateTime.TryParse(this.pinnedTime, out result) && result.Year > 2000 ? Utils.GetTimeStampInSecond(new DateTime?(result)) : 0L;
      }
      set
      {
        if (value < 0L)
          this.pinnedTime = "-1";
        else if (value == 0L)
        {
          this.pinnedTime = (string) null;
        }
        else
        {
          DateTime date = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds((double) value);
          this.pinnedTime = date.ToString(UtcDateTimeConverter.GetConverterValue(date));
        }
      }
    }

    [JsonIgnore]
    [Ignore]
    public bool ParseTz
    {
      get
      {
        bool? nullable = this.isAllDay;
        if (((int) nullable ?? 1) != 0)
          return false;
        nullable = this.isFloating;
        return !nullable.GetValueOrDefault();
      }
    }

    [JsonIgnore]
    public string Actions { get; set; }

    [JsonIgnore]
    public string Resources { get; set; }

    [JsonIgnore]
    public string label { get; set; }

    public string GetTaskId() => this.id;

    public string GetAttendId() => this.attendId;

    public TaskModel Clone()
    {
      TaskModel taskModel1 = new TaskModel();
      taskModel1._Id = this._Id;
      taskModel1.id = this.id;
      taskModel1.projectId = this.projectId;
      taskModel1.priority = this.priority;
      taskModel1.title = this.title;
      taskModel1.content = this.content;
      taskModel1.isAllDay = this.isAllDay;
      taskModel1.repeatFlag = this.repeatFlag;
      taskModel1.completedTime = this.completedTime;
      taskModel1.createdTime = this.createdTime;
      taskModel1.startDate = this.startDate;
      taskModel1.dueDate = this.dueDate;
      taskModel1.status = this.status;
      taskModel1.kind = this.kind;
      taskModel1.Color = this.Color;
      taskModel1.sortOrder = this.sortOrder;
      taskModel1.desc = this.desc;
      taskModel1.timeZone = this.timeZone;
      taskModel1.reminder = this.reminder;
      taskModel1.repeatFirstDate = this.repeatFirstDate;
      taskModel1.completedUserId = this.completedUserId;
      taskModel1.remindTime = this.remindTime;
      taskModel1.etag = this.etag;
      taskModel1.progress = this.progress;
      taskModel1.modifiedTime = this.modifiedTime;
      taskModel1.location = this.location;
      taskModel1.repeatFrom = this.repeatFrom;
      taskModel1.commentCount = this.commentCount;
      taskModel1.tag = this.tag;
      taskModel1.exDate = this.exDate;
      taskModel1.exDates = this.exDates;
      taskModel1.columnId = this.columnId;
      taskModel1.isFloating = this.isFloating;
      taskModel1.parentId = this.parentId;
      taskModel1.isOpen = this.isOpen;
      taskModel1.pinnedTime = this.pinnedTime;
      taskModel1.imgMode = this.imgMode;
      TaskModel taskModel2 = taskModel1;
      if (this.items != null && this.items.Length != 0)
      {
        TaskDetailItemModel[] taskDetailItemModelArray = new TaskDetailItemModel[this.items.Length];
        int num = 0;
        foreach (TaskDetailItemModel taskDetailItemModel1 in this.items)
        {
          TaskDetailItemModel taskDetailItemModel2 = new TaskDetailItemModel()
          {
            id = taskDetailItemModel1.id,
            sortOrder = taskDetailItemModel1.sortOrder,
            TaskId = taskDetailItemModel1.TaskId,
            TaskServerId = taskDetailItemModel1.TaskServerId
          };
          taskDetailItemModelArray[num++] = taskDetailItemModel2;
        }
        taskModel2.items = taskDetailItemModelArray;
      }
      if (this.reminders != null && this.reminders.Length != 0)
      {
        TaskReminderModel[] taskReminderModelArray = new TaskReminderModel[this.reminders.Length];
        int num = 0;
        foreach (TaskReminderModel reminder in this.reminders)
        {
          TaskReminderModel taskReminderModel1 = new TaskReminderModel();
          taskReminderModel1._Id = reminder._Id;
          taskReminderModel1.id = reminder.id;
          taskReminderModel1.Taskid = reminder.Taskid;
          taskReminderModel1.taskserverid = reminder.taskserverid;
          taskReminderModel1.trigger = reminder.trigger;
          TaskReminderModel taskReminderModel2 = taskReminderModel1;
          taskReminderModelArray[num++] = taskReminderModel2;
        }
        taskModel2.reminders = taskReminderModelArray;
      }
      taskModel2.assignee = this.assignee;
      taskModel2.userId = this.userId;
      return taskModel2;
    }

    public string GetProjectId() => this.projectId;

    public TaskModel Copy() => (TaskModel) this.MemberwiseClone();

    public bool IsAssignToMe() => this.assignee == LocalSettings.Settings.LoginUserId;

    public void AddPinnedSecond()
    {
      long pinnedTimeStamp = this.pinnedTimeStamp;
      if (pinnedTimeStamp <= 0L)
        return;
      this.pinnedTimeStamp = pinnedTimeStamp + 1L;
    }

    public bool CheckEnable(bool defaultValue = false)
    {
      ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(this.projectId);
      return projectById == null ? defaultValue : projectById.IsEnable();
    }

    public TimeData GetTimeData()
    {
      return new TimeData()
      {
        StartDate = this.startDate,
        DueDate = this.dueDate,
        IsAllDay = new bool?(!this.startDate.HasValue || ((int) this.isAllDay ?? 1) != 0),
        RepeatFrom = this.repeatFrom,
        RepeatFlag = this.repeatFlag,
        ExDates = this.exDate != null ? ((IEnumerable<string>) this.exDate).ToList<string>() : new List<string>()
      };
    }

    public void GetStringFromArray()
    {
      if (this.tags != null)
        this.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) this.tags).ToList<string>());
      if (this.exDate != null)
        this.exDates = ExDateSerilizer.ToString(this.exDate);
      if (this.childIds == null)
        return;
      this.childrenString = JsonConvert.SerializeObject((object) this.childIds);
    }
  }
}
