// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TimeData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TimeData : BaseViewModel
  {
    public bool HasTime;
    private DateTime? _startDate;
    private DateTime? _dueDate;
    private bool? _isAllDay = new bool?(true);
    private List<TaskReminderModel> _reminders = new List<TaskReminderModel>();
    private string _repeatFrom = "2";
    private string _repeatFlag;
    private bool _isDefault = true;
    private TimeZoneViewModel _timezone = TimeZoneData.LocalTimeZoneModel;
    public BatchData BatchData;

    public List<string> ExDates { get; set; }

    public bool ChangedDateOnly => this.BatchData != null && !this.BatchData.IsUnified;

    public bool IsTimeUnified => this.BatchData == null || this.BatchData.IsTimeUnified;

    public TimeData()
    {
    }

    public TimeData(TaskModel task, List<TaskReminderModel> reminders)
    {
      this._startDate = task.startDate;
      this._dueDate = task.dueDate;
      this._isAllDay = task.isAllDay;
      this._repeatFlag = task.repeatFlag;
      this._repeatFrom = task.repeatFrom;
      this._timezone = new TimeZoneViewModel(task.isFloating.GetValueOrDefault(), task.timeZone);
      string[] array = ExDateSerilizer.ToArray(task.exDates);
      this.ExDates = (array != null ? ((IEnumerable<string>) array).ToList<string>() : (List<string>) null) ?? new List<string>();
      this._reminders = reminders;
    }

    public TimeZoneViewModel TimeZone
    {
      get => this._timezone;
      set
      {
        this._timezone = value;
        this.OnPropertyChanged(nameof (TimeZone));
      }
    }

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? DueDate
    {
      get => this._dueDate;
      set
      {
        this._dueDate = value;
        DateTime? dueDate = this._dueDate;
        DateTime? startDate = this._startDate;
        if ((dueDate.HasValue & startDate.HasValue ? (dueDate.GetValueOrDefault() < startDate.GetValueOrDefault() ? 1 : 0) : 0) != 0)
          this._dueDate = this._startDate;
        this.OnPropertyChanged(nameof (DueDate));
      }
    }

    public List<TaskReminderModel> Reminders
    {
      get => this._reminders;
      set
      {
        this._reminders = value;
        this.OnPropertyChanged(nameof (Reminders));
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

    public string RepeatFlag
    {
      get => this._repeatFlag;
      set
      {
        this._repeatFlag = value;
        this.OnPropertyChanged(nameof (RepeatFlag));
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

    public bool IsDefault
    {
      get => this._isDefault;
      set
      {
        this._isDefault = value;
        this.OnPropertyChanged(nameof (IsDefault));
      }
    }

    public static TimeData Clone(TimeData original)
    {
      if (original == null)
        return new TimeData();
      return new TimeData()
      {
        HasTime = original.HasTime,
        StartDate = original.StartDate,
        DueDate = original.DueDate,
        Reminders = original.Reminders,
        RepeatFrom = original.RepeatFrom,
        RepeatFlag = original.RepeatFlag,
        IsAllDay = original.IsAllDay,
        IsDefault = original.IsDefault,
        ExDates = original.ExDates,
        TimeZone = original.TimeZone,
        BatchData = original.BatchData
      };
    }

    public static TimeData BuildFromDefault(TaskDefaultModel model)
    {
      TimeData timeData = new TimeData();
      timeData.StartDate = TaskDefaultModel.GetDefaultDate(model.Date);
      if (!string.IsNullOrEmpty(model.AllDayReminders))
      {
        List<TaskReminderModel> taskReminderModelList = Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) model.AllDayReminders.Split(',')).ToList<string>());
        timeData.Reminders = taskReminderModelList;
      }
      return timeData;
    }

    public static TimeData BuildDefaultStartAndEnd()
    {
      TimeData timeData1 = new TimeData();
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      if (defaultSafely.DateMode == 1)
      {
        timeData1.IsAllDay = new bool?(Constants.DurationValue.IsAllDayValue(defaultSafely.Duration));
        TimeData timeData2 = timeData1;
        DateTime? nullable1;
        DateTime? nullable2;
        if (!timeData1.IsAllDay.Value)
        {
          nullable1 = defaultSafely.GetDefaultDateTime();
          nullable2 = new DateTime?(DateUtils.GetNextHour(nullable1 ?? DateTime.Today));
        }
        else
          nullable2 = defaultSafely.GetDefaultDateTime();
        timeData2.StartDate = nullable2;
        nullable1 = timeData1.StartDate;
        if (nullable1.HasValue)
        {
          TimeData timeData3 = timeData1;
          nullable1 = timeData1.StartDate;
          DateTime? nullable3 = new DateTime?(nullable1.Value.AddMinutes((double) defaultSafely.Duration));
          timeData3.DueDate = nullable3;
        }
      }
      else
        timeData1.StartDate = defaultSafely.GetDefaultDateTime();
      return timeData1;
    }

    public static List<TaskReminderModel> GetDefaultAllDayReminders()
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      List<TaskReminderModel> defaultAllDayReminders = new List<TaskReminderModel>();
      if (!string.IsNullOrEmpty(defaultSafely?.AllDayReminders))
        defaultAllDayReminders = Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) defaultSafely.AllDayReminders.Split(',')).ToList<string>());
      return defaultAllDayReminders;
    }

    public static List<TaskReminderModel> GetDefaultTimeReminders()
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      List<TaskReminderModel> defaultTimeReminders = new List<TaskReminderModel>();
      if (!string.IsNullOrEmpty(defaultSafely?.TimeReminders))
        defaultTimeReminders = Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) defaultSafely.TimeReminders.Split(',')).ToList<string>());
      return defaultTimeReminders;
    }

    public static TimeData InitDefaultTime()
    {
      List<TaskReminderModel> taskReminderModelList = new List<TaskReminderModel>();
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      DateTime? defaultDate = TaskDefaultModel.GetDefaultDate(defaultSafely?.Date);
      if (!string.IsNullOrEmpty(defaultSafely?.AllDayReminders))
        taskReminderModelList = Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) defaultSafely.AllDayReminders.Split(',')).ToList<string>());
      return new TimeData()
      {
        StartDate = defaultDate,
        IsAllDay = new bool?(true),
        IsDefault = true,
        Reminders = taskReminderModelList
      };
    }

    public static TimeData InitDefaultDuration()
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      DateTime nextHour = DateUtils.GetNextHour(DateUtils.GetNextHour(TaskDefaultModel.GetDefaultDate(defaultSafely?.Date) ?? DateTime.Today));
      DateTime dateTime = nextHour.AddHours(1.0);
      List<TaskReminderModel> taskReminderModelList = new List<TaskReminderModel>();
      if (defaultSafely != null)
      {
        dateTime = nextHour.AddMinutes((double) defaultSafely.Duration);
        if (!string.IsNullOrEmpty(defaultSafely.TimeReminders))
          taskReminderModelList = Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) defaultSafely.TimeReminders.Split(',')).ToList<string>());
      }
      return new TimeData()
      {
        StartDate = new DateTime?(nextHour),
        DueDate = new DateTime?(dateTime),
        IsAllDay = new bool?(false),
        IsDefault = true,
        Reminders = taskReminderModelList
      };
    }

    public override string ToString()
    {
      string[] strArray = new string[9];
      DateTime? nullable = this.StartDate;
      ref DateTime? local1 = ref nullable;
      DateTime valueOrDefault;
      string str1;
      if (!local1.HasValue)
      {
        str1 = (string) null;
      }
      else
      {
        valueOrDefault = local1.GetValueOrDefault();
        str1 = valueOrDefault.ToString("yyyyMMdd'-'HHMM");
      }
      strArray[0] = str1;
      strArray[1] = ",";
      nullable = this.DueDate;
      ref DateTime? local2 = ref nullable;
      string str2;
      if (!local2.HasValue)
      {
        str2 = (string) null;
      }
      else
      {
        valueOrDefault = local2.GetValueOrDefault();
        str2 = valueOrDefault.ToString("yyyyMMdd'-'HHMM");
      }
      strArray[2] = str2;
      strArray[3] = ",";
      strArray[4] = this.IsAllDay.ToString();
      strArray[5] = ",";
      strArray[6] = this.RepeatFlag;
      strArray[7] = ",";
      strArray[8] = this.TimeZone?.TimeZoneName;
      return string.Concat(strArray);
    }
  }
}
