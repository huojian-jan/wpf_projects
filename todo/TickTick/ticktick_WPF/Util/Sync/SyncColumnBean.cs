// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncColumnBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncColumnBean
  {
    private List<ColumnModel> update = new List<ColumnModel>();
    private List<ColumnModel> add = new List<ColumnModel>();
    private List<ColumnProjectModel> deleted = new List<ColumnProjectModel>();

    [JsonProperty(PropertyName = "update")]
    public List<ColumnModel> Update
    {
      get => this.update;
      set => this.update = value;
    }

    [JsonProperty(PropertyName = "add")]
    public List<ColumnModel> Add
    {
      get => this.add;
      set => this.add = value;
    }

    [JsonProperty(PropertyName = "delete")]
    public List<ColumnProjectModel> Deleted
    {
      get => this.deleted;
      set => this.deleted = value;
    }
  }
}
