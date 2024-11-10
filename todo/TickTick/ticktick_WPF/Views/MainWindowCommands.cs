// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainWindowCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public static class MainWindowCommands
  {
    public static readonly ICommand SyncCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SyncCommand()));
    public static readonly ICommand InputCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("task", "add task");
      ((MainWindow) o).InputCommand();
    }));
    public static readonly ICommand CloseCommand = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).CloseCommand()));
    public static readonly ICommand ExitImmersiveCommand = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).ExitImmersiveCommand()));
    public static readonly ICommand JumpTodayList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_tod");
      ((MainWindow) o).JumpSmartProject(SmartListType.Today);
    }));
    public static readonly ICommand JumpAllList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_all");
      ((MainWindow) o).JumpSmartProject(SmartListType.All);
    }));
    public static readonly ICommand JumpTomorrowList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_tom");
      ((MainWindow) o).JumpSmartProject(SmartListType.Tomorrow);
    }));
    public static readonly ICommand JumpWeekList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_7D");
      ((MainWindow) o).JumpSmartProject(SmartListType.Week);
    }));
    public static readonly ICommand JumpAssignMeList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_assign");
      ((MainWindow) o).JumpSmartProject(SmartListType.Assign);
    }));
    public static readonly ICommand JumpInboxList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_inbox");
      ((MainWindow) o).JumpSmartProject(SmartListType.Inbox);
    }));
    public static readonly ICommand JumpCompleteList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_completed");
      ((MainWindow) o).JumpSmartProject(SmartListType.Completed);
    }));
    public static readonly ICommand JumpAbandonList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_wont_do");
      ((MainWindow) o).JumpSmartProject(SmartListType.Abandoned);
    }));
    public static readonly ICommand JumpTrashList = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_trash");
      ((MainWindow) o).JumpSmartProject(SmartListType.Trash);
    }));
    public static readonly ICommand JumpSummary = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_summary");
      ((MainWindow) o).JumpSmartProject(SmartListType.Summary);
    }));
    public static readonly ICommand OpenSettingCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_settings");
      ((MainWindow) o).LeftTabBar.ShowSettingDialog();
    }));
    public static readonly ICommand PrintCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("general", "print");
      ((MainWindow) o).Print();
    }));
    public static readonly ICommand PrintDetailCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("general", "print_detail");
      ((MainWindow) o).Print(true);
    }));
    public static readonly ICommand SetPriorityNoneCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_no_priority");
      KeyBindingCommand.SetPriorityCommand(o, 0);
    }));
    public static readonly ICommand SetPriorityLowCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_low_priority");
      KeyBindingCommand.SetPriorityCommand(o, 1);
    }));
    public static readonly ICommand SetPriorityMediumCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_medium_priority");
      KeyBindingCommand.SetPriorityCommand(o, 3);
    }));
    public static readonly ICommand SetPriorityHighCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_high_priority");
      KeyBindingCommand.SetPriorityCommand(o, 5);
    }));
    public static readonly ICommand SetTodayCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_today");
      KeyBindingCommand.SetDateCommand(o, "today");
    }));
    public static readonly ICommand SetTomorrowCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_tomorrow");
      KeyBindingCommand.SetDateCommand(o, "tomorrow");
    }));
    public static readonly ICommand SetNextWeekCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_next_week");
      KeyBindingCommand.SetDateCommand(o, "nextweek");
    }));
    public static readonly ICommand SelectDateCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_date");
      ((MainWindow) o).SelectDate(false);
    }));
    public static readonly ICommand ClearDateCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.ClearDateCommand));
    public static readonly ICommand PinTaskCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.PinTaskCommand));
    public static readonly ICommand CompleteCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "complete_tasks");
      ((MainWindow) o).ToggleTaskCompleted();
    }));
    public static readonly ICommand DeleteCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.DeleteCommand));
    public static readonly ICommand ExpandAllTaskCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("task", "toggle_all_subtasks");
      ((MainWindow) o).ExpandOrFoldAllTask(true);
    }));
    public static readonly ICommand ExpandAllSectionCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("task", "toggle_all_groups");
      ((MainWindow) o).ExpandOrFoldAllSection();
    }));
    public static readonly ICommand SearchCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("navigate", "go_to_search");
      ((MainWindow) o).SearchCommand();
    }));
    public static readonly ICommand SearchOperationCommand = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).SearchOperationCommand()));
    public static readonly ICommand JumpCalendar = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpCalendar()));
    public static readonly ICommand JumpHabitList = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpHabit()));
    public static readonly ICommand Tab01Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(1)));
    public static readonly ICommand Tab02Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(2)));
    public static readonly ICommand Tab03Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(3)));
    public static readonly ICommand Tab04Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(4)));
    public static readonly ICommand Tab05Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(5)));
    public static readonly ICommand Tab06Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(6)));
    public static readonly ICommand Tab07Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(7)));
    public static readonly ICommand Tab08Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(8)));
    public static readonly ICommand Tab09Command = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).JumpTab(9)));
    public static readonly ICommand OpenStickyCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.OpenStickyCommand));
    public static readonly ICommand TabCommand = (ICommand) new RelayCommand((Action<object>) (o => ((MainWindow) o).OnTabKeyUp()));
    public static readonly ICommand ListViewCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("switch_views", "list_view");
      ((MainWindow) o).SwitchViewMode("list");
    }));
    public static readonly ICommand KanbanViewCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("switch_views", "kanban_view");
      ((MainWindow) o).SwitchViewMode("kanban");
    }));
    public static readonly ICommand TimelineViewCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("switch_views", "timeline_view");
      ((MainWindow) o).SwitchViewMode("timeline");
    }));
  }
}
