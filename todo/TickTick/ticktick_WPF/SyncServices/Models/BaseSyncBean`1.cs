// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.SyncServices.Models.BaseSyncBean`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.SyncServices.Models
{
  public class BaseSyncBean<T>
  {
    public BaseSyncBean()
    {
    }

    public BaseSyncBean(ICollection<T> add = null, ICollection<T> update = null, ICollection<string> delete = null)
    {
      if (add != null)
        this.Add = add.ToList<T>();
      if (update != null)
        this.Update = update.ToList<T>();
      if (delete == null)
        return;
      this.Delete = delete.ToList<string>();
    }

    public BaseSyncBean(IEnumerable<T> add = null, IEnumerable<T> update = null, IEnumerable<string> delete = null)
    {
      if (add != null)
        this.Add = add.ToList<T>();
      if (update != null)
        this.Update = update.ToList<T>();
      if (delete == null)
        return;
      this.Delete = delete.ToList<string>();
    }

    [JsonProperty("add")]
    public List<T> Add { get; set; } = new List<T>();

    [JsonProperty("update")]
    public List<T> Update { get; set; } = new List<T>();

    [JsonProperty("delete")]
    public List<string> Delete { get; set; } = new List<string>();

    public bool Any() => this.HasAdd || this.HasUpdate || this.HasDelete;

    public int Count() => this.Add.Count + this.Update.Count + this.Delete.Count;

    public int AddOrUpdateCount() => this.Add.Count + this.Update.Count;

    public int DeleteCount() => this.Delete.Count;

    [JsonIgnore]
    public bool HasAdd => this.Add.Any<T>();

    [JsonIgnore]
    public bool HasUpdate => this.Update.Any<T>();

    [JsonIgnore]
    public bool HasDelete => this.Delete.Any<string>();
  }
}
