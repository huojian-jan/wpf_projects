﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncFilterBean
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
  public class SyncFilterBean
  {
    [JsonProperty(PropertyName = "update")]
    public List<FilterModel> Update { get; set; } = new List<FilterModel>();

    [JsonProperty(PropertyName = "add")]
    public List<FilterModel> Add { get; set; } = new List<FilterModel>();

    [JsonProperty(PropertyName = "delete")]
    public Collection<string> Deleted { get; set; } = new Collection<string>();

    public bool Empty => this.Add.Count == 0 && this.Update.Count == 0 && this.Deleted.Count == 0;
  }
}