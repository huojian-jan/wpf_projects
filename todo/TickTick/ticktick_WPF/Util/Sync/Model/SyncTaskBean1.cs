// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncTaskBean1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.ObjectModel;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncTaskBean1
  {
    public ObservableCollection<TaskModel> update { get; set; }

    public ObservableCollection<TaskModel> add { get; set; }

    public ObservableCollection<TaskDeleteModel> delete { get; set; }

    public ObservableCollection<TaskDeleteModel> deletedInTrash { get; set; }

    public ObservableCollection<TaskDeleteModel> deletedForever { get; set; }
  }
}
