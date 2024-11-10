// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskModifyModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskModifyModel
  {
    public string id;
    public string action;
    public string when;
    public string desc;
    public string title;
    public string content;
    public string kind;
    public string description;
    public long who;
    public long assignee;
    public bool isAllDay = true;
    public bool isAllDayBefore = true;
    public bool isFloatingBefore;
    public bool isFloating;
    public string startDate;
    public string startDateBefore;
    public string dueDateBefore;
    public string dueDate;
    public string itemReminderDateBefore;
    public string itemReminderDate;
    public string itemTimeZone;
    public string timeZoneBefore;
    public string timeZone;
    public string parent;
    public string progress;
    public string progressBefor;
    public ProfileModel whoProfile;
    public ProfileModel assigneeProfile;
    public List<ModifyAttachmentModel> attachments;
    public List<string> itemTitles;
    public string fromProjectId;
    public string toProjectId;
    public string completedTime;
  }
}
