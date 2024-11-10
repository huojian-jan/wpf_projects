// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.IKanbanColumn
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public interface IKanbanColumn
  {
    bool TaskChanged { get; set; }

    string GetColumnId();

    void ReloadColumn();

    List<DisplayItemModel> RemoveTasks(List<string> selected, string except);

    void RemoveTaskChildren(DisplayItemModel model);
  }
}
