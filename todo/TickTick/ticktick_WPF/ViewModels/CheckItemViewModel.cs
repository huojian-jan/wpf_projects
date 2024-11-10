// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CheckItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class CheckItemViewModel : BaseViewModel
  {
    private bool _showAddHint;
    private bool _showBottomDropLine;
    private bool _showTopDropLine;

    public TaskBaseViewModel SourceViewModel { get; set; }

    public CheckItemViewModel()
    {
    }

    public CheckItemViewModel(TaskBaseViewModel vm, TaskDetailViewModel task = null)
    {
      this.SetSourceModel(vm);
      this.RepeatDiff = ((double?) task?.RepeatDiff).GetValueOrDefault();
      this.IsValid = true;
    }

    public double RepeatDiff { get; set; }

    public string Id => this.SourceViewModel.Id;

    public string TaskServerId => this.SourceViewModel.ParentId;

    public bool ShowDragHandle => this.SourceViewModel.Status == 0;

    public bool Enable { get; set; } = true;

    public bool IsAgendaOwner { get; set; } = true;

    public string TimeZoneName
    {
      get
      {
        if (this.SourceViewModel.OwnerTask != null && ((int) this.SourceViewModel.OwnerTask.IsAllDay ?? 1) == 0)
          return this.SourceViewModel.OwnerTask.TimeZoneName;
        return TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
      }
    }

    public bool IsFloating
    {
      get
      {
        return this.SourceViewModel.OwnerTask != null && ((int) this.SourceViewModel.OwnerTask.IsAllDay ?? 1) == 0 && this.SourceViewModel.OwnerTask.IsFloating;
      }
    }

    private bool _isValid { get; set; }

    public string Title => this.SourceViewModel.Title;

    public int Status => this.SourceViewModel.Status;

    public long SortOrder => this.SourceViewModel.SortOrder;

    public DateTime? CompletedTime => this.SourceViewModel.CompletedTime;

    public DateTime? StartDate => this.SourceViewModel.StartDate;

    public DateTime? DisplayStartDate
    {
      get
      {
        DateTime? startDate = this.StartDate;
        ref DateTime? local = ref startDate;
        return !local.HasValue ? new DateTime?() : new DateTime?(local.GetValueOrDefault().AddDays(this.RepeatDiff));
      }
    }

    public DateTime? SnoozeReminderTime => this.SourceViewModel.RemindTime;

    public bool? IsAllDay => this.SourceViewModel.IsAllDay;

    public bool ShowTopDropLine
    {
      get => this._showTopDropLine;
      set
      {
        this._showTopDropLine = value;
        this.OnPropertyChanged(nameof (ShowTopDropLine));
      }
    }

    public bool ShowBottomDropLine
    {
      get => this._showBottomDropLine;
      set
      {
        this._showBottomDropLine = value;
        this.OnPropertyChanged(nameof (ShowBottomDropLine));
      }
    }

    public bool ShowAddHint
    {
      get => this._showAddHint;
      set
      {
        this._showAddHint = value;
        this.OnPropertyChanged("HintVisible");
      }
    }

    public bool HintVisible => this.ShowAddHint && string.IsNullOrEmpty(this.Title);

    public bool IsValid
    {
      get => this._isValid;
      set
      {
        this._isValid = value;
        this.OnPropertyChanged(nameof (IsValid));
      }
    }

    public CheckItemViewModel Clone()
    {
      return new CheckItemViewModel()
      {
        SourceViewModel = this.SourceViewModel
      };
    }

    public static List<CheckItemViewModel> BuildListFromModels(
      List<TaskDetailItemModel> models,
      TaskDetailViewModel task)
    {
      List<CheckItemViewModel> checkItemViewModelList = new List<CheckItemViewModel>();
      if (models != null && models.Count > 0)
        checkItemViewModelList.AddRange(models.Select<TaskDetailItemModel, CheckItemViewModel>((Func<TaskDetailItemModel, CheckItemViewModel>) (model => new CheckItemViewModel(new TaskBaseViewModel()
        {
          Id = model.id,
          Title = model.title,
          SortOrder = model.sortOrder,
          CompletedTime = model.completedTime,
          Type = DisplayType.CheckItem,
          IsAllDay = model.isAllDay,
          RemindTime = model.snoozeReminderTime
        }, task))));
      return checkItemViewModelList;
    }

    public TaskDetailItemModel ToItemModel()
    {
      return new TaskDetailItemModel()
      {
        id = this.Id,
        TaskServerId = this.TaskServerId,
        status = this.Status,
        sortOrder = this.SortOrder,
        completedTime = this.CompletedTime,
        startDate = this.StartDate,
        isAllDay = this.IsAllDay,
        title = this.Title,
        snoozeReminderTime = this.SnoozeReminderTime
      };
    }

    public async Task Save(bool checkCount = true)
    {
      await TaskDetailItemDao.SaveChecklistItem(this.ToItemModel(), false, checkCount);
    }

    public async Task Insert() => await TaskDetailItemDao.InsertChecklistItem(this.ToItemModel());

    public void SetSourceModel(TaskBaseViewModel vm)
    {
      this.SourceViewModel = vm;
      if (this.SourceViewModel == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        this.OnPropertyChanged("Title");
        this.OnPropertyChanged("HintVisible");
      }), "Title");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("Status")), "Status");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("DisplayStartDate")), "StartDate");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("SortOrder")), "SortOrder");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("IsAllDay")), "IsAllDay");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("SnoozeReminderTime")), "RemindTime");
    }
  }
}
