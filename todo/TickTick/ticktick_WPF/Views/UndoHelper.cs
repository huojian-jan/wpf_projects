// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.UndoHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views
{
  public static class UndoHelper
  {
    public static string NeedToastFilteredTaskId;

    public static List<string> DeletingIds { get; } = new List<string>();

    public static BlockingSet<string> CanUndoIds { get; } = new BlockingSet<string>();

    public static BlockingSet<string> UndoingIds { get; } = new BlockingSet<string>();
  }
}
