// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.AddTaskViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class AddTaskViewModel : BaseViewModel
  {
    private string _calendarId;
    private string _dayText = string.Empty;
    private string _detailDayText = string.Empty;
    private string _hint;
    private string _kind = "TEXT";
    private List<string> _originalTag = new List<string>();
    private int _priority;
    private string _projectId;
    private string _projectName;
    private string _assignee;
    private bool _selectProject;
    private bool _isNote;
    private DateTime? _startDate;
    private List<string> _tags = new List<string>();
    private TimeData _timeData;
    private static Section _section;

    public ObservableCollection<AddTaskAttachmentInfo> Files { get; } = new ObservableCollection<AddTaskAttachmentInfo>();

    public AddTaskViewModel()
    {
    }

    public AddTaskViewModel(IProjectTaskDefault taskDefault)
    {
      this._projectId = taskDefault.GetProjectId();
      this._projectName = taskDefault.GetProjectName();
      this._timeData = taskDefault.GetTimeData();
      this._priority = taskDefault.GetPriority();
      this._tags = taskDefault.GetTags();
      this.ColumnId = taskDefault.GetColumnId();
      this.IsCalendar = taskDefault.IsCalendar();
      this.AccountId = taskDefault.GetAccountId();
      this.CalendarName = taskDefault.GetProjectName();
      this._originalTag = taskDefault.GetTags().ToList<string>();
      this._isNote = taskDefault.GetIsNote();
      this._assignee = taskDefault.GetAssignee();
      this.TaskDefault = taskDefault;
    }

    public List<string> OriginalTags
    {
      get => this._originalTag.ToList<string>();
      set => this._originalTag = value;
    }

    public string OriginalProjectId { get; set; }

    private int OriginalPriority { get; set; }

    public TimeData OriginalTimeData { get; set; }

    private string OriginalProjectName { get; set; }

    public string ColumnId { get; set; }

    public string AddToColumnId { get; set; }

    public string CalendarName { get; set; }

    public string OriginalCalendarName { get; set; }

    public bool IsOriginalNote { get; set; }

    public string OriginalCalendarId { get; set; }

    public bool IsCalendar { get; set; }

    public string AccountId { get; set; }

    public bool IsPin { get; set; }

    public bool IsComplete { get; set; }

    public string CalendarId
    {
      get => this._calendarId;
      set
      {
        this._calendarId = value;
        this.OnPropertyChanged(nameof (CalendarId));
      }
    }

    public bool IsNote
    {
      get => this._isNote;
      set
      {
        this._isNote = value;
        this.OnPropertyChanged(nameof (IsNote));
      }
    }

    public int Priority
    {
      get => this._priority;
      set
      {
        this._priority = value;
        this.OnPropertyChanged(nameof (Priority));
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
        this.OnPropertyChanged(nameof (ProjectId));
      }
    }

    public string ProjectName
    {
      get => this._projectName;
      set
      {
        if (!(this._projectId != value))
          return;
        this._projectName = value;
        this.ResetHint();
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

    public bool SelectProject
    {
      get => true;
      set
      {
        this._selectProject = value;
        this.OnPropertyChanged(nameof (SelectProject));
      }
    }

    public string DetailDayText
    {
      get => this._detailDayText;
      set
      {
        this._detailDayText = value;
        this.OnPropertyChanged(nameof (DetailDayText));
      }
    }

    public Geometry DayTextIcon
    {
      get
      {
        return !this.StartDate.HasValue ? Utils.GetIcon("IcCalendar") : Utils.GetIcon("CalDayIcon" + this.StartDate.Value.Day.ToString());
      }
    }

    public string DayText
    {
      get => this._dayText;
      set
      {
        this._dayText = value;
        this.OnPropertyChanged("DayTextIcon");
      }
    }

    public DateTime? StartDate
    {
      get => this._timeData?.StartDate;
      set
      {
        if (this._timeData != null)
        {
          this._timeData.StartDate = value;
          if (!value.HasValue)
            this._timeData.DueDate = new DateTime?();
        }
        this.OnPropertyChanged(nameof (StartDate));
        this.RefreshDayText(this._timeData);
      }
    }

    public TimeData TimeData
    {
      get => this._timeData;
      set
      {
        this._timeData = value;
        this.StartDate = (DateTime?) value?.StartDate;
        this.ResetHint();
        this.OnPropertyChanged("ShowRepeat");
      }
    }

    public bool ShowRepeat => !string.IsNullOrEmpty(this._timeData?.RepeatFlag);

    public string Hint
    {
      get => this._hint;
      set
      {
        this._hint = value;
        this.OnPropertyChanged(nameof (Hint));
      }
    }

    public string Kind
    {
      get => this._kind;
      set
      {
        this._kind = value;
        this.OnPropertyChanged(nameof (Kind));
      }
    }

    public List<string> Tags
    {
      set
      {
        this._tags = value;
        this.ResetHint();
      }
      get => this._tags;
    }

    public static AddTaskViewModel Build(IProjectTaskDefault taskDefault, Section section = null)
    {
      if (taskDefault == null)
        return new AddTaskViewModel();
      AddTaskViewModel model = new AddTaskViewModel(taskDefault);
      AddTaskViewModel._section = section;
      if (section != null)
        AddTaskViewModel.SetSectionProperty(model, section);
      if (model.IsNote && model.TimeData != null)
      {
        model.TimeData.DueDate = new DateTime?();
        model.TimeData.Reminders = (List<TaskReminderModel>) null;
      }
      if (!string.IsNullOrEmpty(model.AccountId))
      {
        BindCalendarModel defaultCalendar = SubscribeCalendarHelper.GetDefaultCalendar(model.AccountId);
        if (defaultCalendar != null)
          model.CalendarId = defaultCalendar.Id;
      }
      model.UseTagHint = !taskDefault.UseDefaultTags();
      if (!string.IsNullOrEmpty(taskDefault.GetColumnId()))
        model.Hint = Utils.GetString(model.IsNote ? "NewNote" : "NewTask");
      else
        model.ResetHint();
      if (model.IsCalendar)
        model.Hint = string.Format(Utils.GetString("AddAgendaTo"), (object) model.ProjectName);
      return model;
    }

    private static void SetSectionProperty(AddTaskViewModel model, Section section)
    {
      if (section is NoteSection)
        model._isNote = true;
      if (model.IsNote)
        model._priority = 0;
      if (section is PrioritySection && !model._isNote)
      {
        int priority = section.GetPriority();
        model._priority = priority;
      }
      if (section is ProjectSection)
      {
        model._projectId = section.GetProjectId();
        model._projectName = section.Name;
      }
      if (section is TagSection tagSection)
      {
        string sectionId = tagSection.SectionId;
        model.Tags = new List<string>();
        if (sectionId != "notag")
          model.Tags.Add(sectionId);
      }
      if (section != null && section.SectionDate.HasValue)
        model.StartDate = (DateTime?) section?.SectionDate;
      if (section is NodateSection)
        model.StartDate = new DateTime?();
      if (section is PinnedSection)
        model.IsPin = true;
      if (section is AssigneeSection assigneeSection)
        model.Assignee = assigneeSection.GetAssignee();
      if (!(section is CompletedSection) || model._isNote)
        return;
      model.IsComplete = true;
    }

    public IProjectTaskDefault TaskDefault { get; set; }

    private bool UseTagHint { get; set; }

    public void Init()
    {
      this.OriginalProjectId = this._projectId;
      this.OriginalPriority = this._priority;
      this.OriginalTimeData = TimeData.Clone(this._timeData);
      this.OriginalProjectName = this._projectName;
      this.OriginalCalendarId = this._calendarId;
      this.OriginalCalendarName = this.CalendarName;
      List<string> tags = this._tags;
      this.OriginalTags = (tags != null ? tags.ToList<string>() : (List<string>) null) ?? new List<string>();
      this.IsOriginalNote = this.IsNote;
      this.SelectProject = false;
      this.AutoAddTags = true;
      this.StartDate = (DateTime?) this._timeData?.StartDate;
    }

    public bool AutoAddTags { get; set; }

    public void Reset(IProjectTaskDefault taskDefault)
    {
      this.ProjectId = this.OriginalProjectId;
      this.AddToColumnId = (string) null;
      this.ProjectName = this.OriginalProjectName;
      this.Priority = this.OriginalPriority;
      this.CalendarId = this.OriginalCalendarId;
      this.CalendarName = this.OriginalCalendarName;
      this.Tags = this.OriginalTags;
      this.IsNote = this.IsOriginalNote;
      this.ResetTimeData(taskDefault);
      this.Assignee = taskDefault?.GetAssignee();
    }

    public void ResetProject()
    {
      if (!(this._projectId != this.OriginalProjectId))
        return;
      this.ProjectId = this.OriginalProjectId;
      this.AddToColumnId = (string) null;
      this.ProjectName = this.OriginalProjectName;
      this.IsNote = this.IsOriginalNote;
    }

    public void ResetTimeData(IProjectTaskDefault taskDefault)
    {
      TimeData timeData1 = taskDefault?.GetTimeData() ?? TimeData.Clone(this.OriginalTimeData);
      if (AddTaskViewModel._section != null)
      {
        Section section1 = AddTaskViewModel._section;
        DateTime? nullable1;
        int num;
        if (section1 == null)
        {
          num = 0;
        }
        else
        {
          nullable1 = section1.SectionDate;
          num = nullable1.HasValue ? 1 : 0;
        }
        if (num != 0)
        {
          TimeData timeData2 = timeData1;
          Section section2 = AddTaskViewModel._section;
          DateTime? nullable2;
          if (section2 == null)
          {
            nullable1 = new DateTime?();
            nullable2 = nullable1;
          }
          else
            nullable2 = section2.SectionDate;
          timeData2.StartDate = nullable2;
        }
        if (AddTaskViewModel._section is NodateSection)
        {
          TimeData timeData3 = timeData1;
          nullable1 = new DateTime?();
          DateTime? nullable3 = nullable1;
          timeData3.StartDate = nullable3;
        }
      }
      this.TimeData = timeData1;
    }

    public void ResetPriority() => this.Priority = this.OriginalPriority;

    public void ResetHint()
    {
      try
      {
        if (!string.IsNullOrEmpty(this.ColumnId))
          this.Hint = Utils.GetString(this.IsNote ? "NewNote" : "NewTask");
        else if (this.IsCalendar)
          this.Hint = string.Format(Utils.GetString("AddAgendaTo"), (object) this.CalendarName);
        else if (!this.UseTagHint || this._tags == null || this._tags.Count == 0)
        {
          ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this.ProjectId));
          bool flag = this.IsNote || projectModel != null && projectModel.IsNote;
          TimeData timeData = this._timeData;
          if ((timeData != null ? (timeData.StartDate.HasValue ? 1 : 0) : 0) != 0)
          {
            string str = DateUtils.FormatAddTaskDateString(this._timeData.StartDate.Value);
            this.Hint = string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) str, (object) this._projectName, (object) Utils.GetString(flag ? "Notes" : "Task").ToLower());
          }
          else
            this.Hint = string.Format(Utils.GetString("CenterAddTaskTextBoxPreviewText"), (object) this.ProjectName, (object) Utils.GetString(flag ? "Notes" : "Task").ToLower());
        }
        else
          this.Hint = string.Join(" ", (IEnumerable<string>) this._tags.Select<string, string>((Func<string, string>) (tag => "#" + tag?.Replace("#", string.Empty))).ToList<string>());
      }
      catch (Exception ex)
      {
        this.Hint = "";
      }
    }

    private void RefreshDayText(TimeData timeData)
    {
      this.DetailDayText = timeData == null || !timeData.StartDate.HasValue || Utils.IsEmptyDate(timeData.StartDate) ? string.Empty : DateUtils.FormatQuickAddDateString(timeData.StartDate.Value, timeData.DueDate, timeData.IsAllDay);
      this.OnPropertyChanged("DayTextIcon");
    }

    public bool ProjectChanged() => this._projectId != this.OriginalProjectId;

    public string GetProjectColumnId()
    {
      if (!string.IsNullOrEmpty(this.AddToColumnId))
        return this.AddToColumnId;
      return this.ColumnId == null || this.ColumnId.Contains(":") ? string.Empty : this.ColumnId;
    }

    public void AddFile(string path)
    {
      Constants.AttachmentKind fileType = AttachmentProvider.GetFileType(path);
      FileUtils.CollectFileSize(path);
      if (FileUtils.FileOverSize(path) && fileType != Constants.AttachmentKind.IMAGE)
      {
        Utils.Toast(LocalSettings.Settings.IsPro ? Utils.GetString("AttachmentSizeLimitPro") : Utils.GetString("AttachmentSizeLimit"));
      }
      else
      {
        if (!this.Files.All<AddTaskAttachmentInfo>((Func<AddTaskAttachmentInfo, bool>) (f => f.Path != path)))
          return;
        this.Files.Add(new AddTaskAttachmentInfo(path));
      }
    }

    internal void Remove(AddTaskAttachmentInfo info)
    {
      this.Files.Remove(info);
      try
      {
        FileInfo fileInfo = new FileInfo(info.Path);
        fileInfo.Attributes = FileAttributes.Normal;
        fileInfo.Delete();
      }
      catch (Exception ex)
      {
        UtilLog.Info(ex.Message);
      }
    }

    public void ClearFiles(bool needDelete)
    {
      List<AddTaskAttachmentInfo> list = this.Files.ToList<AddTaskAttachmentInfo>();
      Application.Current?.Dispatcher?.Invoke((Action) (() => this.Files.Clear()));
      if (!needDelete)
        return;
      foreach (AddTaskAttachmentInfo taskAttachmentInfo in list)
      {
        try
        {
          FileInfo fileInfo = new FileInfo(taskAttachmentInfo.Path);
          fileInfo.Attributes = FileAttributes.Normal;
          fileInfo.Delete();
        }
        catch (Exception ex)
        {
          UtilLog.Info(ex.Message);
        }
      }
    }

    public List<AddTaskAttachmentInfo> ClearAndGetFiles()
    {
      List<AddTaskAttachmentInfo> list = this.Files.ToList<AddTaskAttachmentInfo>();
      this.ClearFiles(false);
      return list;
    }

    ~AddTaskViewModel()
    {
      if (this.Files.Count <= 0)
        return;
      this.ClearFiles(true);
    }
  }
}
