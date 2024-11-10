// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellViewModel
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
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellViewModel : TimelineDisplayBase
  {
    public bool PlayingAnimation;
    private double _maxWidth = double.NaN;
    private bool _isAllDay;
    private SolidColorBrush _backgroundBrush;
    private SolidColorBrush _borderBrush;
    private SolidColorBrush _thumbBrush;
    private SolidColorBrush _foregroundBrush;
    private double _titleOpacity;
    private Geometry _icon;
    private bool _inline = true;
    private Visibility _visibility;
    private bool _isParent;
    private bool _isOpen = true;
    private TimelineCellOperation _operation = TimelineCellOperation.None;
    private DateTime _startDate = DateTime.Today;
    private DateTime? _endDate;
    private double _left;
    private int _line;
    private string _avatarUrl;
    private static Geometry _noteIcon;
    private static Geometry _abandonIcon;
    private bool _forceShowBorder;
    private int _dragStatus;
    private double _width;
    private int _level;
    public List<TimelineCellViewModel> ChildItems = new List<TimelineCellViewModel>();

    public Task ClearDate() => TaskService.ClearDate(this.Id);

    public async Task CommitDate(TimeData data)
    {
      if (this.DisplayModel.Type == DisplayType.CheckItem)
        return;
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this.Id);
      if (thinTaskById == null)
        return;
      TimeData timeData1 = new TimeData();
      timeData1.StartDate = new DateTime?(this.StartDate);
      timeData1.DueDate = this.EndDate;
      timeData1.IsAllDay = new bool?(this.IsAllDay);
      timeData1.RepeatFlag = data == null ? thinTaskById.repeatFlag : data.RepeatFlag;
      timeData1.RepeatFrom = data == null ? thinTaskById.repeatFrom : data.RepeatFrom;
      TimeData timeData = timeData1;
      List<string> stringList;
      if (data != null)
      {
        stringList = data.ExDates;
      }
      else
      {
        string[] array = ExDateSerilizer.ToArray(thinTaskById.exDates);
        stringList = array != null ? ((IEnumerable<string>) array).ToList<string>() : (List<string>) null;
      }
      timeData.ExDates = stringList;
      timeData1.TimeZone = data == null ? new TimeZoneViewModel(thinTaskById.Floating, thinTaskById.timeZone) : data.TimeZone;
      TimeData timeData2 = timeData1;
      List<TaskReminderModel> taskReminderModelList1;
      if (data == null)
      {
        List<TaskReminderModel> taskReminderModelList2;
        if (!thinTaskById.startDate.HasValue)
          taskReminderModelList2 = TimeData.GetDefaultAllDayReminders();
        else
          taskReminderModelList2 = await TaskReminderDao.GetRemindersByTaskId(thinTaskById.id);
        taskReminderModelList1 = taskReminderModelList2;
      }
      else
        taskReminderModelList1 = data.Reminders;
      timeData2.Reminders = taskReminderModelList1;
      TimeData model = timeData1;
      timeData2 = (TimeData) null;
      timeData1 = (TimeData) null;
      await TaskService.SetDate(this.Id, model);
      SyncManager.TryDelaySync();
    }

    public async Task CommitGroup()
    {
      TaskModel taskById = await TaskDao.GetTaskById(this.Id);
      TimelineGroupViewModel group = this.Parent.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= this._line));
      if (group == null)
        ;
      else if (taskById == null)
        ;
      else
      {
        if (!group.IsOpen)
          this.Operation = TimelineCellOperation.Fold;
        switch (this.Parent.TimelineSortOption.groupBy)
        {
          case "tag":
            break;
          case "priority":
            int result;
            if (!int.TryParse(group.Id ?? "", out result))
              break;
            await TaskService.SetPriority(this.Id, result);
            break;
          case "project":
            ProjectModel projectById = CacheManager.GetProjectById(group.Id);
            if (projectById == null)
              break;
            if (!projectById.IsEnable())
              break;
            if (!(taskById.projectId != group.Id))
              break;
            await TaskService.MoveProject(this.Id, group.Id, new bool?(false));
            break;
          case "assignee":
            if (!(taskById.assignee != group.Id))
              break;
            if (!string.IsNullOrEmpty(group.Id) && group.Id != "-1")
            {
              List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(taskById.projectId);
              if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != group.Id)))
              {
                Utils.Toast(Utils.GetString("ChangeAssigneeError"));
                break;
              }
            }
            await TaskService.SetAssignee(this.Id, group.Id, false);
            break;
          default:
            if (!(taskById.columnId != group.Id))
              break;
            taskById.columnId = group.Id;
            if (!string.IsNullOrEmpty(taskById.parentId))
              await TaskDao.UpdateParent(taskById.id, string.Empty);
            await TaskService.SaveTaskColumnId(this.Id, group.Id);
            break;
        }
      }
    }

    private void SetIcon()
    {
      if (this.DisplayModel == null)
        this.Icon = (Geometry) null;
      else
        this.Icon = TimelineCellViewModel.GetIcon(this.DisplayModel.Kind == "NOTE", this.DisplayModel.Status == -1);
    }

    private static Geometry GetIcon(bool note, bool abandon)
    {
      if (note)
      {
        if (TimelineCellViewModel._noteIcon == null)
          TimelineCellViewModel._noteIcon = Utils.GetIcon("IcNoteIndicator");
        return TimelineCellViewModel._noteIcon;
      }
      if (!abandon)
        return (Geometry) null;
      if (TimelineCellViewModel._abandonIcon == null)
        TimelineCellViewModel._abandonIcon = Utils.GetIcon("IcCalAbandonedIndicator");
      return TimelineCellViewModel._abandonIcon;
    }

    private void SetColor()
    {
      if (this.DisplayModel == null)
        return;
      bool isDark = this.Parent.IsDark;
      int status = this.DisplayModel.Status;
      int priority = this.DisplayModel.Priority;
      bool flag1 = this.Operation.Contain(TimelineCellOperation.Edit);
      bool flag2 = this.Operation.Contain(TimelineCellOperation.BatchSelect);
      TimelineColorType timelineColorType = flag2 ? TimelineColorType.NoColor : this.Parent.ColorType;
      string empty = string.Empty;
      this.TitleOpacity = timelineColorType != TimelineColorType.NoColor ? (status == 0 & flag1 ? 1.0 : (status == 0 | flag1 ? 0.8 : (this.Parent.IsDark ? 0.2 : 0.4))) : (status != 0 ? (flag1 ? 0.6 : (!isDark ? 0.3 : 0.2)) : (flag1 ? 1.0 : 0.9));
      string str;
      switch (timelineColorType)
      {
        case TimelineColorType.List:
          str = this.DisplayModel.Color;
          break;
        case TimelineColorType.Tag:
          str = CacheManager.GetTags(this.DisplayModel?.Tags).OrderBy<TagModel, long>((Func<TagModel, long>) (t => t.sortOrder)).FirstOrDefault<TagModel>()?.color;
          break;
        case TimelineColorType.Priority:
          str = this.DisplayModel.Type == DisplayType.Note ? ThemeUtil.GetColor("PrimaryColor").ToString() : Utils.GetPriorityColor(priority).ToString();
          break;
        default:
          str = !isDark ? (this.Status == 0 ? "#FFFFFF" : "#F5F5F5") : (this.Status == 0 ? "#3C3C3C" : "#2A2A2A");
          break;
      }
      if (isDark && str == ThemeUtil.GetColor("BaseColorOpacity40").Color.ToString())
        str = "#66FFFFFF";
      string color = str?.ToLower();
      if (string.IsNullOrEmpty(str) || str.ToLower() == "transparent")
        color = ThemeUtil.GetColorString("ColorPrimary", isDark);
      double num1 = 1.0;
      if (color != null && color.StartsWith("#") && color.Length > 7)
        num1 = (double) Convert.ToInt32(color.Substring(1, 2), 16) / 256.0;
      int num2 = (int) ((status == 0 ? (flag1 ? 100.0 : 60.0) : (flag1 ? 40.0 : 20.0)) * num1);
      this._forceShowBorder = flag2 || num2 < 10 && !isDark;
      this.BorderBrush = ThemeUtil.GetColor(flag2 ? "PrimaryColor" : "BaseColorOpacity10");
      if (flag2)
      {
        this.BackgroundBrush = isDark ? ThemeUtil.GetColorInDict(this.BorderBrush.Color.ToString(), 10) : ThemeUtil.GetBlendColor("#FFFFFF", this.BorderBrush.Color.ToString(), 1f, 0.05f);
        this.ThumbBrush = Brushes.Transparent;
        this.ForegroundBrush = ThemeUtil.GetColorInDict(isDark ? "#FFFFFF" : "#191919", 90);
      }
      else
      {
        double colorGrayscale = ColorUtils.GetColorGrayscale(color, num2, !isDark ? 1 : 0);
        this.ThumbBrush = flag1 ? Brushes.Transparent : ThemeUtil.GetColorInDict(colorGrayscale <= 0.8 ? "#FFFFFF" : "#191919", colorGrayscale > 0.8 && status != 0 || isDark && status != 0 ? 20 : (isDark || colorGrayscale <= 0.8 || status != 0 ? 60 : 40));
        if (!flag1 && !isDark && colorGrayscale < 0.5)
          num2 = 40;
        if (flag1 && !isDark && colorGrayscale < 0.167)
          num2 = 80;
        this.ForegroundBrush = ThemeUtil.GetColorInDict(isDark || flag1 && this.Inline && colorGrayscale <= 0.65 ? "#FFFFFF" : "#191919", 90);
        if (timelineColorType == TimelineColorType.NoColor)
          num2 = 100;
        this.BackgroundBrush = ThemeUtil.GetColorInDict(color, num2);
      }
    }

    public void SetLeft()
    {
      this.Left = (double) (this.StartDate.Date - this.Parent.StartDate).Days * this.Parent.OneDayWidth;
    }

    public int Status
    {
      get
      {
        TaskBaseViewModel displayModel = this.DisplayModel;
        return displayModel == null ? 0 : displayModel.Status;
      }
    }

    public override string Title => this.DisplayModel?.TitleWithoutLink;

    public long SortOrder
    {
      get
      {
        TaskBaseViewModel displayModel = this.DisplayModel;
        return displayModel == null ? 0L : displayModel.SortOrder;
      }
    }

    public string Id => this.DisplayModel?.Id;

    public int Progress
    {
      get
      {
        TaskBaseViewModel displayModel = this.DisplayModel;
        return displayModel == null ? 0 : displayModel.Progress;
      }
    }

    public double MaxWidth
    {
      get => this._maxWidth;
      set => this.ChangeAndNotify<double>(ref this._maxWidth, value, nameof (MaxWidth));
    }

    public bool IsAllDay
    {
      get => this._isAllDay;
      set
      {
        if (this._isAllDay == value)
          return;
        this.ChangeAndNotify<bool>(ref this._isAllDay, value, nameof (IsAllDay));
      }
    }

    public SolidColorBrush BackgroundBrush
    {
      get => this._backgroundBrush;
      set
      {
        this._backgroundBrush = value;
        this.OnPropertyChanged(nameof (BackgroundBrush));
      }
    }

    public SolidColorBrush BorderBrush
    {
      get => this._borderBrush;
      set
      {
        this._borderBrush = value;
        this.OnPropertyChanged(nameof (BorderBrush));
      }
    }

    public SolidColorBrush ThumbBrush
    {
      get => this._thumbBrush;
      set
      {
        this._thumbBrush = value;
        this.OnPropertyChanged(nameof (ThumbBrush));
      }
    }

    public SolidColorBrush ForegroundBrush
    {
      get => this._foregroundBrush;
      set
      {
        this._foregroundBrush = value;
        this.OnPropertyChanged(nameof (ForegroundBrush));
      }
    }

    public double TitleOpacity
    {
      get => this._titleOpacity;
      set => this.ChangeAndNotify<double>(ref this._titleOpacity, value, nameof (TitleOpacity));
    }

    public Geometry Icon
    {
      get => this._icon;
      set => this.ChangeAndNotify<Geometry>(ref this._icon, value, nameof (Icon));
    }

    public bool Inline
    {
      get => this.IsNew || this._inline;
      set
      {
        if (this._inline == value)
          return;
        this._inline = value;
        this.OnPropertyChanged(nameof (Inline));
      }
    }

    public double BorderThickness
    {
      get
      {
        TimelineViewModel parent = this.Parent;
        return (parent != null ? (parent.ColorType == TimelineColorType.NoColor ? 1 : 0) : 0) != 0 && !this.Parent.IsDark || this._forceShowBorder ? 1.0 : 0.0;
      }
    }

    public bool IsNew { get; set; }

    public double Width
    {
      get
      {
        if (this._width < 0.1)
          this.SetWidth();
        return this._width;
      }
    }

    public void SetWidth()
    {
      int val2 = 1;
      double oneDayWidth = this.Parent.OneDayWidth;
      DateTime? endDate = this.EndDate;
      if (endDate.HasValue)
        val2 = (endDate.GetValueOrDefault().AddSeconds(-1.0).Date - this.StartDate.Date).Days + 1;
      double num = Math.Max((double) Math.Max(1, val2) * oneDayWidth - 4.0, 6.0);
      if (Math.Abs(num - this._width) <= 0.1)
        return;
      this._width = num;
      this.Inline = num > 24.0;
      this.OnPropertyChanged("Width");
    }

    public Visibility Visibility
    {
      get => this._dragStatus != 1 ? this._visibility : Visibility.Visible;
      set
      {
        this._visibility = value;
        this.OnPropertyChanged(nameof (Visibility));
      }
    }

    public bool IsParent
    {
      get => this._isParent;
      set
      {
        if (this._isParent == value)
          return;
        this._isParent = value;
        this.OnPropertyChanged(nameof (IsParent));
      }
    }

    public bool IsOpen
    {
      get => this._isOpen;
      set
      {
        if (this._isOpen == value)
          return;
        this._isOpen = value;
        this.OnPropertyChanged(nameof (IsOpen));
      }
    }

    public string Tips
    {
      get
      {
        TimelineCellOperation pos = this.Operation.GetPos();
        DateTime? nullable1 = this.EndDate;
        ref DateTime? local = ref nullable1;
        DateTime? nullable2 = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(this.IsAllDay ? -1.0 : 0.0)) : new DateTime?();
        string str1 = string.Empty;
        DateTime startDate;
        if (pos != TimelineCellOperation.Full)
        {
          int num1;
          if (nullable2.HasValue)
          {
            startDate = nullable2.Value;
            DateTime date1 = startDate.Date;
            startDate = this.StartDate;
            DateTime date2 = startDate.Date;
            num1 = (date1 - date2).Days + 1;
          }
          else
            num1 = 1;
          int num2 = num1;
          str1 = string.Format("{0} ", (object) num2) + (num2 > 1 ? Utils.GetString("PublicDays") : Utils.GetString("PublicDay"));
        }
        if (pos != TimelineCellOperation.Start)
        {
          if (pos != TimelineCellOperation.Full && pos == TimelineCellOperation.End)
          {
            nullable1 = nullable2;
            DateTime date = nullable1 ?? this.StartDate;
            string str2 = date.ToString("ddd");
            return !Utils.IsEn() ? ticktick_WPF.Util.DateUtils.FormatShortMonthDay(date) + " " + str2 + ", " + str1 : str2 + " " + ticktick_WPF.Util.DateUtils.FormatShortMonthDay(date) + ", " + str1;
          }
          return !nullable2.HasValue ? ticktick_WPF.Util.DateUtils.FormatShortMonthDay(this.StartDate) ?? "" : ticktick_WPF.Util.DateUtils.FormatShortMonthDay(this.StartDate) + " - " + ticktick_WPF.Util.DateUtils.FormatShortMonthDay(nullable2.Value);
        }
        startDate = this.StartDate;
        string str3 = startDate.ToString("ddd");
        return !Utils.IsEn() ? ticktick_WPF.Util.DateUtils.FormatShortMonthDay(this.StartDate) + " " + str3 + ", " + str1 : str3 + " " + ticktick_WPF.Util.DateUtils.FormatShortMonthDay(this.StartDate) + ", " + str1;
      }
    }

    public TimelineCellOperation Operation
    {
      get => this._operation;
      set
      {
        if (this._operation == value)
          return;
        int num1 = this._operation.Contain(TimelineCellOperation.Edit) != value.Contain(TimelineCellOperation.Edit) ? 1 : 0;
        bool flag = this._operation.Contain(TimelineCellOperation.BatchSelect) != value.Contain(TimelineCellOperation.BatchSelect);
        this._operation = value;
        if (this._operation.RemovePos() == TimelineCellOperation.None || this._operation.Contain(TimelineCellOperation.BatchSelect) || this._operation.Contain(TimelineCellOperation.Edit) && !this._operation.Contain(TimelineCellOperation.Hover))
        {
          if (this.Visibility != Visibility.Visible)
            this.Visibility = Visibility.Visible;
        }
        else if (this.Visibility != Visibility.Collapsed)
          this.Visibility = Visibility.Collapsed;
        if (value.IsEditingOrMoving())
          this.OnPropertyChanged("Tips");
        int num2 = flag ? 1 : 0;
        if ((num1 | num2) != 0)
          this.NotifyColorChanged(true);
        if (!flag)
          return;
        this.Parent.OnItemBatchSelectChanged(this);
        this.OnPropertyChanged("BatchSelected");
      }
    }

    public DateTime StartDate
    {
      get => this._startDate;
      private set
      {
        if (!(this._startDate != value))
          return;
        this._startDate = value;
        this.OnPropertyChanged(nameof (StartDate));
        this.OnPropertyChanged("Tips");
        this.SetWidth();
      }
    }

    public void TrySetStartDate(DateTime? time)
    {
      if (time.HasValue)
        this.StartDate = time.GetValueOrDefault();
      else
        this.StartDate = new DateTime();
    }

    public DateTime? EndDate
    {
      get => this._endDate;
      set
      {
        DateTime? endDate = this._endDate;
        DateTime? nullable = value;
        if ((endDate.HasValue == nullable.HasValue ? (endDate.HasValue ? (endDate.GetValueOrDefault() != nullable.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
          return;
        this._endDate = value;
        this.OnPropertyChanged(nameof (EndDate));
        this.OnPropertyChanged("Tips");
        this.SetWidth();
      }
    }

    public double Left
    {
      get => this._left;
      set
      {
        if (Math.Abs(this._left - value) < 0.05)
          return;
        this._left = value;
        this.OnPropertyChanged(nameof (Left));
        this.Parent?.NotifyItemPosChanged(this);
      }
    }

    public bool SetLeft(double left, bool notify)
    {
      if (Math.Abs(this._left - left) < 0.05)
        return false;
      this._left = left;
      this.OnPropertyChanged("Left");
      if (notify)
        this.Parent?.NotifyItemPosChanged(this);
      return true;
    }

    public double Top { get; private set; } = -1.0;

    public int Line
    {
      get => this._line;
      set
      {
        if (this._line != value)
        {
          this._line = value;
          this.OnPropertyChanged(nameof (Line));
        }
        double num = (double) this.Line * 40.0;
        if (Math.Abs(this.Top - num) <= 0.1)
          return;
        this.Top = num;
        this.OnPropertyChanged("Top");
        this.Parent?.NotifyItemPosChanged(this);
      }
    }

    public void SetLine(int line, bool notify)
    {
      if (this._line != line)
      {
        this._line = line;
        this.OnPropertyChanged("Line");
      }
      double num = (double) this.Line * 40.0;
      if (Math.Abs(this.Top - num) <= 0.1)
        return;
      this.Top = num;
      this.OnPropertyChanged("Top");
      if (!notify)
        return;
      this.Parent?.NotifyItemPosChanged(this);
    }

    public void SetLineValue(int line)
    {
      if (this._line.Equals(line))
        return;
      this._line = line;
    }

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

    public async void SetAvatar()
    {
      TimelineCellViewModel timelineCellViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(timelineCellViewModel.AvatarUrl);
      timelineCellViewModel.Avatar = avatarByUrlAsync;
      timelineCellViewModel.OnPropertyChanged("Avatar");
    }

    public int DragStatus
    {
      get => this._dragStatus;
      set
      {
        if (value == this._dragStatus)
          return;
        this._dragStatus = value;
        this.OnPropertyChanged("Visibility");
        this.OnPropertyChanged(nameof (DragStatus));
      }
    }

    public TimelineViewModel Parent { get; }

    public TimelineCellViewModel ParentItem { get; set; }

    public TaskBaseViewModel DisplayModel { get; private set; }

    public override DisplayType Type => this.DisplayModel.Type;

    public bool Editable => this.DisplayModel.Editable;

    public bool BatchSelected => this.Operation.Contain(TimelineCellOperation.BatchSelect);

    public int Level
    {
      get => this._level;
      set
      {
        this._level = value;
        this.OnPropertyChanged(nameof (Level));
      }
    }

    public TimelineCellViewModel(TimelineViewModel parent, TaskBaseViewModel displayModel)
    {
      TimelineCellViewModel timelineCellViewModel = this;
      this.Parent = parent ?? TimelineViewModel.Instance;
      Task.Run((Action) (() => PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) displayModel, new EventHandler<PropertyChangedEventArgs>(timelineCellViewModel.OnModelPropertyChanged), string.Empty)));
      this.SetBaseVm(displayModel);
    }

    private void Rebind(TaskBaseViewModel remove, TaskBaseViewModel add)
    {
      if (remove != null)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) remove, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
      if (add == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) add, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
    }

    private void SetBaseVm(TaskBaseViewModel baseVm)
    {
      if (baseVm == null)
        return;
      this.DisplayModel = baseVm;
      this._startDate = this.DisplayModel.StartDate ?? DateTime.Today;
      this._endDate = this.DisplayModel.DueDate;
      this._isAllDay = ((int) this.DisplayModel.IsAllDay ?? 1) != 0 || !this.DisplayModel.StartDate.HasValue;
      this._avatarUrl = !string.IsNullOrEmpty(this.DisplayModel.Assignee) ? AvatarHelper.GetCacheUrl(this.DisplayModel.Assignee, this.DisplayModel.ProjectId) : string.Empty;
      this.SetIcon();
      this.SetAvatar();
    }

    public void ResetBaseVm(TaskBaseViewModel baseVm)
    {
      TaskBaseViewModel old = this.DisplayModel;
      this.DisplayModel = baseVm;
      if (old != baseVm)
        Task.Run((Action) (() => this.Rebind(old, baseVm)));
      if (baseVm == null)
        return;
      this.StartDate = this.DisplayModel.StartDate ?? DateTime.Today;
      this.EndDate = this.DisplayModel.DueDate;
      this.IsAllDay = ((int) this.DisplayModel.IsAllDay ?? 1) != 0 || !this.DisplayModel.StartDate.HasValue;
      this._avatarUrl = !string.IsNullOrEmpty(this.DisplayModel.Assignee) ? AvatarHelper.GetCacheUrl(this.DisplayModel.Assignee, this.DisplayModel.ProjectId) : string.Empty;
      this.OnPropertyChanged("Id");
      this.OnPropertyChanged("Title");
      this.OnPropertyChanged("Status");
      this.SetIcon();
      this.SetAvatar();
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Tag") || this.Parent.ColorType != TimelineColorType.Tag)
            return;
          this.SetColor();
          return;
        case 4:
          if (!(propertyName == "Kind"))
            return;
          this.SetIcon();
          return;
        case 5:
          switch (propertyName[0])
          {
            case 'C':
              if (!(propertyName == "Color"))
                return;
              break;
            case 'T':
              if (!(propertyName == "Title"))
                return;
              this.OnPropertyChanged("Title");
              return;
            default:
              return;
          }
          break;
        case 6:
          if (!(propertyName == "Status"))
            return;
          this.OnPropertyChanged("Status");
          this.Operation = this.Operation.Remove(TimelineCellOperation.Edit);
          this.SetColor();
          this.SetIcon();
          return;
        case 7:
          if (!(propertyName == "DueDate") || !(sender is TaskBaseViewModel taskBaseViewModel1))
            return;
          this.EndDate = taskBaseViewModel1.DueDate;
          return;
        case 8:
          switch (propertyName[2])
          {
            case 'A':
              if (!(propertyName == "IsAllDay") || !(sender is TaskBaseViewModel taskBaseViewModel2))
                return;
              this.IsAllDay = ((int) taskBaseViewModel2.IsAllDay ?? 1) != 0;
              return;
            case 'i':
              if (!(propertyName == "Priority"))
                return;
              break;
            case 'o':
              if (!(propertyName == "Progress"))
                return;
              this.OnPropertyChanged("Progress");
              return;
            case 's':
              if (!(propertyName == "Assignee"))
                return;
              this.AvatarUrl = !string.IsNullOrEmpty(this.DisplayModel?.Assignee) ? AvatarHelper.GetCacheUrl(this.DisplayModel?.Assignee, this.DisplayModel?.ProjectId) : string.Empty;
              return;
            default:
              return;
          }
          break;
        case 9:
          if (!(propertyName == "StartDate") || !(sender is TaskBaseViewModel taskBaseViewModel3))
            return;
          this.TrySetStartDate(taskBaseViewModel3.StartDate);
          return;
        default:
          return;
      }
      this.SetColor();
    }

    public async Task SetColorAndAvatar()
    {
      Task.Yield();
      if (this.Avatar == null)
        this.SetAvatar();
      if (this.BackgroundBrush != null)
        return;
      this.SetColor();
    }

    public void NotifyWidthChanged() => this.OnPropertyChanged("Width");

    public void NotifyColorChanged(bool force = false)
    {
      if (!(this.BackgroundBrush != null | force))
        return;
      this.SetColor();
      this.OnPropertyChanged("BorderThickness");
    }

    public double GetLeft() => this._left < 0.0 && this._left + this.Width > 0.0 ? 0.0 : this._left;

    public void CheckAvatarUrl()
    {
      if (string.IsNullOrEmpty(this.DisplayModel.Assignee) || !(this.DisplayModel.Assignee != "-1") || !string.IsNullOrEmpty(this._avatarUrl))
        return;
      this._avatarUrl = AvatarHelper.GetCacheUrl(this.DisplayModel.Assignee, this.DisplayModel.ProjectId);
    }

    public bool AllParentItemOpen()
    {
      for (TimelineCellViewModel parentItem = this.ParentItem; parentItem != null; parentItem = parentItem.ParentItem)
      {
        if (!parentItem.IsOpen)
          return false;
      }
      return true;
    }

    public void ToggleArrangeItemOpen()
    {
      if (this.Parent == null)
        return;
      this.Parent.ToggleArrangeItemOpen(this);
    }
  }
}
