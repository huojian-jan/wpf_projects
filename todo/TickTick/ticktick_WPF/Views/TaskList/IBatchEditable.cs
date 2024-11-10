// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.IBatchEditable
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public interface IBatchEditable
  {
    void ShowBatchOperationDialog();

    void SetSelectedTaskIds(List<string> taskIds);

    void RemoveSelectedId(string id);

    List<string> GetSelectedTaskIds();

    void ReloadList();

    UIElement BatchOperaPlacementTarget();
  }
}
