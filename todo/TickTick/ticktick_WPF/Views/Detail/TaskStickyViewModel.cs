// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskStickyViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskStickyViewModel : BaseViewModel
  {
    public readonly TaskBaseViewModel SourceModel;
    private bool _editting;

    public string Title => this.SourceModel.Title;

    public string StickyTitle
    {
      get
      {
        return !string.IsNullOrEmpty(this.SourceModel.Title) ? this.SourceModel.Title : Utils.GetString("StickyNote");
      }
    }

    public int Status => this.SourceModel.Status;

    public string Kind => this.SourceModel.Kind;

    public bool Enable => this.SourceModel.Editable;

    public string Id => this.SourceModel.Id;

    public string ToastString { get; set; }

    public TaskStickyWindow Window { get; set; }

    public string DateText
    {
      get
      {
        if (this.SourceModel.StartDate.HasValue)
          return DateUtils.FormatDateString(this.SourceModel.StartDate.Value, this.SourceModel.DueDate, this.SourceModel.IsAllDay);
        if (this.SourceModel.Status != 0)
          return Utils.GetString("DateNotSet");
        return !(this.Kind == "NOTE") ? Utils.GetString("DateAndReminder") : Utils.GetString("SetReminder");
      }
    }

    public string RepeatText
    {
      get
      {
        string repeatFlag = this.SourceModel.RepeatFlag;
        if (RepeatUtils.GetRepeatType(this.SourceModel.RepeatFrom, repeatFlag) == RepeatFromType.Custom)
          return Utils.GetString("RepeatByCustom");
        if (string.IsNullOrEmpty(repeatFlag) || repeatFlag == "RRULE:FREQ=NONE")
          return string.Empty;
        return !repeatFlag.Contains("FREQ=DAILY") || !repeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND") || repeatFlag.Contains("INTERVAL") && !repeatFlag.Contains("INTERVAL=1") ? RRuleUtils.RRule2String(this.SourceModel.RepeatFrom, repeatFlag, this.SourceModel.StartDate, false) : Utils.GetString("OfficialWorkingDays");
      }
    }

    public TaskStickyViewModel(string taskId)
    {
      TaskBaseViewModel taskBaseViewModel = TaskCache.GetTaskById(taskId);
      if (taskBaseViewModel == null)
        taskBaseViewModel = new TaskBaseViewModel()
        {
          Id = taskId
        };
      this.SourceModel = taskBaseViewModel;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        this.OnPropertyChanged(nameof (DateText));
        this.OnPropertyChanged(nameof (Kind));
      }), nameof (Kind));
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        this.OnPropertyChanged(nameof (Title));
        this.OnPropertyChanged(nameof (StickyTitle));
      }), nameof (Title));
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        this.OnPropertyChanged(nameof (DateText));
        this.OnPropertyChanged(nameof (RepeatText));
      }), "StartDate");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (RepeatText))), "RepeatFlag");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (RepeatText))), "RepeatFrom");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (DateText))), "DueDate");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (DateText))), "IsAllDay");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (Enable))), "Editable");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged(nameof (Status))), nameof (Status));
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.SourceModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.OnPropertyChanged("Deleted")), "Deleted");
    }

    public async void CompleteTaskCommand()
    {
      if (this.Kind == "NOTE")
        return;
      int status = this.Status != 0 ? 0 : 2;
      if ((await TaskService.SetTaskStatus(this.SourceModel.Id, status, inDetail: true))?.RepeatTask != null)
      {
        this.TryToast(Utils.GetString("CreatedTheNextCircle"));
      }
      else
      {
        if (status != 2)
          return;
        this.TryToast(Utils.GetString("TaskCompleted"));
      }
    }

    public async Task ShowDateSelector(FrameworkElement sender)
    {
      TaskStickyViewModel taskStickyViewModel = this;
      TaskModel task = await TaskDao.GetThinTaskById(taskStickyViewModel.SourceModel.Id);
      TimeData timeData;
      if (task == null)
      {
        task = (TaskModel) null;
        timeData = (TimeData) null;
      }
      else
      {
        List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
        timeData = new TimeData()
        {
          StartDate = task.startDate,
          DueDate = task.dueDate,
          IsAllDay = task.isAllDay,
          Reminders = remindersByTaskId,
          RepeatFrom = task.repeatFrom,
          RepeatFlag = task.repeatFlag
        };
        if (!string.IsNullOrEmpty(task.timeZone))
          timeData.TimeZone = new TimeZoneViewModel(task.Floating, task.timeZone);
        if (timeData.IsAllDay.HasValue && timeData.IsAllDay.Value && timeData.StartDate.HasValue && timeData.DueDate.HasValue)
        {
          DateTime dateTime = timeData.StartDate.Value;
          DateTime date1 = dateTime.Date;
          dateTime = timeData.DueDate.Value;
          DateTime date2 = dateTime.Date;
          if (date1 == date2)
            timeData.DueDate = new DateTime?();
        }
        if (timeData.StartDate.HasValue)
          timeData.IsDefault = false;
        bool canSkip = !TaskService.IsNonRepeatTask(await TaskDao.GetThinTaskById(taskStickyViewModel.SourceModel.Id));
        TaskStickyWindow window = sender as TaskStickyWindow;
        bool isNote = task.kind == "NOTE";
        SetDateDialog dialog = SetDateDialog.GetDialog();
        dialog.ClearEventHandle();
        dialog.Clear += (EventHandler) (async (obj, model) => await this.ClearTaskDate());
        dialog.Save += (EventHandler<TimeData>) (async (obj, model) =>
        {
          TaskService.TryFixRepeatFlag(ref model);
          await this.SaveTaskDate(model);
        });
        dialog.Hided += (EventHandler) ((obj, e) =>
        {
          window?.Activate();
          window?.SetEditing(false);
        });
        dialog.SkipRecurrence += new EventHandler(taskStickyViewModel.OnSkipRecurrence);
        dialog.Show(timeData, new SetDateDialogArgs(isNote: isNote, target: (UIElement) window?.BottomGrid, hOffset: -20.0, vOffset: 25.0, canSkip: canSkip));
        TaskStickyWindow taskStickyWindow = window;
        if (taskStickyWindow == null)
        {
          task = (TaskModel) null;
          timeData = (TimeData) null;
        }
        else
        {
          taskStickyWindow.SetEditing(true);
          task = (TaskModel) null;
          timeData = (TimeData) null;
        }
      }
    }

    private void OnSkipRecurrence(object sender, EventArgs e)
    {
      TaskService.SkipCurrentRecurrence(this.SourceModel.Id, toastWindow: (IToastShowWindow) this.Window);
    }

    private async Task SaveTaskDate(TimeData model)
    {
      this.CheckIfReminderPassed(model);
      await TaskService.SetDate(this.SourceModel.Id, model);
    }

    private void CheckIfReminderPassed(TimeData model)
    {
      if (model == null || !model.IsAllDay.HasValue || model.IsAllDay.Value || !model.StartDate.HasValue || !(model.StartDate.Value < DateTime.Now))
        return;
      List<TaskReminderModel> reminders = model.Reminders;
      // ISSUE: explicit non-virtual call
      if ((reminders != null ? (__nonvirtual (reminders.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      this.TryToast(Utils.GetString("InvalidReminder"));
    }

    private async Task ClearTaskDate() => await TaskService.ClearDate(this.SourceModel.Id);

    private void TryToast(string getString)
    {
      this.ToastString = getString;
      this.OnPropertyChanged("ToastString");
    }

    public bool IsEmptyTask()
    {
      return this.Kind != "CHECKLIST" && string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.SourceModel.Content);
    }

    public void ResetDateText() => this.OnPropertyChanged("DateText");
  }
}
