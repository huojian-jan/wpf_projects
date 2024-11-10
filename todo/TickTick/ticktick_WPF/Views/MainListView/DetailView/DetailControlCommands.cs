// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.DetailView.DetailControlCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MainListView.DetailView
{
  public static class DetailControlCommands
  {
    public static readonly ICommand SetPriorityNoneCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_no_priority");
      ((TaskDetailView) o).SetPriority(0);
    }));
    public static readonly ICommand SetPriorityLowCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_low_priority");
      ((TaskDetailView) o).SetPriority(1);
    }));
    public static readonly ICommand SetPriorityMediumCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_medium_priority");
      ((TaskDetailView) o).SetPriority(3);
    }));
    public static readonly ICommand SetPriorityHighCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_high_priority");
      ((TaskDetailView) o).SetPriority(5);
    }));
    public static readonly ICommand SetTodayCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailView) o).SetDate("today")));
    public static readonly ICommand SetTomorrowCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailView) o).SetDate("tomorrow")));
    public static readonly ICommand SetNextWeekCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailView) o).SetDate("nextweek")));
    public static readonly ICommand SelectDateCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailView) o).SelectDate()));
    public static readonly ICommand ClearDateCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "clear_time");
      ((TaskDetailView) o).ClearDate();
    }));
    public static readonly ICommand CompleteCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "complete_tasks");
      ((TaskDetailView) o).ToggleTaskCompleted();
    }));
    public static readonly ICommand PrintCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("general", "print");
      ((TaskDetailView) o).OnPrint();
    }));
    public static readonly ICommand EscCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailView) o).OnEsc()));
    public static readonly ICommand PinCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "pin_tasks");
      ((TaskDetailView) o).OnPin();
    }));
    public static readonly ICommand OpenAsSticky = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "open_as_sticky_note");
      ((TaskDetailView) o).OpenAsSticky();
    }));
  }
}
