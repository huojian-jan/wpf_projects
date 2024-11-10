// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.IKanban
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public interface IKanban
  {
    event EventHandler<List<string>> SetSelected;

    void AddColumn(string columnId, HorizontalDirection direction);

    void DeleteColumn(string columnId, bool prompt);

    void StartDragTask(string columnId, DisplayItemModel model);

    void Reload(bool forceReload = false, bool restoreSelete = false);

    DisplayItemModel GetDraggingTask();

    void DropTaskInColumn(DisplayItemModel taskId, ColumnViewModel dropColumn);

    int GetColumnCount();

    Task<List<string>> GetColumnNames();

    void SetInOperation();

    void CancelOperation();

    bool IsInDragging();

    string GetProjectId();

    bool IsTaskPopupOpened();

    void ClearSelected();

    KanbanViewModel GetViewModel();

    void Toast(string text);
  }
}
