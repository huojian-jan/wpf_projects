// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.DetailWindowCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public static class DetailWindowCommands
  {
    public static readonly ICommand OpenAsSticky = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailWindow) o).OpenAsSticky()));
  }
}
