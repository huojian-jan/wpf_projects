// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TaskBaseViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TaskBaseViewModel : BaseViewModel, INode, AgendaHelper.IAgenda
  {
    public BlockingList<TaskBaseViewModel> CheckItems;
    public TaskBaseViewModel OwnerTask;
    private DateTime? _startDate;
    private DateTime? _dueDate;
    private DateTime? _completedTime;
    private DateTime? _modifiedTime;
    private DateTime? _createdTime;
    private DateTime? _remindTime;
    private long _sortOrder;
    private int _priority;
    private int _progress;
    private int _status;
    private int _deleted;
    private string _projectId;
    private string _timeZoneName = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
    private string _title;
    private string _content;
    private string _desc;
    private string _repeatFlag;
    private string _repeatFrom;
    private string _kind;
    private string _color;
    private string _projectName;
    private string _assignee;
    private string _tag;
    private string _commentCount;
    private bool? _isAllDay;
    private bool _editable = true;
    private DisplayType _type;
    private int _imageMode;
    public string Resources;
    public string Actions;
    public string Label;
    private int _propertyCheckCode;

    public bool IsTaskOrNote => this.Type == DisplayType.Task || this.Type == DisplayType.Note;

    public bool IsNote => this.Type == DisplayType.Note;

    public bool IsTask => this.Kind != "NOTE";

    public bool IsCheckItem => this.Type == DisplayType.CheckItem;

    public bool IsEvent => this.Type == DisplayType.Event;

    public bool IsPomo => this.Type == DisplayType.Pomo;

    public bool IsAgenda => this.Type == DisplayType.Agenda;

    public bool IsCourse => this.Type == DisplayType.Course;

    public bool IsHabit => this.Type == DisplayType.Habit;

    public string Id { get; set; }

    public string AttendId { get; set; }

    public string ParentId { get; set; }

    public long PinnedTime { get; set; }

    public bool IsPinned => this.PinnedTime > 0L;

    public long ProjectOrder { get; set; }

    public string Permission { get; set; }

    public string TeamId { get; set; }

    public string TaskRepeatId { get; set; }

    public string CalendarId { get; set; }

    public string ColumnId { get; set; }

    public string ColumnName { get; set; }

    public long ParentOrder { get; set; }

    public bool IsOpen { get; set; }

    public string ChildrenIds { get; set; }

    public bool HasLocation { get; set; }

    public bool IsFloating { get; set; }

    public string[] Tags { get; set; }

    public string ReminderString { get; set; }

    public DisplayType Type
    {
      get => this._type;
      set
      {
        this._type = value;
        this.OnPropertyChanged(nameof (Type));
      }
    }

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = Utils.IsEmptyDate(value) ? new DateTime?() : value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? DueDate
    {
      get => this._dueDate;
      set
      {
        this._dueDate = value;
        this.OnPropertyChanged(nameof (DueDate));
      }
    }

    public string CompletedUser { get; set; }

    public string Creator { get; set; }

    public DateTime? CompletedTime
    {
      get => this._completedTime;
      set
      {
        this._completedTime = value;
        this.OnPropertyChanged(nameof (CompletedTime));
      }
    }

    public DateTime? ModifiedTime
    {
      get => this._modifiedTime;
      set
      {
        this._modifiedTime = value;
        this.OnPropertyChanged(nameof (ModifiedTime));
      }
    }

    public DateTime? CreatedTime
    {
      get => this._createdTime;
      set
      {
        this._createdTime = value;
        this.OnPropertyChanged(nameof (CreatedTime));
      }
    }

    public DateTime? RemindTime
    {
      get => this._remindTime;
      set
      {
        this._remindTime = value;
        this.OnPropertyChanged(nameof (RemindTime));
      }
    }

    public long SortOrder
    {
      get => this._sortOrder;
      set
      {
        this._sortOrder = value;
        this.OnPropertyChanged(nameof (SortOrder));
        if (this.Type == DisplayType.CheckItem)
          this.OwnerTask?.OnPropertyChanged("CheckItemContent");
        this.CheckItems?.Value.ForEach((Action<TaskBaseViewModel>) (c => c.ParentOrder = value));
      }
    }

    public int Priority
    {
      get => this._priority;
      set
      {
        this._priority = value;
        this.SetCheckItemPriority();
        this.OnPropertyChanged(nameof (Priority));
      }
    }

    public int Progress
    {
      get => this._progress;
      set
      {
        this._progress = value;
        this.OnPropertyChanged(nameof (Progress));
      }
    }

    public int Status
    {
      get => this.Type != DisplayType.Note ? this._status : 0;
      set
      {
        this._status = value;
        this.OnPropertyChanged(nameof (Status));
      }
    }

    public int Deleted
    {
      get => this._deleted;
      set
      {
        this._deleted = value;
        int num;
        if (value == 0)
        {
          ProjectModel projectById = CacheManager.GetProjectById(this._projectId);
          num = projectById != null ? (projectById.IsEnable() ? 1 : 0) : 0;
        }
        else
          num = 0;
        this.Editable = num != 0;
        this.OnPropertyChanged(nameof (Deleted));
      }
    }

    public string ProjectId
    {
      get => this._projectId;
      set
      {
        if (!(this._projectId != value))
          return;
        this._projectId = value;
        this.SetProject(value);
        this.OnPropertyChanged(nameof (ProjectId));
      }
    }

    public void SetProject(string projectId)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById == null)
        return;
      this.Color = projectById.color;
      this.ProjectName = projectById.Isinbox ? Utils.GetString("Inbox") : projectById.name;
      this.ProjectOrder = projectById.sortOrder;
      this.Permission = projectById.permission;
      this.TeamId = projectById.teamId;
      this.Editable = this.Deleted == 0 && projectById.IsEnable();
      this.SetItemsProject();
    }

    public List<TitleChunk> TitleChunks { get; set; }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
        this.OnPropertyChanged("TitleWithoutLink");
        if (this.Type == DisplayType.CheckItem)
          this.OwnerTask?.OnPropertyChanged("CheckItemContent");
        this.TitleChunks = (List<TitleChunk>) null;
      }
    }

    public string TitleWithoutLink => TaskUtils.EscapeLinkContent(this._title);

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public string Desc
    {
      get => this._desc;
      set
      {
        this._desc = value;
        this.OnPropertyChanged(nameof (Desc));
      }
    }

    public string RepeatFlag
    {
      get => this._repeatFlag;
      set
      {
        this._repeatFlag = value;
        this.OnPropertyChanged(nameof (RepeatFlag));
      }
    }

    public string RepeatFrom
    {
      get => this._repeatFrom;
      set
      {
        this._repeatFrom = value;
        this.OnPropertyChanged(nameof (RepeatFrom));
      }
    }

    public string CommentCount
    {
      get => this._commentCount;
      set
      {
        this._commentCount = value;
        this.OnPropertyChanged(nameof (CommentCount));
        this.NotifyCommentsChanged();
      }
    }

    public string Kind
    {
      get => this._kind;
      set
      {
        this._kind = value;
        this.Type = string.IsNullOrEmpty(this.AttendId) ? (this._kind == "NOTE" ? DisplayType.Note : DisplayType.Task) : DisplayType.Agenda;
        this.OnPropertyChanged(nameof (Kind));
      }
    }

    public string TimeZoneName
    {
      get => this._timeZoneName;
      set
      {
        this._timeZoneName = value;
        this.OnPropertyChanged(nameof (TimeZoneName));
      }
    }

    public string Color
    {
      get => this._color;
      set
      {
        string str = value;
        if (str != null && (str.Length == 6 || str.Length == 8) && !str.StartsWith("#"))
          str = "#" + str;
        this._color = str;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public string ProjectName
    {
      get
      {
        if (string.IsNullOrEmpty(this._projectName))
          this._projectName = CacheManager.GetProjectById(this.ProjectId)?.name;
        return this._projectName;
      }
      set
      {
        this._projectName = value;
        this.OnPropertyChanged(nameof (ProjectName));
      }
    }

    public string Assignee
    {
      get => this._assignee;
      set
      {
        if (!(this._assignee != value))
          return;
        this._assignee = value;
        this.OnPropertyChanged(nameof (Assignee));
      }
    }

    public string Tag
    {
      get => this._tag;
      set
      {
        this._tag = value;
        this.SetCheckItemTag();
        this.OnPropertyChanged(nameof (Tag));
      }
    }

    public bool Editable
    {
      get => this._editable;
      set
      {
        if (this._editable == value)
          return;
        this._editable = value;
        this.OnPropertyChanged(nameof (Editable));
      }
    }

    public bool? IsAllDay
    {
      get => this._isAllDay;
      set
      {
        this._isAllDay = value;
        this.OnPropertyChanged(nameof (IsAllDay));
      }
    }

    public int ImageMode
    {
      get => this._imageMode;
      set
      {
        this._imageMode = value;
        this.OnPropertyChanged(nameof (ImageMode));
      }
    }

    public string ExDates { get; set; }

    public string EntityId { get; set; }

    public TaskBaseViewModel()
    {
    }

    public TaskBaseViewModel(TaskModel task)
    {
      ProjectModel projectById = CacheManager.GetProjectById(task.projectId);
      if (projectById != null)
      {
        if (projectById.color != null && (projectById.color.Length == 6 || projectById.color.Length == 8) && !projectById.color.StartsWith("#"))
          projectById.color = "#" + projectById.color;
        this._color = projectById.color;
        this._projectName = projectById.Isinbox ? Utils.GetString("Inbox") : projectById.name;
        this.ProjectOrder = projectById.sortOrder;
        this.Permission = projectById.permission;
        this.TeamId = projectById.teamId;
      }
      this._editable = task.deleted == 0 && projectById != null && projectById.IsEnable();
      this.Id = task.id;
      this.SortOrder = task.sortOrder;
      this._projectId = task.projectId;
      this.AttendId = task.attendId;
      this.ParentId = task.parentId;
      this.PinnedTime = task.pinnedTimeStamp;
      this.ColumnId = task.columnId;
      this.TimeZoneName = task.timeZone;
      this.IsOpen = task.isOpen;
      this.ChildrenIds = task.childrenString;
      this.HasLocation = task.hasLocation == 1;
      this.IsFloating = task.isFloating.GetValueOrDefault();
      this.ExDates = task.exDates;
      this._startDate = task.startDate;
      this._dueDate = task.dueDate;
      this._completedTime = task.completedTime;
      this.CompletedUser = task.completedUserId;
      this._modifiedTime = task.modifiedTime;
      this._createdTime = task.createdTime;
      this._remindTime = task.remindTime;
      this._priority = task.priority;
      this._progress = task.progress.GetValueOrDefault();
      this._status = task.status;
      this._deleted = task.deleted;
      this._title = task.title;
      this.Content = task.content;
      this.ImageMode = task.imgMode;
      this._desc = task.desc;
      this._repeatFlag = task.repeatFlag;
      this._repeatFrom = task.repeatFrom;
      this._kind = task.kind;
      this._type = string.IsNullOrEmpty(task.attendId) ? (this.Kind == "NOTE" ? DisplayType.Note : DisplayType.Task) : DisplayType.Agenda;
      this._assignee = task.assignee;
      this._tag = task.tag;
      this._isAllDay = task.isAllDay;
      this._commentCount = task.commentCount;
      this.Tags = TagSerializer.ToTags(task.tag).ToArray();
      this.TaskRepeatId = task.repeatTaskId;
      this.Creator = task.creator;
      this.Resources = task.Resources;
      this.Actions = task.Actions;
      this.Label = task.label;
    }

    public TaskBaseViewModel(TaskDetailItemModel item) => this.BuildCheckItem(item, true);

    public void RemoveParent()
    {
      int num = this.IsTaskOrNote ? 1 : 0;
    }

    public TaskBaseViewModel Copy() => (TaskBaseViewModel) this.MemberwiseClone();

    public bool IsAssignToMe() => this.Assignee == LocalSettings.Settings.LoginUserId;

    public void SetProperties(TaskBaseViewModel model)
    {
      foreach (PropertyInfo property in this.GetType().GetProperties())
      {
        object obj1 = property.GetValue((object) this);
        object obj2 = property.GetValue((object) model);
        if ((obj1 != null || obj2 != null) && (obj1 == null || obj2 == null || !obj1.Equals(obj2)) && property.GetSetMethod() != (MethodInfo) null)
          property.SetValue((object) this, obj2);
      }
    }

    public CourseDisplayModel Course { get; set; }

    public TaskBaseViewModel(CourseDisplayModel course)
    {
      this.Id = course.UniqueId;
      this._type = DisplayType.Course;
      this.Course = course;
      this.Title = course.Title;
      this.Content = course.Room + (string.IsNullOrEmpty(course.Room) ? "" : " ") + course.Teacher;
      this.IsAllDay = new bool?(false);
      this.StartDate = new DateTime?(course.CourseStart);
      this.DueDate = new DateTime?(course.CourseEnd);
      this._projectId = course.ScheduleId;
      this._projectName = course.ScheduleName;
      this._color = course.Color;
      this.ReminderString = course.Reminders == null ? (string) null : JsonConvert.SerializeObject((object) course.Reminders);
    }

    public TaskBaseViewModel(CalendarEventModel model, string id = null)
    {
      this.Id = id ?? model.Id;
      this.EntityId = model.Id;
      this._type = DisplayType.Event;
      this.Title = model.Title;
      this.Content = model.Content;
      this.IsAllDay = new bool?(model.IsAllDay);
      this.StartDate = model.DueStart;
      this.DueDate = model.DueEnd;
      this._projectId = "8ac3038d93c54b80a67321b6a03df066";
      this.RepeatFlag = model.RepeatFlag;
      this._repeatFrom = "0";
      this.CalendarId = model.CalendarId;
      this.ReminderString = model.Reminders;
      BindCalendarModel bindCalendarById = CacheManager.GetBindCalendarById(model.CalendarId);
      this.Editable = bindCalendarById != null && bindCalendarById.Accessible;
    }

    public TaskBaseViewModel(Section section)
    {
      this._title = section.Name;
      this.Id = section.SectionId;
      this._type = DisplayType.Section;
      this._projectId = section.ProjectId;
      this._sortOrder = section.Ordinal;
    }

    public async Task<TaskModel> ToTask()
    {
      if (!this.IsTaskOrNote)
        return (TaskModel) null;
      TaskModel taskById = await TaskDao.GetTaskById(this.Id);
      if (taskById == null)
        return (TaskModel) null;
      taskById.sortOrder = this.SortOrder;
      taskById.projectId = this.ProjectId;
      taskById.attendId = this.AttendId;
      taskById.parentId = this.ParentId;
      taskById.pinnedTimeStamp = this.PinnedTime;
      taskById.columnId = this.ColumnId;
      taskById.timeZone = this.TimeZoneName;
      taskById.isOpen = this.IsOpen;
      taskById.childrenString = this.ChildrenIds;
      taskById.isFloating = new bool?(this.IsFloating);
      taskById.startDate = this._startDate;
      taskById.dueDate = this._dueDate;
      taskById.completedTime = this._completedTime;
      taskById.completedUserId = this.CompletedUser;
      taskById.modifiedTime = this._modifiedTime;
      taskById.remindTime = this._remindTime;
      taskById.priority = this._priority;
      taskById.progress = new int?(this._progress);
      taskById.status = this._status;
      taskById.deleted = this._deleted;
      taskById.title = this._title;
      taskById.content = this.Content;
      taskById.desc = this._desc;
      taskById.repeatFlag = this._repeatFlag;
      taskById.repeatFrom = this._repeatFrom;
      taskById.kind = this._kind;
      taskById.assignee = this._assignee;
      taskById.tag = this._tag;
      taskById.isAllDay = this._isAllDay;
      return taskById;
    }

    public string GetTaskId() => !this.IsCheckItem ? this.Id : this.ParentId;

    public string GetAttendId() => this.AttendId;

    public void BuildCheckItem(TaskDetailItemModel item, bool setItem = false)
    {
      this.Id = item.id;
      this.Type = DisplayType.CheckItem;
      this.SortOrder = item.sortOrder;
      this.ParentId = item.TaskServerId;
      this.StartDate = item.startDate;
      this.CompletedTime = item.completedTime;
      this.Status = item.status;
      this.Title = item.title;
      this.IsAllDay = item.isAllDay;
      this.RemindTime = item.snoozeReminderTime;
      this.SetOwnerTask(item.TaskServerId, setItem);
    }

    public void SetOwnerTask(string taskId, bool setItem)
    {
      TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
      if (taskById == null)
        return;
      this.Color = taskById._color;
      this.ProjectName = taskById._projectName;
      this.ProjectId = taskById._projectId;
      this.ProjectOrder = taskById.ProjectOrder;
      this.Permission = taskById.Permission;
      this.TeamId = taskById.TeamId;
      this.ColumnId = taskById.ColumnId;
      this.Editable = taskById.Editable;
      this.Priority = taskById._priority;
      this.Tags = taskById.Tags;
      this.Tag = taskById._tag;
      this.ParentOrder = taskById.SortOrder;
      if (setItem)
      {
        this.OwnerTask = taskById;
        taskById.AddItems(this);
      }
      else
        this.OwnerTask?.CheckItem(this);
    }

    private void CheckItem(TaskBaseViewModel item)
    {
      if (item == null)
        return;
      this.CheckItems.RemoveAll((Predicate<TaskBaseViewModel>) (c => c.Id == item.Id && !c.Equals((object) item)));
      if (this.CheckItems?.FirstOrDefault((Func<TaskBaseViewModel, bool>) (c => c.Id == item.Id)) != null)
        return;
      this.CheckItems?.Add(item);
      this.OnPropertyChanged("CheckItemContent");
      this.OnPropertyChanged("CheckItems");
    }

    private void AddItems(TaskBaseViewModel item)
    {
      if (item == null)
        return;
      if (this.CheckItems == null)
        this.CheckItems = new BlockingList<TaskBaseViewModel>();
      this.CheckItems.RemoveAll((Predicate<TaskBaseViewModel>) (c => c.Id == item.Id && !c.Equals((object) item)));
      if (!this.CheckItems.All((Func<TaskBaseViewModel, bool>) (i => i.Id != item.Id)))
        return;
      this.CheckItems.Add(item);
      this.OnPropertyChanged("CheckItemContent");
      this.OnPropertyChanged("CheckItems");
    }

    public void RemoveCheckItem(TaskBaseViewModel value)
    {
      if (this.CheckItems == null)
        return;
      this.CheckItems.Remove(value);
      this.OnPropertyChanged("CheckItemContent");
      this.OnPropertyChanged("CheckItems");
    }

    private void SetItemsProject()
    {
      if (this.CheckItems == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in this.CheckItems.Value)
      {
        taskBaseViewModel.Color = this._color;
        taskBaseViewModel.ProjectName = this._projectName;
        taskBaseViewModel.ProjectId = this._projectId;
        taskBaseViewModel.ProjectOrder = this.ProjectOrder;
        taskBaseViewModel.Permission = this.Permission;
        taskBaseViewModel.TeamId = this.TeamId;
        taskBaseViewModel.Editable = this.Editable;
      }
    }

    private void SetCheckItemTag()
    {
      if (this.CheckItems == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in this.CheckItems.Value)
      {
        taskBaseViewModel.Tags = this.Tags;
        taskBaseViewModel.Tag = this._tag;
      }
    }

    private void SetCheckItemPriority()
    {
      if (this.CheckItems == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in this.CheckItems.Value)
        taskBaseViewModel.Priority = this._priority;
    }

    public void SetTags(List<string> tags)
    {
      // ISSUE: explicit non-virtual call
      if (tags != null && __nonvirtual (tags.Count) > 0)
      {
        this.Tags = tags.Select<string, string>((Func<string, string>) (t => t.ToLower())).ToArray<string>();
        this.Tag = TagSerializer.ToJsonContent(tags);
      }
      else
      {
        this.Tags = new string[0];
        this.Tag = (string) null;
      }
    }

    public void CheckCheckItems(List<TaskDetailItemModel> items)
    {
      if (this.CheckItems == null)
        this.CheckItems = new BlockingList<TaskBaseViewModel>();
      if (items == null)
        return;
      List<TaskBaseViewModel> taskBaseViewModelList = this.CheckItems.Where((Predicate<TaskBaseViewModel>) (c => items.All<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (i => i.id != c.Id))));
      List<TaskDetailItemModel> list = items.Where<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (i => this.CheckItems.ToList().All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (c => i.id != c.Id)))).ToList<TaskDetailItemModel>();
      foreach (TaskBaseViewModel m in taskBaseViewModelList)
      {
        this.CheckItems.Remove(m);
        TaskDetailItemCache.DeleteCheckItemById(m.Id);
      }
      foreach (TaskDetailItemModel taskDetailItemModel in list)
      {
        TaskBaseViewModel checkItemById = TaskDetailItemCache.GetCheckItemById(taskDetailItemModel?.id);
        if (checkItemById == null)
          TaskDetailItemCache.AddCheckItemToDict(taskDetailItemModel);
        else
          this.CheckItems.Add(checkItemById);
      }
    }

    public void NotifyCommentsChanged() => this.OnPropertyChanged("Comments");

    public string GetAssignee() => !string.IsNullOrEmpty(this.Assignee) ? this.Assignee : "-1";

    internal bool InToday()
    {
      if (!this.StartDate.HasValue)
        return false;
      if (!this.DueDate.HasValue)
        return this.StartDate.Value.Date == DateTime.Today;
      return this.StartDate.Value.Date <= DateTime.Today && this.DueDate.Value.Date >= DateTime.Today;
    }

    public bool OutDate()
    {
      if (!this.StartDate.HasValue)
        return false;
      return !this.DueDate.HasValue ? this.StartDate.Value.Date < DateTime.Today : this.DueDate.Value <= DateTime.Today;
    }

    public void SetDependenceModel(TaskBaseViewModel displayTask, params string[] properties)
    {
      if (displayTask == null || properties == null || properties.Length == 0)
        return;
      foreach (string property1 in properties)
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) displayTask, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
        {
          PropertyInfo property2 = this.GetType().GetProperty(e.PropertyName);
          if (!(property2 != (PropertyInfo) null))
            return;
          property2.SetValue((object) this, property2.GetValue(o));
        }), property1);
    }

    public bool IsPropertiesEqual(TaskBaseViewModel model)
    {
      if (model == null)
        return false;
      if (this._propertyCheckCode == 0)
        this._propertyCheckCode = (this.Id + this._startDate.ToString() + this._dueDate.ToString() + this._isAllDay.ToString() + this._timeZoneName + this._repeatFlag + this._repeatFrom + this._title).GetHashCode();
      if (model._propertyCheckCode == 0)
        model._propertyCheckCode = (model.Id + model._startDate.ToString() + model._dueDate.ToString() + model._isAllDay.ToString() + model._timeZoneName + model._repeatFlag + model._repeatFrom + model._title).GetHashCode();
      return this._propertyCheckCode == model._propertyCheckCode;
    }

    public List<TitleChunk> GetTitleNum()
    {
      if (this.TitleChunks == null)
      {
        string title = this.Title;
        this.TitleChunks = title != null ? title.CompareNumber() : (List<TitleChunk>) null;
      }
      return this.TitleChunks;
    }
  }
}
