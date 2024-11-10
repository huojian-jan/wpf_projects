// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskDisplayModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Models
{
  [Obsolete]
  public class TaskDisplayModel
  {
    public string id { get; set; }

    public int type { get; set; }

    public string taskId { get; set; }

    public string title { get; set; }

    public string content { get; set; }

    public bool? isAllDay { get; set; }

    public DateTime? startDate { get; set; }

    public DateTime? dueDate { get; set; }

    public int priority { get; set; }

    public int progress { get; set; }

    public string repeatFlag { get; set; }

    public string repeatFrom { get; set; }

    public string avatar { get; set; }

    public int status { get; set; }

    public string kind { get; set; }

    public DateTime? completedTime { get; set; }

    public DateTime? modifiedTime { get; set; }

    public DateTime? createdTime { get; set; }

    public DateTime? remindTime { get; set; }

    public long sortOrder { get; set; }

    public string color { get; set; }

    public string projectId { get; set; }

    public long projectOrder { get; set; }

    public string projectName { get; set; }

    public string assignee { get; set; }

    public string tag { get; set; }

    public string groupId { get; set; }

    public long orderInDate { get; set; }

    public string attendId { get; set; }

    public string taskTitle { get; set; }

    public bool hasLocation { get; set; }

    public string calendarId { get; set; }

    public int deleted { get; set; }

    public string columnId { get; set; }

    public long itemOrder { get; set; }

    public bool Editable { get; set; }

    public bool IsFloating { get; set; }

    public string TimeZoneName { get; set; } = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;

    public HabitModel Habit { get; set; }

    public HabitCheckInModel HabitCheckIn { get; set; }

    public string ParentId { get; set; }

    public bool IsOpen { get; set; }

    public string ChildrenIds { get; set; }

    public long pinnedTime { get; set; }

    public bool IsPinned => this.pinnedTime > 0L;

    public bool IsCompleted { get; set; }

    public bool IsAbandoned { get; set; }
  }
}
