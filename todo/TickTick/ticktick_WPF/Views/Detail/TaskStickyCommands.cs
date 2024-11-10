// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskStickyCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public static class TaskStickyCommands
  {
    public static readonly ICommand CreatNewCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("task", "add task");
      ((TaskStickyWindow) o).CreateNew();
    }));
    public static readonly ICommand SetColorCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "color_sticky");
      ((TaskStickyWindow) o).QuickChangeColor();
    }));
    public static readonly ICommand CloseCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskStickyWindow) o).TryClose()));
    public static readonly ICommand PinCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "pin_sticky");
      ((TaskStickyWindow) o).Pin();
    }));
    public static readonly ICommand CompleteCommand = (ICommand) new RelayCommand((Action<object>) (o => ((TaskStickyWindow) o).CompleteTaskCommand()));
    public static ICommand StickyCollapseCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "toggle_all_sticky");
      ((TaskStickyWindow) o).CollapseOrExpandAll();
    }));
    public static ICommand StickyAlignTopCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "arrange_by_top");
      ((TaskStickyWindow) o).Align(false, false);
    }));
    public static ICommand StickyAlignLeftCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "arrange_by_left");
      ((TaskStickyWindow) o).Align(true, false);
    }));
    public static ICommand StickyAlignRightCommand = (ICommand) new RelayCommand((Action<object>) (o =>
    {
      UserActCollectUtils.AddShortCutEvent("edit_sticky_note", "arrange_by_right");
      ((TaskStickyWindow) o).Align(true, true);
    }));
  }
}
