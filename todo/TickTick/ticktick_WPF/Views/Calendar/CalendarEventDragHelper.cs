// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarEventDragHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class CalendarEventDragHelper
  {
    public static void CheckIfEventCanDrag(
      CalendarDisplayViewModel model,
      out bool canDrag,
      out bool needToast,
      out string toastStr)
    {
      canDrag = true;
      needToast = false;
      toastStr = (string) null;
      if (model.IsCalendarEvent)
      {
        BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == model.CalendarId));
        if (bindCalendarModel == null || !bindCalendarModel.Accessible)
        {
          canDrag = false;
          needToast = false;
        }
      }
      if (model.IsCheckItem)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(model.GetTaskId());
        if (taskById == null || !string.IsNullOrEmpty(taskById.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) taskById))
        {
          canDrag = false;
          needToast = true;
          toastStr = Utils.GetString("AttendeeSetDate");
        }
      }
      if (!string.IsNullOrEmpty(model.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) model))
      {
        canDrag = false;
        needToast = true;
        toastStr = Utils.GetString("AttendeeSetDate");
      }
      if (model.IsHabit)
      {
        canDrag = false;
        needToast = true;
        toastStr = Utils.GetString("CannotDragHabit");
      }
      if (model.IsPomo)
      {
        canDrag = false;
        needToast = false;
      }
      if (model.Course == null)
        return;
      canDrag = false;
      needToast = true;
      toastStr = Utils.GetString("OperationNotSupport");
    }
  }
}
