// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class CalendarCommands
  {
    public static readonly ICommand PrintCommand = (ICommand) new RelayCommand((Action<object>) (o => ((CalendarControl) o).Print(false)));
    public static readonly ICommand PrintAllCommand = (ICommand) new RelayCommand((Action<object>) (o => ((CalendarControl) o).Print(true)));
  }
}
