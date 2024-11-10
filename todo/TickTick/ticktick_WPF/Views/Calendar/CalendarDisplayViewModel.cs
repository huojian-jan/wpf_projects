// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDisplayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDisplayViewModel : INotifyPropertyChanged, AgendaHelper.IAgenda
  {
    public readonly List<CalendarDisplayViewModel> Children = new List<CalendarDisplayViewModel>();
    public double RepeatDiff;
    private bool _dragging;
    private bool _isOpened = true;
    private bool _selected;
    private SolidColorBrush _titleColor;
    private SolidColorBrush _backColor;
    private SolidColorBrush _backBorderColor;
    private Thickness _backBorderThickness;
    private int _level;
    public int Column;
    public int ColumnSpan;
    public int Row;
    public CourseDisplayModel Course;
    private bool _inDark;
    private bool _inArrange;
    private static BlockingSet<string> _errorColors = new BlockingSet<string>();
    private TaskCell _taskCell;

    public CalendarDisplayViewModel Parent { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName = null)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged != null)
          propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
        this._taskCell?.OnTaskPropertyChanged(propertyName);
      }));
    }

    public TaskBaseViewModel SourceViewModel { get; set; }

    public string AttendId => this.SourceViewModel.AttendId;

    public string EventId => !this.IsCalendarEvent ? string.Empty : this.SourceViewModel.Id;

    public string HabitId => !this.IsHabit ? string.Empty : this.SourceViewModel.Id;

    public string PomoId => !this.IsPomo ? string.Empty : this.SourceViewModel.Id;

    public string ItemId => !this.IsCheckItem ? string.Empty : this.SourceViewModel.Id;

    public string ProjectId => this.SourceViewModel.ProjectId;

    public string RepeatFlag => this.SourceViewModel.RepeatFlag;

    public string RepeatFrom => this.SourceViewModel.RepeatFrom;

    public string TaskId => !this.IsTaskOrNote ? string.Empty : this.SourceViewModel.Id;

    public string TimeText { get; set; }

    protected bool ShowSpanTimeText { get; set; }

    public bool IsTaskOrNote
    {
      get
      {
        return this.Type == DisplayType.Task || this.Type == DisplayType.Note || this.Type == DisplayType.Derivative;
      }
    }

    public bool IsDerivative => this.Type == DisplayType.Derivative;

    public bool IsCalendarEvent => this.Type == DisplayType.Event;

    public bool IsSection => this.Type == DisplayType.Section;

    public bool IsHabit => this.Type == DisplayType.Habit;

    public bool IsPomo => this.Type == DisplayType.Pomo;

    public bool IsCheckItem => this.Type == DisplayType.CheckItem;

    public bool ShowInFocus { get; set; }

    public bool IsLoadMore { get; set; }

    public bool ShowIcon
    {
      get
      {
        if (this.Type == DisplayType.Habit || this.Type == DisplayType.Note)
          return true;
        return this.IsAbandoned && this.Type == DisplayType.Task;
      }
    }

    public bool CanDrag => this.SourceViewModel.Editable;

    public string CalendarId => this.SourceViewModel.CalendarId;

    public bool DragMode { get; set; }

    public Geometry Icon { get; set; }

    public bool IsCompleted => this.Status == 2;

    public bool IsAbandoned => this.Status == -1;

    public DisplayType Type
    {
      get => this.RepeatDiff <= 0.0 ? this.SourceViewModel.Type : DisplayType.Derivative;
    }

    public bool IsOpened
    {
      get => this._isOpened;
      set
      {
        this._isOpened = value;
        this.OnPropertyChanged(nameof (IsOpened));
      }
    }

    public int Priority => this.SourceViewModel.Priority;

    public string Title => this.SourceViewModel.TitleWithoutLink;

    public string Color { get; set; }

    public int Status => this.SourceViewModel.Status;

    public int Level
    {
      get => this._level;
      set
      {
        this._level = value;
        this.OnPropertyChanged(nameof (Level));
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

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.SetColor();
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public string Tips { get; set; }

    public bool? IsAllDay => this.SourceViewModel.IsAllDay;

    public DateTime? StartDate => this.SourceViewModel.StartDate;

    public DateTime? DueDate => this.SourceViewModel.DueDate;

    public DateTime? DisplayStartDate
    {
      get
      {
        DateTime? startDate = this.SourceViewModel.StartDate;
        ref DateTime? local = ref startDate;
        return !local.HasValue ? new DateTime?() : new DateTime?(local.GetValueOrDefault().AddDays(this.RepeatDiff));
      }
    }

    public DateTime? DisplayDueDate
    {
      get
      {
        DateTime? dueDate = this.SourceViewModel.DueDate;
        ref DateTime? local = ref dueDate;
        return !local.HasValue ? new DateTime?() : new DateTime?(local.GetValueOrDefault().AddDays(this.RepeatDiff));
      }
    }

    public SolidColorBrush TitleColor
    {
      get => this._titleColor;
      set
      {
        this._titleColor = value;
        this.OnPropertyChanged(nameof (TitleColor));
      }
    }

    public SolidColorBrush BackColor
    {
      get => this._backColor;
      set
      {
        this._backColor = value;
        this.OnPropertyChanged(nameof (BackColor));
      }
    }

    public SolidColorBrush BackBorderColor
    {
      get => this._backBorderColor;
      set
      {
        this._backBorderColor = value;
        this.OnPropertyChanged(nameof (BackBorderColor));
      }
    }

    public Thickness BackBorderThickness
    {
      get => this._backBorderThickness;
      set
      {
        this._backBorderThickness = value;
        this.OnPropertyChanged(nameof (BackBorderThickness));
      }
    }

    public double LoadMoreWidth { get; set; }

    public string GetTaskId() => this.SourceViewModel.GetTaskId();

    public string GetAttendId() => this.AttendId;

    public void SetIcon(bool notify = false)
    {
      switch (this.Type)
      {
        case DisplayType.Task:
          if (this.IsAbandoned)
          {
            this.Icon = Utils.GetIcon("IcCalAbandonedIndicator");
            break;
          }
          break;
        case DisplayType.Habit:
          this.Icon = !this.IsAbandoned ? (!this.IsCompleted ? Utils.GetIcon("IcLineHabit") : Utils.GetIcon("IcHabitCompletedIndicator")) : Utils.GetIcon("IcHabitUncompletedIndicator");
          break;
        case DisplayType.Note:
          this.Icon = Utils.GetIcon("IcNoteIndicator");
          break;
      }
      if (!notify)
        return;
      this.OnPropertyChanged("Icon");
    }

    public static CalendarDisplayViewModel Build(CalendarDisplayModel model)
    {
      if (model == null)
        return new CalendarDisplayViewModel();
      string taskColor = model.SourceViewModel.Color;
      if (!model.IsVirtual)
        taskColor = model.CourseDisplayModel != null ? model.SourceViewModel.Color : CalendarDisplayViewModel.TryGetColor(taskColor, model.SourceViewModel.Tag, model.SourceViewModel.Priority);
      CalendarDisplayViewModel displayViewModel = new CalendarDisplayViewModel();
      displayViewModel.Color = taskColor;
      displayViewModel.Course = model.CourseDisplayModel;
      displayViewModel.RepeatDiff = model.repeatDiff;
      displayViewModel.TimeText = model.CourseDisplayModel == null ? (string) null : ticktick_WPF.Util.DateUtils.FormatHourMinuteText(model.CourseDisplayModel.CourseStart);
      displayViewModel.SourceViewModel = model.SourceViewModel;
      displayViewModel.SetIcon();
      return displayViewModel;
    }

    public async void SetSourceModel(TaskBaseViewModel vm, bool setEvent = false)
    {
      CalendarDisplayViewModel displayViewModel = this;
      TaskBaseViewModel old = displayViewModel.SourceViewModel;
      displayViewModel.SourceViewModel = vm;
      if (setEvent)
        await Task.Delay(new Random().Next(50, 200));
      if (old != null)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) old, new EventHandler<PropertyChangedEventArgs>(displayViewModel.OnTaskPropertyChanged), string.Empty);
      if (!setEvent)
      {
        old = (TaskBaseViewModel) null;
      }
      else
      {
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) vm, new EventHandler<PropertyChangedEventArgs>(displayViewModel.OnTaskPropertyChanged), string.Empty);
        old = (TaskBaseViewModel) null;
      }
    }

    private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.Course != null)
        return;
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Tag"))
            return;
          goto label_27;
        case 4:
          return;
        case 5:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "Color"))
                return;
              goto label_27;
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
            return;
          goto label_27;
        case 7:
          if (!(propertyName == "DueDate"))
            return;
          break;
        case 8:
          switch (propertyName[0])
          {
            case 'I':
              if (!(propertyName == "IsAllDay"))
                return;
              break;
            case 'P':
              if (!(propertyName == "Priority"))
                return;
              goto label_27;
            default:
              return;
          }
          break;
        case 9:
          if (!(propertyName == "StartDate"))
            return;
          break;
        default:
          return;
      }
      this.SetTimeText(this is TaskCellViewModel);
      return;
