﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncTaskParentRes
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncTaskParentRes
  {
    private Dictionary<string, string> id2error = new Dictionary<string, string>();
    private Dictionary<string, SyncTaskParentEtag> id2etag = new Dictionary<string, SyncTaskParentEtag>();

    [JsonProperty(PropertyName = "id2etag")]
    public Dictionary<string, SyncTaskParentEtag> Id2etag
    {
      get => this.id2etag;
      set => this.id2etag = value;
    }

    [JsonProperty(PropertyName = "id2error")]
    public Dictionary<string, string> Id2error
    {
      get => this.id2error;
      set => this.id2error = value;
    }
  }
}