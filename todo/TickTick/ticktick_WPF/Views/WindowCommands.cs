// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.WindowCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public static class WindowCommands
  {
    public static readonly ICommand EscCommand = (ICommand) new RelayCommand((Action<object>) (o => ((Window) o).Close()));
  }
}