label_27:
      string color1;
      if (!string.IsNullOrEmpty(this.Course?.Color))
      {
        color1 = this.Course.Color;
      }
      else
      {
        string color2 = this.SourceViewModel?.Color;
        string tag = this.SourceViewModel?.Tag;
        TaskBaseViewModel sourceViewModel = this.SourceViewModel;
        int priority = sourceViewModel != null ? sourceViewModel.Priority : 0;
        color1 = CalendarDisplayViewModel.TryGetColor(color2, tag, priority);
      }
      this.Color = color1;
      this.SetColor();
    }

    public static CalendarDisplayViewModel Build(TaskBaseViewModel task)
    {
      return CalendarDisplayViewModel.Build(new CalendarDisplayModel(task));
    }

    protected static string TryGetColor(string taskColor, string taskTag, int taskPriority)
    {
      string color = taskColor;
      switch (LocalSettings.Settings.CellColorType.ToLower())
      {
        case "tag":
          color = CacheManager.GetTags(TagSerializer.ToTags(taskTag)?.ToArray()).OrderBy<TagModel, long>((Func<TagModel, long>) (t => t.sortOrder)).FirstOrDefault<TagModel>()?.color;
          break;
        case "priority":
          color = Utils.GetPriorityColor(taskPriority).Color.ToString();
          if (color.Length == 7)
          {
            color = color.Remove(1, 2);
            break;
          }
          break;
      }
      return color;
    }

    public CalendarDisplayViewModel Clone()
    {
      return new CalendarDisplayViewModel()
      {
        SourceViewModel = this.SourceViewModel.Copy(),
        Icon = this.Icon,
        Color = this.Color,
        _backColor = this._backColor,
        _titleColor = this._titleColor,
        _backBorderColor = this._backBorderColor,
        _backBorderThickness = this._backBorderThickness,
        RepeatDiff = this.RepeatDiff
      };
    }

    public void SetTheme(bool isDark)
    {
      this._inDark = isDark;
      this.SetColor();
    }

    private void SetColor()
    {
      if (this.ShowInFocus)
      {
        string color = this._inDark ? "#FFFFFF" : "#191919";
        this.TitleColor = ThemeUtil.GetColorInDict(color, 40);
        this.BackColor = ThemeUtil.GetColorInDict(color, this.Selected ? 10 : 5);
        this.BackBorderColor = ThemeUtil.GetColorInDict(this.IsPomo ? color : (string) null, 10);
        this.BackBorderThickness = this.IsPomo ? new Thickness(1.0) : new Thickness(0.0);
      }
      else
      {
        bool flag = this.Selected || this.DragMode;
        int percent = this.IsPomo ? (flag ? 60 : 40) : (this.Status == 0 & flag ? 100 : (this.Status == 0 | flag ? 80 : (this._inDark ? 20 : 40)));
        if (this._inDark && this.Color == ThemeUtil.GetColor("BaseColorOpacity40").Color.ToString())
          this.Color = "#66FFFFFF";
        string str1 = this.Color;
        if (string.IsNullOrEmpty(this.Color) || this.Color.ToLower() == "transparent")
          str1 = ThemeUtil.GetColorString("ColorPrimary", this._inDark);
        double num = 1.0;
        if (str1 != null && str1.Length > 7)
        {
          string str2 = str1.Substring(1, 2);
          try
          {
            num = (double) Convert.ToInt32(str2, 16) / 256.0;
          }
          catch (Exception ex)
          {
            if (!CalendarDisplayViewModel._errorColors.Contains(str1))
            {
              CalendarDisplayViewModel._errorColors.Add(str1);
              UtilLog.Info("GetColorAlphaError : " + str1);
            }
          }
        }
        int opacity = (int) ((this.IsPomo ? (flag ? 15.0 : 5.0) : (this.Status == 0 ? (flag ? 100.0 : 60.0) : (this._selected ? 40.0 : 20.0))) * num);
        double colorGrayscale = ColorUtils.GetColorGrayscale(str1, opacity, !this._inDark ? 1 : 0);
        if (!this._selected && !this._inDark)
        {
          if (colorGrayscale < 0.54)
            opacity = 30;
          if (this.Status != 0 && colorGrayscale < 0.85)
            opacity = 10;
        }
        if (this._selected && !this._inDark && colorGrayscale < 0.167)
          opacity = 80;
        this.TitleColor = ThemeUtil.GetColorInDict(!flag && this._inDark || flag && colorGrayscale <= 0.65 ? "#FFFFFF" : "#191919", percent);
        this.BackColor = ThemeUtil.GetColorInDict(str1, this.DragMode ? 100 : opacity);
        this.BackBorderColor = ThemeUtil.GetColorInDict(this.IsPomo ? str1 : "#FFFFFF", (int) ((flag ? 40.0 : 30.0) * (this.IsPomo ? num : 2.0)));
        this.BackBorderThickness = this.IsPomo ? new Thickness(1.0) : new Thickness(0.0);
      }
    }

    public void SetTimeText(bool inArrange, bool isSpan)
    {
      this._inArrange = inArrange;
      this.SetTimeText(isSpan);
    }

    private void SetTimeText(bool isSpan)
    {
      if (!string.IsNullOrEmpty(this.HabitId))
        return;
      DateTime? nullable1 = this.DisplayStartDate;
      if (!nullable1.HasValue)
      {
        this.TimeText = string.Empty;
        this.OnPropertyChanged("TimeText");
      }
      else
      {
        if (this._inArrange)
        {
          DateTime? nullable2 = this.DisplayDueDate;
          if (!nullable2.HasValue)
            nullable2 = this.DisplayStartDate;
          else if (((int) this.IsAllDay ?? 1) != 0)
            nullable2 = new DateTime?(nullable2.Value.AddSeconds(-1.0));
          this.TimeText = ticktick_WPF.Util.DateUtils.FormatDateCheckYear(nullable2.Value);
        }
        else if (((int) this.IsAllDay ?? 1) == 0)
        {
          nullable1 = this.DisplayStartDate;
          string str1 = ticktick_WPF.Util.DateUtils.FormatHourMinuteText(nullable1.Value);
          if (isSpan)
          {
            nullable1 = this.DisplayDueDate;
            if (nullable1.HasValue)
            {
              nullable1 = this.DisplayDueDate;
              DateTime dateTime1 = nullable1.Value;
              nullable1 = this.DisplayStartDate;
              DateTime dateTime2 = nullable1.Value;
              if (dateTime1 != dateTime2)
              {
                string str2 = str1;
                nullable1 = this.DisplayDueDate;
                string str3 = ticktick_WPF.Util.DateUtils.FormatHourMinuteText(nullable1.Value);
                str1 = str2 + " - " + str3;
              }
            }
          }
          this.TimeText = str1;
        }
        else
          this.TimeText = string.Empty;
        this.OnPropertyChanged("TimeText");
      }
    }

    public List<CalendarDisplayViewModel> GetChildren(bool checkOpen = true)
    {
      List<CalendarDisplayViewModel> result = new List<CalendarDisplayViewModel>();
      GetChild(this.Children, this.Level + 1, this.IsOpened);
      return result;

      void GetChild(List<CalendarDisplayViewModel> children, int level, bool isOpen)
      {
        foreach (CalendarDisplayViewModel child in children)
        {
          if (!checkOpen | isOpen)
            result.Add(child);
          child.Level = level;
          if (child.Children.Count > 0)
            GetChild(child.Children, level + 1, isOpen && child.IsOpened);
        }
      }
    }

    public void SetTaskCell(TaskCell taskCell) => this._taskCell = taskCell;

    ~CalendarDisplayViewModel() => this._taskCell = (TaskCell) null;
  }
}
