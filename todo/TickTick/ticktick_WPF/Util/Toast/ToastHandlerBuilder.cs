// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Toast.ToastHandlerBuilder
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.Toast
{
  public class ToastHandlerBuilder
  {
    public static IToastHandler Build(ToastType type)
    {
      if (ToastHandlerBuilder.IsRemindToast(type))
        return (IToastHandler) new ReminderToastHandler();
      if (ToastHandlerBuilder.IsPomoToast(type))
        return (IToastHandler) new PomoToastHandler();
      return ToastHandlerBuilder.IsHabitToast(type) ? (IToastHandler) new HabitToastHandler() : (IToastHandler) null;
    }

    public static bool IsRemindToast(ToastType type)
    {
      return type == ToastType.RemindTask || type == ToastType.RemindCheckItem || type == ToastType.RemindEvent || type == ToastType.RemindHabit || type == ToastType.RemindCourse;
    }

    public static bool IsPomoToast(ToastType type)
    {
      return type == ToastType.PomoCompleted || type == ToastType.PomoRelaxCompleted;
    }

    public static bool IsHabitToast(ToastType type)
    {
      return type == ToastType.MultiHabitCompleted || type == ToastType.BoolHabitCompleted;
    }
  }
}
