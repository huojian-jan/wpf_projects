// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Toast.ToastOptionModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Toast
{
  public class ToastOptionModel
  {
    public string Id { get; set; }

    public object Parma { get; set; }

    public DateTime StartTime { get; set; }

    public ToastType ToastType { get; set; }

    public string TargetId { get; set; }

    public string CurrentOptionName { get; set; }

    public ToastOptionModel() => this.Id = Utils.GetGuid0(16);

    public ToastOptionModel(ReminderModel reminderModel)
    {
      this.Id = Utils.GetGuid0(16);
      if (!string.IsNullOrEmpty(reminderModel.HabitId))
      {
        this.TargetId = reminderModel.HabitId;
        this.ToastType = ToastType.RemindHabit;
      }
      else if (!string.IsNullOrEmpty(reminderModel.EventId))
      {
        this.TargetId = reminderModel.EventId;
        this.ToastType = ToastType.RemindEvent;
      }
      else if (!string.IsNullOrEmpty(reminderModel.CheckItemId))
      {
        this.TargetId = reminderModel.CheckItemId;
        this.ToastType = ToastType.RemindCheckItem;
      }
      else if (reminderModel.Type == 8)
      {
        this.TargetId = reminderModel.Id;
        this.ToastType = ToastType.RemindCourse;
      }
      else
      {
        this.TargetId = reminderModel.TaskId;
        this.ToastType = ToastType.RemindTask;
      }
      this.Parma = (object) reminderModel.ReminderTime;
      this.StartTime = reminderModel.StartDate ?? DateTime.Now;
    }

    public string ToJson(string currentName)
    {
      this.CurrentOptionName = currentName;
      return JsonConvert.SerializeObject((object) this);
    }

    public static ToastOptionModel FromJson(string jsonStr)
    {
      if (string.IsNullOrEmpty(jsonStr))
        return (ToastOptionModel) null;
      try
      {
        return JsonConvert.DeserializeObject<ToastOptionModel>(jsonStr);
      }
      catch
      {
        return (ToastOptionModel) null;
      }
    }
  }
}
