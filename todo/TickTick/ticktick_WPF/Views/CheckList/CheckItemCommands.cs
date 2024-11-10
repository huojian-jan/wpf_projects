// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CheckList.CheckItemCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.CheckList
{
  public static class CheckItemCommands
  {
    public static readonly ICommand SetTodayCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).SetDate("today")));
    public static readonly ICommand SetTomorrowCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).SetDate("tomorrow")));
    public static readonly ICommand SetNextWeekCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).SetDate("nextweek")));
    public static readonly ICommand SelectDateCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).SelectDate()));
    public static readonly ICommand ClearDateCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).ClearDate()));
    public static readonly ICommand CompleteCommand = (ICommand) new RelayCommand((Action<object>) (o => ((ChecklistItemControl) o).ToggleTaskCompleted()));
  }
}
