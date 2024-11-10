// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncProjectGroupBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncProjectGroupBean
  {
    private List<ProjectGroupModel> update = new List<ProjectGroupModel>();
    private List<ProjectGroupModel> add = new List<ProjectGroupModel>();
    private Collection<string> deleted = new Collection<string>();

    [JsonProperty(PropertyName = "update")]
    public List<ProjectGroupModel> Update
    {
      get => this.update;
      set => this.update = value;
    }

    [JsonProperty(PropertyName = "add")]
    public List<ProjectGroupModel> Add
    {
      get => this.add;
      set => this.add = value;
    }

    [JsonProperty(PropertyName = "deleted")]
    public Collection<string> Deleted
    {
      get => this.deleted;
      set => this.deleted = value;
    }

    public bool Empty => this.Add.Count == 0 && this.Deleted.Count == 0 && this.Update.Count == 0;
  }
}
