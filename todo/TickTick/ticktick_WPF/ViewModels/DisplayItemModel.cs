// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.DisplayItemModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class DisplayItemModel : BaseViewModel, AgendaHelper.IAgenda, INode
  {
    private string _avatarUrl;
    private string _content;
    private int _calendarType;
    private bool _dragging;
    private bool _dropHover;
    private bool _isOpen = true;
    private int _num;
    private bool _selected;
    private bool _showProject = true;
    private bool _lineVisible = true;
    private int _level;
    private bool _showTopMargin = true;
    private bool _showBottomMargin = true;
    public long SpecialOrder;
    public List<string> DragTaskIds;
    private bool _inOperation;
    private bool _inBatchSelected;
    private bool _underTaskItem = true;
    private bool _inGantt;
    private bool _hitVisible = true;
    public bool SortByCTime;
    private string _completionRate;
    private bool _eventAdded;
    private List<TagViewModel> _displayTags;

    public TaskBaseViewModel SourceViewModel { get; set; }

    public string[] Tags => this.SourceViewModel.Tags;

    public string TimeZoneName => this.SourceViewModel.TimeZoneName;

    public Section Section { get; set; }

    public string Id => this.SourceViewModel.Id;

    public string TaskId => this.SourceViewModel.GetTaskId();

    public string Assignee => this.SourceViewModel.Assignee;

    public string RepeatFrom => this.SourceViewModel.RepeatFrom;

    public string ProjectId => this.SourceViewModel.ProjectId;

    public long SortOrder => this.SourceViewModel.SortOrder;

    public string EntityId => this.SourceViewModel.EntityId ?? this.SourceViewModel.Id;

    public DateTime? RemindTime => this.SourceViewModel.RemindTime;

    public DateTime ModifiedTime => this.SourceViewModel.ModifiedTime ?? DateTime.Now;

    public DateTime CreatedTime => this.SourceViewModel.CreatedTime ?? DateTime.Now;

    public long ProjectOrder => this.SourceViewModel.ProjectOrder;

    public long ParentOrder => this.SourceViewModel.ParentOrder;

    public bool ShowLocation => this.SourceViewModel.HasLocation;

    public string Title => this.SourceViewModel.Title;

    public string Color
    {
      get
      {
        return !string.IsNullOrEmpty(this.DisplayColor) ? this.DisplayColor : this.SourceViewModel.Color ?? "Transparent";
      }
    }

    public bool ShowProjectColor
    {
      get
      {
        return !string.IsNullOrEmpty(this.Color) && this.Color.ToLower() != "transparent" && !this.IsCourse;
      }
    }

    public string DisplayColor { get; set; }

    public string CalendarId => this.SourceViewModel.CalendarId;

    public bool ShowDragBar { get; set; } = true;

    public bool BatchMode { get; set; }

    public string AttendId => this.SourceViewModel.AttendId;

    public string ColumnId => this.SourceViewModel.ColumnId;

    public bool InKanban { get; set; }

    public bool InMatrix { get; set; }

    public string TeamId { get; set; }

    public string Permission { get; set; }

    public bool IsFloating => this.SourceViewModel.IsFloating;

    public HabitModel Habit
    {
      get
      {
        if (!this.IsHabit)
          return (HabitModel) null;
        return !(this.SourceViewModel is HabitBaseViewModel sourceViewModel) ? (HabitModel) null : sourceViewModel.Habit;
      }
    }

    public int OriginLevel { get; set; }

    public bool IsPinned => this.PinnedTime > 0L;

    public DisplayItemModel Parent { get; set; }

    public string ReminderString { get; set; }

    public string CompletionRate
    {
      get => this._completionRate;
      set
      {
        this._completionRate = value;
        this.OnPropertyChanged(nameof (CompletionRate));
      }
    }

    public ProjectIdentity CurrentProjectIdentity { get; set; }

    public bool InGantt
    {
      get => this._inGantt;
      set
      {
        this._inGantt = value;
        this.OnPropertyChanged(nameof (InGantt));
      }
    }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
        this.OnPropertyChanged("ShowContent");
      }
    }

    public bool HitVisible
    {
      get => this._hitVisible;
      set
      {
        if (this._hitVisible == value)
          return;
        this._hitVisible = value;
        this.OnPropertyChanged(nameof (HitVisible));
      }
    }

    public long PinnedTime => this.SourceViewModel.PinnedTime;

    public string ProjectName => this.SourceViewModel.ProjectName;

    public bool ShowTopMargin
    {
      get => this._showTopMargin;
      set
      {
        if (this._showTopMargin == value)
          return;
        this._showTopMargin = value;
        this.OnPropertyChanged(nameof (ShowTopMargin));
      }
    }

    public bool ShowBottomMargin
    {
      get => this._showBottomMargin;
      set
      {
        if (this._showBottomMargin == value)
          return;
        this._showBottomMargin = value;
        this.OnPropertyChanged(nameof (ShowBottomMargin));
      }
    }

    public bool Enable => this.SourceViewModel.Editable;

    public bool LineVisible
    {
      get => this._lineVisible;
      set
      {
        this._lineVisible = value;
        this.OnPropertyChanged(nameof (LineVisible));
      }
    }

    public HabitCheckInModel HabitCheckIn
    {
      get
      {
        if (!this.IsHabit)
          return (HabitCheckInModel) null;
        return !(this.SourceViewModel is HabitBaseViewModel sourceViewModel) ? (HabitCheckInModel) null : sourceViewModel.HabitCheckIn;
      }
    }

    public int CalendarType
    {
      get => this._calendarType;
      set
      {
        this._calendarType = value;
        this.OnPropertyChanged(nameof (CalendarType));
      }
    }

    public bool DropHover
    {
      get => this._dropHover;
      set
      {
        if (this._dropHover == value)
          return;
        this._dropHover = value;
        this.OnPropertyChanged(nameof (DropHover));
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

    public List<TagViewModel> DisplayTags
    {
      get
      {
        if (this._displayTags == null)
          this._displayTags = DisplayItemModel.BuildTagModels((IReadOnlyList<string>) this.SourceViewModel.Tags, this.InKanban, this.InMatrix, !this.InKanban || LocalSettings.Settings.ExtraSettings.KbSize != 0 ? 2 : 1);
        return this._displayTags;
      }
    }

    public void SetDisplayTags(int maxCount)
    {
      this._displayTags = DisplayItemModel.BuildTagModels((IReadOnlyList<string>) this.SourceViewModel.Tags, this.InKanban, this.InMatrix, maxCount);
      this.OnPropertyChanged("DisplayTags");
    }

    public int Progress => this.SourceViewModel.Progress;

    public bool ShowTag
    {
      get
      {
        return this.SourceViewModel.Tags != null && ((IEnumerable<string>) this.SourceViewModel.Tags).Any<string>();
      }
    }

    public bool ShowProgress => this.Progress > 0 && this.Status == 0;

    public string ProgressContent => this.Progress.ToString() + "%";

    public bool IsCustomizedSection => this.Type == DisplayType.Section && this.Section.Customized;

    public string AvatarUrl
    {
      get => this._avatarUrl ?? string.Empty;
      set
      {
        this._avatarUrl = value;
        this.OnPropertyChanged(nameof (AvatarUrl));
        this.SetAvatar();
      }
    }

    public BitmapImage Avatar { get; set; }

    public async void TrySetAvatar()
    {
      if (string.IsNullOrEmpty(this.AvatarUrl) || this.Avatar != null)
        return;
      this.SetAvatar();
    }

    public async Task SetAvatar()
    {
      DisplayItemModel displayItemModel = this;
      displayItemModel.Avatar = AvatarHelper.GetAvatarByUrl(displayItemModel.AvatarUrl, getDefault: false);
      if (displayItemModel.Avatar == null)
      {
        BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(displayItemModel.AvatarUrl);
        displayItemModel.Avatar = avatarByUrlAsync;
        displayItemModel.OnPropertyChanged("Avatar");
      }
      else
        displayItemModel.OnPropertyChanged("Avatar");
    }

    public int Deleted => this.SourceViewModel.Deleted;

    public string RepeatFlag => this.SourceViewModel.RepeatFlag;

    public string Kind => this.SourceViewModel.Kind;

    public DateTime? DueDate
    {
      get => this.ParseData != null ? this.ParseData.DueDate : this.SourceViewModel.DueDate;
    }

    public DateTime? StartDate
    {
      get => this.ParseData != null ? this.ParseData.StartDate : this.SourceViewModel.StartDate;
    }

    public DateTime? CompletedTime => this.SourceViewModel.CompletedTime;

    public int Num
    {
      get => this._num;
      set
      {
        this._num = value;
        this.OnPropertyChanged(nameof (Num));
      }
    }

    public int Priority => this.SourceViewModel.Priority;

    public bool IsOpen
    {
      get => this._isOpen;
      set
      {
        this._isOpen = value;
        this.OnPropertyChanged(nameof (IsOpen));
      }
    }

    public bool IsAbandoned => this.Status == -1;

    public bool IsCompleted => this.Status == 2;

    public int Status => !this.Linked ? this.SourceViewModel.Status : 2;

    public bool? IsAllDay
    {
      get => this.ParseData != null ? this.ParseData.IsAllDay : this.SourceViewModel.IsAllDay;
    }

    public DateTime? NoteDisplayDate
    {
      get
      {
        if (!this.IsNote)
          return new DateTime?();
        return !this.SortByCTime ? this.SourceViewModel.ModifiedTime : this.SourceViewModel.CreatedTime;
      }
    }

    public bool ShowNoteDate => LocalSettings.Settings.ShowDetails && !this.InMatrix;

    public bool? ShowAttachment { get; set; }

    public bool? ShowReminder { get; set; }

    public bool ShowDescription
    {
      get
      {
        return !string.IsNullOrEmpty(this.SourceViewModel.Content) && this.Kind != "CHECKLIST" && !SettingsHelper.GetShowDetail() && !this.IsCourse;
      }
    }

    public bool ShowComment
    {
      get => this.SourceViewModel.CommentCount != null && this.SourceViewModel.CommentCount != "0";
    }

    public bool ShowPomo { get; set; }

    public bool ShowRepeat
    {
      get
      {
        return !string.IsNullOrEmpty(this.RepeatFlag) && !this.RepeatFlag.Contains("NONE") && this.StartDate.HasValue;
      }
    }

    public bool ShowContent
    {
      get => !string.IsNullOrEmpty(this.Content?.Trim()) && SettingsHelper.GetShowDetail();
    }

    public void NotifyShowIconsChanged() => this.OnPropertyChanged("ShowIcons");

    public bool IsLoadMore => this.Type == DisplayType.LoadMore;

    public bool IsTaskOrNote => this.IsTask || this.IsNote;

    public bool IsAgenda => this.Type == DisplayType.Agenda;

    public bool IsTask => this.Type == DisplayType.Task || this.Type == DisplayType.Agenda;

    public bool IsSection => this.Type == DisplayType.Section;

    public bool IsSplit => this.Type == DisplayType.Split;

    public bool IsItem => this.Type == DisplayType.CheckItem;

    public bool IsEvent => this.Type == DisplayType.Event;

    public bool IsHabit => this.Type == DisplayType.Habit;

    public bool IsCourse => this.Type == DisplayType.Course;

    public bool IsNote => this.Type == DisplayType.Note;

    public bool ReadOnly
    {
      get
      {
        return !this.Enable || this.IsCourse || this.IsHabit || this._inBatchSelected || this.InMatrix;
      }
    }

    public bool IsNormalItem => !this.IsSection && !this.IsLoadMore && !this.IsSplit;

    public DisplayType Type => this.SourceViewModel.Type;

    public bool Selected
    {
      get => this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool ShowProject
    {
      get => this._showProject;
      set
      {
        this._showProject = value;
        this.OnPropertyChanged(nameof (ShowProject));
      }
    }

    public int Level
    {
      get => this._level;
      set
      {
        this._level = value;
        this.OnPropertyChanged(nameof (Level));
      }
    }

    public bool InOperation
    {
      get => this._inOperation;
      set
      {
        this._inOperation = value;
        this.OnPropertyChanged(nameof (InOperation));
      }
    }

    public bool UnderTaskItem
    {
      get => this._underTaskItem;
      set
      {
        this._underTaskItem = value;
        this.OnPropertyChanged(nameof (UnderTaskItem));
      }
    }

    public bool InBatchSelected
    {
      get => this._inBatchSelected;
      set
      {
        if (this._inBatchSelected == value && !this.IsSection)
          return;
        this._inBatchSelected = value;
        this.OnPropertyChanged(nameof (InBatchSelected));
        this.OnPropertyChanged("ReadOnly");
        this.OnPropertyChanged("SectionRightActionText");
      }
    }

    public string SectionRightActionText
    {
      get
      {
        if (!this.IsSection)
          return (string) null;
        if (this.Section.Children == null || this.Section.Children.Count == 0)
          return (string) null;
        if (this.InKanban || this.Section.Children.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.InMatrix)))
          return (string) null;
        if (this.IsCustomizedSection)
        {
          if (this._inBatchSelected)
          {
            List<DisplayItemModel> childrenModels = this.GetChildrenModels(true);
            return Utils.GetString((childrenModels != null ? (childrenModels.All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => !m.CanBatchSelect || m.InBatchSelected)) ? 1 : 0) : 0) != 0 ? "DeselectAll" : "SelectAll");
          }
        }
        else
        {
          int num;
          if (this.InBatchSelected && !(this.Section is HabitSection) && !(this.Section is TimetableSection) && !(this.Section.SectionId == "8ac3038d93c54b80a67321b6a03df066"))
          {
            List<DisplayItemModel> children = this.Section.Children;
            num = children != null ? (children.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.CanBatchSelect)) ? 1 : 0) : 0;
          }
          else
            num = 0;
          if (num != 0)
          {
            List<DisplayItemModel> childrenModels = this.GetChildrenModels(true);
            return Utils.GetString((childrenModels != null ? (childrenModels.All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => !m.CanBatchSelect || m.InBatchSelected)) ? 1 : 0) : 0) != 0 ? "DeselectAll" : "SelectAll");
          }
          if (this.Section is OutdatedSection section && section.ShowPostpone)
            return Utils.GetString("Postpone");
        }
        return (string) null;
      }
    }

    public string ParentId => !this.IsItem ? this.SourceViewModel.ParentId : string.Empty;

    public Geometry Icon { get; set; }

    public double IconWidth { get; set; } = 14.0;

    public string GetTaskId() => this.TaskId;

    public string GetAttendId() => this.AttendId;

    public static DisplayItemModel Copy(DisplayItemModel origin)
    {
      DisplayItemModel displayItemModel = origin.Clone();
      displayItemModel.Dragging = false;
      return displayItemModel;
    }

    public DisplayItemModel()
    {
    }

    public DisplayItemModel(TaskBaseViewModel vm, bool showProject = false, bool isKanbanMode = false)
    {
      this.SourceViewModel = vm;
      this._avatarUrl = !string.IsNullOrEmpty(vm.Assignee) ? AvatarHelper.GetCacheUrl(vm.Assignee, vm.ProjectId) : string.Empty;
      if (vm.IsEvent)
      {
        this.SourceViewModel.ProjectName = SubscribeCalendarHelper.GetCalendarName(vm.CalendarId);
        this.CalendarType = SubscribeCalendarHelper.GetCalendarType(vm.CalendarId);
      }
      if (vm.IsTaskOrNote)
      {
        ProjectModel projectById = CacheManager.GetProjectById(vm.ProjectId);
        this.TeamId = projectById?.teamId;
        this.Permission = projectById?.permission;
      }
      if (vm is HabitBaseViewModel habitBaseViewModel)
        this.ShowReminder = new bool?((habitBaseViewModel.HabitCheckIn == null || habitBaseViewModel.HabitCheckIn.CheckStatus == 0 && habitBaseViewModel.HabitCheckIn.Value < habitBaseViewModel.HabitCheckIn.Goal) && HabitUtils.IsReminderValid(habitBaseViewModel.Habit.Reminder));
      this._isOpen = vm.IsOpen;
      this._showProject = showProject && (!vm.IsTaskOrNote || CacheManager.GetProjectById(vm.ProjectId) != null);
      this.InKanban = isKanbanMode;
    }

    private void AddVmPropertyChangedEvent()
    {
      if (this.SourceViewModel == null)
        return;
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this.SourceViewModel, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Tag"))
            break;
          this.OnPropertyChanged("Tags");
          if (this.InKanban)
            this.SetDisplayTags(LocalSettings.Settings.ExtraSettings.KbSize == 0 ? 1 : 2);
          this.OnPropertyChanged("ShowTag");
          this.OnPropertyChanged("ShowTagProjectPanel");
          break;
        case 4:
          switch (propertyName[0])
          {
            case 'D':
              if (!(propertyName == "Desc") || !SettingsHelper.GetShowDetail())
                return;
              DisplayItemModel.AssembleModelContent(this);
              return;
            case 'K':
              if (!(propertyName == "Kind"))
                return;
              this.SetIcon();
              return;
            case 'T':
              if (!(propertyName == "Type"))
                return;
              this.OnPropertyChanged("Type");
              this.OnPropertyChanged("ShowNoteDate");
              return;
            default:
              return;
          }
        case 5:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "Color"))
                return;
              this.OnPropertyChanged("Color");
              this.OnPropertyChanged("ShowProjectColor");
              return;
            case 'T':
              if (!(propertyName == "Title"))
                return;
              this.OnPropertyChanged("Title");
              return;
            default:
              return;
          }
        case 6:
          if (!(propertyName == "Status"))
            break;
          this.OnPropertyChanged("Status");
          this.SetIcon();
          this.OnPropertyChanged("IsAbandoned");
          this.OnPropertyChanged("IsCompleted");
          this.OnPropertyChanged("ShowIcons");
          break;
        case 7:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "Content"))
                return;
              if (SettingsHelper.GetShowDetail())
              {
                DisplayItemModel.AssembleModelContent(this);
                return;
              }
              this.OnPropertyChanged("ShowIcons");
              return;
            case 'D':
              if (!(propertyName == "DueDate"))
                return;
              this.OnPropertyChanged("DueDate");
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
              this.OnPropertyChanged("ShowIcons");
              return;
            case 'i':
              if (!(propertyName == "Assignee"))
                return;
              this.AvatarUrl = !string.IsNullOrEmpty(this.Assignee) ? AvatarHelper.GetCacheUrl(this.Assignee, this.ProjectId) : string.Empty;
              return;
            case 'l':
              if (!(propertyName == "IsAllDay"))
                return;
              this.OnPropertyChanged("IsAllDay");
              return;
            case 'o':
              if (!(propertyName == "Priority"))
                return;
              this.OnPropertyChanged("Priority");
              return;
            case 't':
              if (!(propertyName == "Editable"))
                return;
              this.OnPropertyChanged("Enable");
              return;
            default:
              return;
          }
        case 9:
          if (!(propertyName == "StartDate"))
            break;
          this.OnPropertyChanged("StartDate");
          this.OnPropertyChanged("ShowIcons");
          break;
        case 10:
          if (!(propertyName == "RepeatFlag"))
            break;
          this.OnPropertyChanged("ShowRepeat");
          this.OnPropertyChanged("ShowIcons");
          break;
        case 11:
          if (!(propertyName == "ProjectName"))
            break;
          this.OnPropertyChanged("ProjectName");
          break;
        case 12:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "CommentCount"))
                return;
              this.OnPropertyChanged("ShowIcons");
              return;
            case 'M':
              if (!(propertyName == "ModifiedTime"))
                return;
              this.OnPropertyChanged("NoteDisplayDate");
              return;
            default:
              return;
          }
        case 16:
          if (!(propertyName == "CheckItemContent") || !SettingsHelper.GetShowDetail())
            break;
          DisplayItemModel.AssembleModelContent(this);
          break;
      }
    }

    public static List<TagViewModel> BuildTagModels(
      IReadOnlyList<string> tags,
      bool isKanbanMode = false,
      bool inMatrix = false,
      int maxCount = 2)
    {
      if (tags == null || tags.Count <= 0)
        return new List<TagViewModel>();
      maxCount = !SettingsHelper.GetShowDetail() || isKanbanMode || inMatrix ? maxCount : 20;
      List<TagViewModel> tagViewModelList = new List<TagViewModel>();
      List<TagModel> cachedTags = TagDataHelper.GetTags();
      List<TagViewModel> list = tags.Select(tag => new
      {
        tag = tag,
        local = cachedTags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag))
      }).Select(_param1 => _param1.local == null ? new TagViewModel(_param1.tag, _param1.tag) : new TagViewModel(_param1.local)).OrderBy<TagViewModel, long>((Func<TagViewModel, long>) (tag => tag.SortOrder)).ToList<TagViewModel>();
      if (list.Count <= maxCount)
        return list;
      string extra = string.Empty;
      for (int index = maxCount; index < list.Count; ++index)
      {
        TagViewModel tagViewModel = list[index];
        extra = extra + tagViewModel.Tag + (index == list.Count - 1 ? string.Empty : ", ");
      }
      tagViewModelList.AddRange(list.Take<TagViewModel>(maxCount));
      tagViewModelList.Add(new TagViewModel("+" + (list.Count - maxCount).ToString(), extra));
      return tagViewModelList;
    }

    public static DisplayItemModel BuildSection(Section section, int num = 0)
    {
      bool flag = true;
      if (section is CustomizedSection)
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == section.ProjectId));
        flag = projectModel != null && projectModel.IsEnable();
      }
      if (flag && section is OutdatedSection)
      {
        flag = false;
        foreach (DisplayItemModel child in section.Children)
        {
          if (child.Enable && !child.IsAgenda)
          {
            flag = true;
            break;
          }
          if (child.IsAgenda && child.AttendId == child.Id)
          {
            flag = true;
            break;
          }
        }
      }
      return new DisplayItemModel(new TaskBaseViewModel(section)
      {
        Editable = flag,
        ProjectId = section.ProjectId
      })
      {
        Section = section,
        _num = num
      };
    }

    public static DisplayItemModel BuildLoadMore()
    {
      return new DisplayItemModel(new TaskBaseViewModel()
      {
        Type = DisplayType.LoadMore
      });
    }

    private string GetEntityType()
    {
      switch (this.Type)
      {
        case DisplayType.Task:
          return "task";
        case DisplayType.CheckItem:
          return "item";
        case DisplayType.Agenda:
          return "agenda";
        case DisplayType.Derivative:
          return "derivative";
        case DisplayType.Event:
          return "event";
        default:
          return string.Empty;
      }
    }

    public static DisplayItemModel AssembleModelContent(DisplayItemModel model)
    {
      if (model?.SourceViewModel != null)
      {
        TaskBaseViewModel sourceViewModel = model.SourceViewModel;
        string str1 = string.Empty;
        if (LocalSettings.Settings.InSearch && !string.IsNullOrEmpty(SearchHelper.SearchKey))
        {
          if (sourceViewModel.Kind == "CHECKLIST")
          {
            Match match1 = SearchHelper.SearchRegex.Match(sourceViewModel.Desc?.ToLower() ?? string.Empty);
            if (sourceViewModel.Desc != null && match1.Success)
            {
              str1 = match1.Index > 8 ? "..." + sourceViewModel.Desc.Substring(match1.Index - 8).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ") : sourceViewModel.Desc;
            }
            else
            {
              List<TaskBaseViewModel> checkItemsByTaskId = TaskDetailItemCache.GetCheckItemsByTaskId(model.TaskId);
              if (checkItemsByTaskId != null)
              {
                checkItemsByTaskId.Sort((Comparison<TaskBaseViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
                foreach (TaskBaseViewModel taskBaseViewModel in checkItemsByTaskId)
                {
                  Match match2 = SearchHelper.SearchRegex.Match(taskBaseViewModel.Title?.ToLower() ?? string.Empty);
                  if (taskBaseViewModel.Title != null && match2.Success)
                  {
                    str1 = "- " + (match2.Index > 8 ? "..." + taskBaseViewModel.Title.Substring(match2.Index - 8) : taskBaseViewModel.Title);
                    break;
                  }
                }
                if (string.IsNullOrEmpty(str1))
                {
                  if (!string.IsNullOrEmpty(sourceViewModel.Desc))
                  {
                    str1 = sourceViewModel.Desc;
                  }
                  else
                  {
                    TaskBaseViewModel taskBaseViewModel = checkItemsByTaskId.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (i => i.Status == 0)) ?? checkItemsByTaskId.FirstOrDefault<TaskBaseViewModel>();
                    str1 = string.IsNullOrEmpty(taskBaseViewModel?.Title) ? string.Empty : "- " + taskBaseViewModel.Title;
                  }
                }
              }
            }
          }
          else if (!string.IsNullOrEmpty(sourceViewModel.Content))
          {
            string str2 = TaskUtils.ReplaceAttachmentNameInString(sourceViewModel.Content);
            Match match = SearchHelper.SearchRegex.Match(str2.ToLower());
            str1 = !match.Success || match.Index <= 11 ? str2 : "..." + str2.Substring(match.Index - 10).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
          }
          else
            str1 = string.Empty;
        }
        else
          str1 = !(sourceViewModel.Kind == "CHECKLIST") ? sourceViewModel.Content : (string.IsNullOrEmpty(sourceViewModel.Desc) ? "- " + TaskDetailItemCache.GetPrimarySubtaskInTask(model.TaskId)?.Title : sourceViewModel.Desc);
        if (model.Content != str1)
          model.Content = str1;
      }
      return model;
    }

    public static async void AssembleModelContent(ObservableCollection<DisplayItemModel> models)
    {
      if (models == null || !models.Any<DisplayItemModel>())
        return;
      List<string> taskIds = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.IsTask && !string.IsNullOrEmpty(model.TaskId) && !string.IsNullOrEmpty(model.Kind) && model.Kind == "CHECKLIST")).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (model => model.TaskId)).ToList<string>();
      if (taskIds.Any<string>())
      {
        Dictionary<string, string> descDict = new Dictionary<string, string>();
        ObservableCollection<TaskModel> tasksInTaskIds = await TaskDao.GetTasksInTaskIds(taskIds);
        if (tasksInTaskIds != null && tasksInTaskIds.Any<TaskModel>())
        {
          foreach (TaskModel taskModel in (Collection<TaskModel>) tasksInTaskIds)
          {
            if (!string.IsNullOrEmpty(taskModel.desc) && !descDict.ContainsKey(taskModel.id))
              descDict.Add(taskModel.id, taskModel.desc);
          }
        }
        Dictionary<string, TaskDetailItemModel> subtaskInTaskIds = await TaskDetailItemDao.GetPrimarySubtaskInTaskIds(taskIds.Where<string>((Func<string, bool>) (id => id != null && !descDict.Keys.Contains<string>(id))).ToList<string>());
        foreach (DisplayItemModel model in (Collection<DisplayItemModel>) models)
        {
          if (model.IsTask && model.Kind == "CHECKLIST" && !string.IsNullOrEmpty(model.TaskId))
          {
            if (descDict.ContainsKey(model.TaskId))
              model.Content = descDict[model.TaskId];
            else if (subtaskInTaskIds.ContainsKey(model.TaskId))
            {
              TaskDetailItemModel taskDetailItemModel = subtaskInTaskIds[model.TaskId];
              model.Content = "- " + taskDetailItemModel.title;
            }
          }
        }
      }
      taskIds = (List<string>) null;
    }

    public TaskModel GetTaskModel()
    {
      TaskModel taskModel = new TaskModel();
      taskModel.id = this.Id;
      taskModel.content = this.Content;
      taskModel.sortOrder = this.SortOrder;
      taskModel.projectId = this.ProjectId;
      taskModel.priority = this.Priority;
      taskModel.title = this.Title;
      taskModel.status = this.Status;
      taskModel.isAllDay = this.IsAllDay;
      taskModel.startDate = this.StartDate;
      taskModel.dueDate = this.DueDate;
      taskModel.kind = this.Kind;
      taskModel.deleted = this.Deleted;
      taskModel.repeatFlag = this.RepeatFlag;
      taskModel.repeatFrom = this.RepeatFrom;
      taskModel.assignee = this.Assignee;
      taskModel.commentCount = this.SourceViewModel.CommentCount;
      taskModel.completedTime = this.CompletedTime;
      string[] tags = this.Tags;
      taskModel.tag = TagSerializer.ToJsonContent(tags != null ? ((IEnumerable<string>) tags).ToList<string>() : (List<string>) null);
      taskModel.parentId = this.ParentId;
      taskModel.columnId = this.ColumnId;
      taskModel.pinnedTimeStamp = this.PinnedTime;
      return taskModel;
    }

    public DisplayItemModel Clone() => (DisplayItemModel) this.MemberwiseClone();

    public List<DisplayItemModel> Children { get; set; }

    public List<DisplayItemModel> BatchModels { get; set; }

    public List<string> ChildIds { get; set; }

    public bool HasChildren => this.Children != null && this.Children.Count > 0;

    public bool IsParentTask => TaskCache.IsParentTask(this.Id);

    public bool InDetail { get; set; }

    public int MoreCount { get; set; }

    public bool NewAdd { get; set; }

    public List<DisplayItemModel> GetChildrenModels(bool getAll, HashSet<string> openedTaskIds = null)
    {
      List<DisplayItemModel> ms = new List<DisplayItemModel>();
      if (this.IsSection)
      {
        Section section = this.Section;
        List<DisplayItemModel> displayItemModelList1;
        if (section == null)
        {
          displayItemModelList1 = (List<DisplayItemModel>) null;
        }
        else
        {
          List<DisplayItemModel> children = section.Children;
          displayItemModelList1 = children != null ? children.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (c => c._level == 0)).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
        }
        List<DisplayItemModel> displayItemModelList2 = displayItemModelList1;
        if (displayItemModelList2 != null)
        {
          foreach (DisplayItemModel model in displayItemModelList2)
          {
            ms.Add(model);
            GetChildren(model, ms, true);
          }
        }
      }
      else
        GetChildren(this, ms, true);
      return ms;

      void GetChildren(DisplayItemModel model, List<DisplayItemModel> ms, bool getAllChild)
      {
        if (!getAllChild && !model._isOpen || model.Children == null)
          return;
        foreach (DisplayItemModel child in model.Children)
        {
          if (openedTaskIds != null)
            child._isOpen = openedTaskIds.Contains(child.Id);
          ms.Add(child);
          GetChildren(child, ms, getAll);
        }
      }
    }

    private async void OnChildrenChanged()
    {
      DisplayItemModel displayItemModel = this;
      displayItemModel.Level = displayItemModel.Level;
      if (LocalSettings.Settings.ShowDetails)
        displayItemModel.CompletionRate = TaskCompletionRateDao.GetRateStrByIdInDb(displayItemModel.TaskId, displayItemModel.ProjectId);
      displayItemModel.OnPropertyChanged("HasChildren");
      displayItemModel.OnPropertyChanged("ShowIcons");
    }

    public void AddChild(DisplayItemModel displayModel, DisplayItemModel front)
    {
      List<DisplayItemModel> children = this.Children;
      if (this.IsSection)
        children = this.Section.Children;
      else if (this.ChildIds != null)
        this.ChildIds.Add(displayModel.Id);
      else
        this.ChildIds = new List<string>()
        {
          displayModel.Id
        };
      if (children != null)
      {
        int num = front == null ? 0 : children.IndexOf(front);
        children.Insert(Math.Min(num + 1, children.Count), displayModel);
      }
      else
        this.Children = new List<DisplayItemModel>()
        {
          displayModel
        };
      this.OnChildrenChanged();
    }

    public void RemoveItem(DisplayItemModel item)
    {
      List<DisplayItemModel> children = this.Children;
      if (this.IsSection)
        children = this.Section.Children;
      children?.Remove(item);
      this.ChildIds?.Remove(item.Id);
      this.OnChildrenChanged();
      // ISSUE: explicit non-virtual call
      this.Num = children != null ? __nonvirtual (children.Count) : 0;
    }

    public PomodoroSummaryModel PomoSummary { get; set; }

    public SectionAddTaskViewModel AddViewModel { get; set; }

    public bool IsNewAdd { get; set; }

    public TimeData ParseData { get; set; }

    public bool IsToggling { get; set; }

    public int? CaretIndex { get; set; }

    public bool InSticky { get; set; }

    public bool Linked { get; set; }

    public bool CanBatchSelect
    {
      get => this.IsTaskOrNote && this.Permission != "comment" && this.Permission != "read";
    }

    public CourseDisplayModel Course => this.SourceViewModel.Course;

    public bool InTomorrow { get; set; }

    public bool SearchComment { get; set; }

    public bool IsOutDate()
    {
      DateTime? nullable;
      if (this.DueDate.HasValue)
      {
        nullable = this.DueDate;
        DateTime today = DateTime.Today;
        if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= today ? 1 : 0) : 0) != 0)
          return true;
      }
      nullable = this.StartDate;
      if (nullable.HasValue)
      {
        nullable = this.DueDate;
        if (!nullable.HasValue)
        {
          nullable = this.StartDate;
          DateTime today = DateTime.Today;
          return nullable.HasValue && nullable.GetValueOrDefault() < today;
        }
      }
      return false;
    }

    public void RemoveParent()
    {
    }

    public void NotifyChanged()
    {
      this.OnPropertyChanged("ShowTag");
      this.OnPropertyChanged("ShowIndicator");
      this.OnPropertyChanged("IsHide");
    }

    public void SetIcon()
    {
      this.Icon = ThemeUtil.GetTaskIconGeometry(this.Type, this.Status, this.Kind, this.CalendarType);
      this.IconWidth = this.Type == DisplayType.Course ? 15.0 : 14.0;
      this.OnPropertyChanged("Icon");
      this.OnPropertyChanged("IconWidth");
    }

    public bool OutDate()
    {
      if (!this.StartDate.HasValue)
        return false;
      return !this.DueDate.HasValue ? this.StartDate.Value < DateTime.Today : this.DueDate.Value <= DateTime.Today;
    }

    public void SetHabit(HabitModel habit)
    {
      if (!(this.SourceViewModel is HabitBaseViewModel sourceViewModel))
        return;
      sourceViewModel.Habit = habit;
    }

    public void SetHabitCheckIn(HabitCheckInModel checkIn)
    {
      if (!(this.SourceViewModel is HabitBaseViewModel sourceViewModel))
        return;
      sourceViewModel.HabitCheckIn = checkIn;
    }

    public string GetAssignee() => !string.IsNullOrEmpty(this.Assignee) ? this.Assignee : "-1";

    public void OnTimeParsed(TimeData parseData)
    {
      if (parseData == null && this.ParseData == null)
        return;
      this.ParseData = parseData;
      this.OnPropertyChanged("StartDate");
      this.OnPropertyChanged("DueDate");
      this.OnPropertyChanged("IsAllDay");
    }

    public void SetPropertyChangedEvent()
    {
      if (this._eventAdded)
        return;
      this._eventAdded = true;
      this.AddVmPropertyChangedEvent();
    }

    public void NotifyPropertyChanged(string name) => this.OnPropertyChanged(name);

    public async Task LoadPomo()
    {
      DisplayItemModel displayItemModel1 = this;
      PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(displayItemModel1.Id);
      displayItemModel1.PomoSummary = pomoByTaskId;
      DisplayItemModel displayItemModel2 = displayItemModel1;
      PomodoroSummaryModel pomoSummary = displayItemModel1.PomoSummary;
      int num = (pomoSummary != null ? (pomoSummary.IsEmpty() ? 1 : 0) : 1) == 0 ? 1 : 0;
      displayItemModel2.ShowPomo = num != 0;
      displayItemModel1.OnPropertyChanged("ShowPomo");
      displayItemModel1.NotifyShowIconsChanged();
    }

    public void SetShowAttachment()
    {
      this.ShowAttachment = new bool?(this.IsTaskOrNote && AttachmentCache.GetTaskExistAttachment(this.SourceViewModel.Id));
      this.NotifyShowIconsChanged();
    }

    public bool GetShowAttachment()
    {
      if (!this.ShowAttachment.HasValue)
        this.ShowAttachment = new bool?(this.IsTaskOrNote && AttachmentCache.GetTaskExistAttachment(this.SourceViewModel.Id));
      return this.ShowAttachment.Value;
    }

    public void SetShowReminder(bool val)
    {
      this.ShowReminder = new bool?(val);
      this.NotifyShowIconsChanged();
    }

    public bool CanHoverSwitch(int hoverIndex)
    {
      return (hoverIndex != 0 || !this.IsSection || this.Section is CompletedSection) && (this.InDetail || this.Status == 0) && !this.IsLoadMore && !(this.Section is HabitSection);
    }

    public void SortChildren(IComparer<DisplayItemModel> comparer)
    {
      List<DisplayItemModel> children = this.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      if (comparer == null)
        this.Children.Sort((Comparison<DisplayItemModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      else
        this.Children.Sort(comparer);
      foreach (DisplayItemModel child in this.Children)
        child.SortChildren(comparer);
    }

    public List<TitleChunk> GetTitleNum() => this.SourceViewModel.GetTitleNum();

    public static DisplayItemModel BuildSplit()
    {
      return new DisplayItemModel()
      {
        SourceViewModel = new TaskBaseViewModel()
        {
          Type = DisplayType.Split
        }
      };
    }
  }
}
