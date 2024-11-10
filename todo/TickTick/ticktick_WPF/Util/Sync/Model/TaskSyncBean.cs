// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.TaskSyncBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class TaskSyncBean
  {
    [JsonProperty(PropertyName = "add")]
    public List<TaskModel> Added { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "update")]
    public List<TaskModel> Updated { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "updating")]
    public List<TaskModel> Updating { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "deletedInTrash")]
    public List<TaskModel> DeletedInTrash { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "deletedForever")]
    public List<TaskModel> DeletedForever { get; set; } = new List<TaskModel>();

    public bool Empty
    {
      get
      {
        return this.Added.Count == 0 && this.Updated.Count == 0 && this.Updating.Count == 0 && this.DeletedInTrash.Count == 0 && this.DeletedForever.Count == 0;
      }
    }

    public List<string> GetIds()
    {
      List<string> ids = new List<string>();
      if (this.Added.Any<TaskModel>())
        ids.AddRange(this.Added.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      if (this.Updated.Any<TaskModel>())
        ids.AddRange(this.Updated.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      if (this.Updating.Any<TaskModel>())
        ids.AddRange(this.Updating.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      if (this.DeletedInTrash.Any<TaskModel>())
        ids.AddRange(this.DeletedInTrash.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      if (this.DeletedForever.Any<TaskModel>())
        ids.AddRange(this.DeletedForever.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      return ids;
    }
  }
}
