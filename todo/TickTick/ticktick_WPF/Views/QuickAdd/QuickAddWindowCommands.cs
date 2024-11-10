// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickAddWindowCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public static class QuickAddWindowCommands
  {
    public static readonly ICommand SetPriorityNoneCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetPriority(0)));
    public static readonly ICommand SetPriorityLowCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetPriority(1)));
    public static readonly ICommand SetPriorityMediumCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetPriority(3)));
    public static readonly ICommand SetPriorityHighCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetPriority(5)));
    public static readonly ICommand SetTodayCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetDate("today")));
    public static readonly ICommand SetTomorrowCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetDate("tomorrow")));
    public static readonly ICommand SetNextWeekCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SetDate("nextweek")));
    public static readonly ICommand SelectDateCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).SelectDate()));
    public static readonly ICommand ClearDateCommand = (ICommand) new RelayCommand((Action<object>) (o => ((QuickAddWindow) o).ClearDate()));
  }
}
