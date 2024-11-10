// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.IndependentWindowCommand
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class IndependentWindowCommand
  {
    public static readonly ICommand SyncCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.SyncCommand));
    public static readonly ICommand InputCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.InputCommand(o, (ProjectIdentity) null)));
    public static readonly ICommand PrintCommand = (ICommand) new RelayCommand((Action<object>) (o => ((IndependentWindow) o).Print()));
    public static readonly ICommand SetPriorityNoneCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetPriorityCommand(o, 0)));
    public static readonly ICommand SetPriorityLowCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetPriorityCommand(o, 1)));
    public static readonly ICommand SetPriorityMediumCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetPriorityCommand(o, 3)));
    public static readonly ICommand SetPriorityHighCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetPriorityCommand(o, 5)));
    public static readonly ICommand SetTodayCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetDateCommand(o, "today")));
    public static readonly ICommand SetTomorrowCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetDateCommand(o, "tomorrow")));
    public static readonly ICommand SetNextWeekCommand = (ICommand) new RelayCommand((Action<object>) (o => KeyBindingCommand.SetDateCommand(o, "nextweek")));
    public static readonly ICommand ClearDateCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.ClearDateCommand));
    public static readonly ICommand PinTaskCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.PinTaskCommand));
    public static readonly ICommand DeleteCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.DeleteCommand));
    public static readonly ICommand OpenStickyCommand = (ICommand) new RelayCommand(new Action<object>(KeyBindingCommand.OpenStickyCommand));
    public static readonly ICommand ListViewCommand = (ICommand) new RelayCommand((Action<object>) (o => ((IndependentWindow) o).SwitchViewMode("list")));
    public static readonly ICommand KanbanViewCommand = (ICommand) new RelayCommand((Action<object>) (o => ((IndependentWindow) o).SwitchViewMode("kanban")));
    public static readonly ICommand TimelineViewCommand = (ICommand) new RelayCommand((Action<object>) (o => ((IndependentWindow) o).SwitchViewMode("timeline")));
  }
}
